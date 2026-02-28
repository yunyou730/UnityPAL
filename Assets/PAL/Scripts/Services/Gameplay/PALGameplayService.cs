using System.Collections.Generic;
using ayy.pal.core;
using Unity.VisualScripting;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 PAL_MakeScene
     */
    public class PALGameplayService : Service,IInitializable,IDestroyable,IUpdateable
    {
        static private int kLogicFPS = 10;
        private float _deltaTimeCounter = 0.0f;

        private Camera _mainCamera = null;
        
        // 主角所对应的 SpriteEntities
        private List<int> _partySpriteEntityKeys = new List<int>();
        
        // 地图 npc 所对应的 SpriteEntities
        // @miao @todo
        
        // 参考 PAL_UpdatePartyGestures(), 用于把最后的
        private int _partyStepFrameIndex = 0;
        
        // Services
        private GameStateDataService _dataService = null;
        private PaletteService _paletteService = null;
        private ViewportService _viewportService = null;
        private MapService _mapService = null;
        
        private SpriteEntityManager _spriteEntityManager = null;
        private MapEntityManager _mapEntityManager = null;
        private InputManager _inputManager = null;
        
        
        // 统计出来,所有 覆盖了 sprite 的 tiles
        private List<MapTileCoord> _coverSpriteTileCoords = new List<MapTileCoord>();

        public PALGameplayService(Camera mainCamera)
        {
            _mainCamera = mainCamera;
        }

        public void Init()
        {
            _dataService = PalGame.GetInstance().GetService<GameStateDataService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _viewportService = PalGame.GetInstance().GetService<ViewportService>();
            _mapService = PalGame.GetInstance().GetService<MapService>();

            _spriteEntityManager = PalGame.GetInstance().GetService<SpriteEntityManager>();
            _mapEntityManager = PalGame.GetInstance().GetService<MapEntityManager>();
            _inputManager = PalGame.GetInstance().GetService<InputManager>();
            
            LoadPalette();
            CreateMapEntity();
            CreatePartySpriteEntities();
        }

        public void Destroy()
        {
        }

        public void Update()
        {
            _deltaTimeCounter += Time.deltaTime;
            if (_deltaTimeCounter > 1f / kLogicFPS)
            {
                _deltaTimeCounter -= 1f / kLogicFPS;
                OnTick();
                
                //FrameRefresh(); // 试试看, FPS10,能不能行?
            }
            FrameRefresh();
        }

        private void OnTick()
        {
            bool isInputThisFrame = _inputManager.GetInputDir() != EPALDirection.Unknown;

            TickUpdateParty();
            TickUpdatePartyGestures(isInputThisFrame);
        }


        private void FrameRefresh()
        {
            // @miao @todo
            //. 注意！！这里一致频繁修改 mesh，从mesh 里获取colors 数组, 有大的性能问题!
            // 可以先把 逻辑写对, 后面解决一下这个逻辑!!
            ClearCoverFrameTiles();
            UpdateViewport();
            UpdateCameraFollowViewport();
            UpdateSpriteEntities();
            ApplyCoverFrameTiles();
        }

        private void LoadPalette()
        {
            int paletteId = _dataService.CurrentPaletteId;
            bool useNight = _dataService.UseNightPalette;
            _paletteService.LoadPalette(paletteId,useNight);
        }

        private void CreateMapEntity()
        {
            int mapId = _dataService.SceneId;
            _mapEntityManager.SwitchMapById(mapId);
        }

        private void CreatePartySpriteEntities()
        {
            _partySpriteEntityKeys.Clear();
            // 创建主角 party 的 sprite
            for (int i = 0;i <= _dataService.MaxPartyMemberIndex;i++)
            {
                Party party = _dataService.GetPlayerParty(i);
                int roleId = party.RoleId;
                int spriteId = _dataService.GetSpriteIdByRoleId(roleId);
                var spriteEntityKey = _spriteEntityManager.CreateSpriteEntity(spriteId);
                
                // 记录 主角Party 和 SpriteEntity 对应关系 
                _partySpriteEntityKeys.Add(spriteEntityKey);
            }
        }
        
        // 参考 scene.c PAL_UpdateParty()
        private bool TickUpdateParty()
        {
            EPALDirection inputDir = _inputManager.GetInputDir();
            if (inputDir == EPALDirection.Unknown)
            {
                return false;
            }
            // 让队伍转向
            _dataService.PartyDirection = inputDir;
            
            int xOffset = (inputDir == EPALDirection.West || inputDir == EPALDirection.South) ? -16 : 16;
            int yOffset = ((inputDir == EPALDirection.West || inputDir == EPALDirection.North) ? -8 : 8);

            // source & target, todo
            int xSource = _dataService.ViewportX + _dataService.PartyOffsetX;
            int ySource = _dataService.ViewportY + _dataService.PartyOffsetY;
            int xTarget = xSource + xOffset;
            int yTarget = ySource + yOffset;
            
            // @miao @todo, 这里应该调用 PAL_CheckObstacle

            // Move the viewport
            int prevX = _dataService.ViewportX;
            int prevY = _dataService.ViewportY;
            int nextX = prevX + xOffset;
            int nextY = prevY + yOffset;
            _dataService.SetViewportXY(nextX, nextY);

            return true;
        }

        // 参考 PAL_UpdatePartyGestures
        void TickUpdatePartyGestures(bool walking)
        {
            if (walking)
            {
                TickUpdatePartyGesturesForWalking();
            }
            else
            {
                TickUpdatePartyGesturesForStanding();
            }
        }
        
        // 参考 PAL_UpdatePartyGestures, walking = true
        private void TickUpdatePartyGesturesForWalking()
        {
            int stepFrameLeaderIndex = 0;
            int stepFrameFollowerIndex = 0;
            
            _partyStepFrameIndex = (_partyStepFrameIndex + 1) % 4;  // 走路动画有4帧
            if ((_partyStepFrameIndex & 1) != 0)
            {
                stepFrameLeaderIndex = (_partyStepFrameIndex + 1) / 2;
                stepFrameFollowerIndex = 3 - stepFrameLeaderIndex;
            }
            else
            {
                stepFrameLeaderIndex = 0;
                stepFrameFollowerIndex = 0;
            }

            // 更新 leaer,party0 的坐标
            Party p0 = _dataService.GetPlayerParty(0);
            p0.PixelX = _dataService.PartyOffsetX;
            p0.PixelY = _dataService.PartyOffsetY;
            
            // 更新 leader, party0 的 frame index
            int roleId = p0.RoleId;
            PALPlayerRole playerRole = _dataService.GameData.PlayerRoles[roleId];
            
            if(playerRole.GetWalkFramesCount() == 4) // 看这个 role 的走路动画, 一共有4 帧,还是3 帧
            {
                p0.FrameIndex = (int)_dataService.PartyDirection * 4 + _partyStepFrameIndex;
            }
            else
            {
                p0.FrameIndex = (int)_dataService.PartyDirection * 3 + stepFrameLeaderIndex;
            }
            
            // 更新 party 里, 除了leader 之外, 的 frame index 和 position .
            // todo
            
        }

        // 参考 PAL_UpdatePartyGestures, walking = false
        private void TickUpdatePartyGesturesForStanding()
        {
            // 主角朝向 frame
            Party p0 = _dataService.GetPlayerParty(0);
            PALPlayerRole r0 = _dataService.GameData.PlayerRoles[p0.RoleId];
            int walkFrames = r0.GetWalkFramesCount(); // 走路动画有多少帧, 如果是0 帧, 则修正为默认3帧
            p0.FrameIndex = (int)_dataService.PartyDirection * walkFrames;

            // 队伍朝向,todo
            
            // 走路frame 更新
            _partyStepFrameIndex &= 2;
            _partyStepFrameIndex ^= 2;
        }

        private void UpdateViewport()
        {
            int viewportX = _dataService.ViewportX;
            int viewportY = _dataService.ViewportY;
            _viewportService.SetPixelCoord(viewportX, viewportY);
        }

        private void UpdateCameraFollowViewport()
        {
            Vector3 viewportWorldPos = _viewportService.GetViewportWorldPosition();
            Vector3 pos = new Vector3(viewportWorldPos.x, viewportWorldPos.y, PalConst.CAMERA_DEFAULt_Z);
            _mainCamera.transform.position = pos;
        }

        // 用于处理 tiles 和 sprite 的遮挡关系, 还原所有 tiles 的 z值
        private void ClearCoverFrameTiles()
        {
            MapWrapper mapWrapper = _mapService.GetCurrentMap();
            foreach (MapTileCoord tileCoord in _coverSpriteTileCoords)
            {
                mapWrapper.SetTileVertexColor(ETileLayer.Top,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.black);
                mapWrapper.SetTileVertexColor(ETileLayer.Bottom,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.black);
            }
            _coverSpriteTileCoords.Clear();
        }

        // 用于处理 tiles 和 sprite 的遮挡关系, 拔高所有 tiles 的 z值
        private void ApplyCoverFrameTiles()
        {
            MapWrapper mapWrapper = _mapService.GetCurrentMap();
            foreach (MapTileCoord tileCoord in _coverSpriteTileCoords)
            {
                mapWrapper.SetTileVertexColor(ETileLayer.Top,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.yellow);
                mapWrapper.SetTileVertexColor(ETileLayer.Bottom,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.yellow);
            }
            mapWrapper.ApplyTileVertexColorsChange();
        }

        private void UpdateSpriteEntities()
        {
            int viewportX = _dataService.ViewportX;
            int viewportY = _dataService.ViewportY;
            // sprite entity pos
            for (int i = 0; i <= _dataService.MaxPartyMemberIndex; i++)
            {
                Party party = _dataService.GetPlayerParty(i);
                int spriteEntityKey = _partySpriteEntityKeys[i];
                SpriteEntity spriteEntity = _spriteEntityManager.GetSpriteEntity(spriteEntityKey);
                int layer = _dataService.AtLayer;

                PALSpriteFrame spriteFrame = spriteEntity.SwitchFrame(party.FrameIndex);
                int pixelX = party.PixelX - spriteFrame.W / 2;
                int pixelY = party.PixelY + layer + 10; // hard code +10, 需要抽象为 枚举
                spriteEntity.SetPixelPosition(pixelX,pixelY);
                spriteEntity.SetLayer(layer + 6);   // hard code + 6, 需要抽象为枚举
                spriteEntity.ApplyPixelPos(viewportX,viewportY);

                
                // 计算所有,可能遮挡该 sprite 的 tiles
                UpdateTilesSpriteOcclusion(viewportX,viewportY,pixelX,pixelY,layer,spriteFrame);
            }
        }

        private void UpdateTilesSpriteOcclusion(int viewportX,int viewportY,int pixelX,int pixelY,int layer,PALSpriteFrame spriteFrame)
        {
            // 计算所有,可能遮挡该 sprite 的 tiles
            // @miao @todo
            // DOS 世界空间下, 像素坐标
            int worldPixelX = viewportX + pixelX - layer / 2;
            int worldPixelY = viewportY + pixelY - layer;
            
            // MapTileCoord testMapCoord;
            // Metrics.ConvertWorldSpacePixelCoordToTileCoord(worldPixelX,worldPixelY,out testMapCoord);
            // _coverSpriteTileCoords.Add(testMapCoord);
            
            MapTileCoord mapCoord1;
            Metrics.ConvertWorldSpacePixelCoordToTileCoord(worldPixelX - spriteFrame.W / 2,worldPixelY - spriteFrame.H,out mapCoord1);

            MapTileCoord mapCoord2;
            Metrics.ConvertWorldSpacePixelCoordToTileCoord(worldPixelX + spriteFrame.W / 2,worldPixelY,out mapCoord2);
            
            for (int ty = mapCoord1.TileY; ty <= mapCoord2.TileY; ty++)
            {
                for (int tx = mapCoord1.TileX; tx <= mapCoord2.TileX; tx++)
                {
                    MapTileCoord mapCoordTmp = new MapTileCoord();
                    mapCoordTmp.TileX = tx;
                    mapCoordTmp.TileY = ty;
                    //mapCoordTmp.TileH = mapCoord0.TileH;
                    mapCoordTmp.TileH = 0;
                    _coverSpriteTileCoords.Add(mapCoordTmp);
                    
                    mapCoordTmp.TileH = 1;
                    _coverSpriteTileCoords.Add(mapCoordTmp);
                }
            }
        }

    }
}

