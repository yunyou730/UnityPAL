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
        
        // 用于做 渲染排序
        private RenderOrderManager _renderOrderManager = null;

        public PALGameplayService(Camera mainCamera)
        {
            _mainCamera = mainCamera;
            _renderOrderManager = new RenderOrderManager();
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
            PALMap palMap = _mapService.GetCurrentMap().GetPalMap();
            MapWrapper mapWrapper = _mapService.GetCurrentMap();
            
            _renderOrderManager.ClearRenderOrder(mapWrapper); // @miao @todo. 待重构
            
            UpdateViewport();
            UpdateCameraFollowViewport();
            UpdateSpriteEntities();

            
            int viewportX = _dataService.ViewportX;
            int viewportY = _dataService.ViewportY;
            _renderOrderManager.SortRenderOrder(palMap,mapWrapper,viewportX,viewportY);
            _renderOrderManager.ApplyRenderOrder(palMap,mapWrapper);
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
            
            
            /*
             * 这里这么理解:
             * 1. 对于2D像素坐标 x:
             *      如果按左（west) or 下(south) ,则 横向偏移 -16 (半个 tile width)
             *      否则, 即按了 右 （east） or 上(north), 则 横向偏移 + 16 (半个 tile width)
             *
             * 2. 对于 2D像素坐标 y:
             *      如果按了 左(west) or 上(north), 则 纵向偏移 -8  (半个 tile height)
             *      否则, 即 按了 右(east) or 下 (south), 则 纵向偏移 8 (半个 tile height)
             * 
             *      注意，在像素坐标系里, viewport 的坐标, sprite 的坐标, 都是左上角;
             *      并且, y坐标, 是向下递增的
             *      具体参考 飞书文档 , 坐标系 的 解释
             *      https://o2oh6846tj.feishu.cn/wiki/Dyn2wM929iSrVPkPLRdcoNXandd#share-SCaVdqYP7oeNARxkrqmcCh8Enjh
             * 
             * 
             * 3. 移动偏移, 只作用于 viewport 的坐标
             *      因为 整个队伍 party 里每个 sprite 的坐标定位
             *      是依赖于 viewport.xy ,即 viewport 的像素坐标 , 加上 party Offset .xy ,也就是 sprite 的像素坐标偏移
             *      这样最终定位 世界坐标系下的 像素坐标的
             *
             * 4. 
             * 
             */
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
        
		/*
			参考 SDLPal scene.c  PAL_SceneDrawSprites 方法.
			代码里的 layer + 10, layer + 6 是对 player 也就是 主角队伍的 sprite 的特殊处理

			里面在调用 PAL_AddSpriteToDraw 的时候,
			在遍历绘制 party member, 即 玩家操作的队伍 时, 有 wLayer + 10, wLayer + 6 这种操作
			wLayer 是用户存档, 或者说是当前玩家队伍, 正在进行的 gameplay 里的一个数值 

      		PAL_AddSpriteToDraw(lpBitmap,
         		gpGlobals->rgParty[i].x - PAL_RLEGetWidth(lpBitmap) / 2,
         		gpGlobals->rgParty[i].y + gpGlobals->wLayer + 10,
         		gpGlobals->wLayer + 6);

			PAL_AddSpriteToDraw 做了几件事
			1. 收集  party member 的 sprites ,以及对应的 cover tiles
			2. 收集 Monsters/Npcs/others 的 sprites, 以及对应的 cover tiles
			3. 给所有 sprites 排序 
			4. 绘制所有 sprite 
			
		*/
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
                
                
                // @miao @todo
                // 这里,直接设置 sprite Entity 的 z值
                _renderOrderManager.CollectSpriteNode(spriteEntity);
                
                // // 这里,直接设置 sprite Entity 的 z值
                // spriteEntity.ApplyPixelPos(viewportX,viewportY);
                
                
                
                // 在下面的函数中, 也直接设置 ,可能遮挡 sprite 的tiles 的 z值
                UpdateSpriteCoverTiles(viewportX, viewportY, spriteEntity);
            }
        }
        
        /*
         * 参考 SDLPAL scene.c PAL_CalcCoverTiles() 方法 
         * 计算所有,可能遮挡该 sprite 的 tiles
         */
        private void UpdateSpriteCoverTiles(
            int viewportX,
            int viewportY,
            SpriteEntity spriteEntity)
        {
            int pixelX,pixelY;
            spriteEntity.GetPixelPos(out pixelX,out pixelY);
            
            int layer = spriteEntity.GetLogicLayer();
            PALSpriteFrame spriteFrame = spriteEntity.GetCurrentSpriteFrame();
            
            // 计算所有,可能遮挡该 sprite 的 tiles
            // DOS 世界空间下, 像素坐标
            int sx = viewportX + pixelX - layer / 2;
            int sy = viewportY + pixelY - layer;
            int sh = (sx % 32 != 0) ? 1 : 0;   // 看 h是 0, 还是1 

            int width = spriteFrame.W;
            int height = spriteFrame.H;

            int dx = 0;
            int dy = 0;
            int dh = 0;

            //Debug.Log("ayy-begin cover tiles");
            PALMap palMap = _mapService.GetCurrentMap().GetPalMap();
            // 这里, 具体覆盖哪些 tiles 这块, 说实话没看太懂. 先照着抄吧！
            // 但总之,目的就是, 把 sprite 有可能 cover到的 tiles,  给标记出来
            for (int y = (sy - height - 15) / 16; y <= sy / 16; y++)
            {
                for (int x = (sx - width / 2) / 32; x <= (sx + width / 2) / 32; x++)
                {
                    for (int i = ((x == (sx - width / 2) / 32) ? 0 : 3); i < 5; i++)
                    {
                        //
                        // Scan tiles in the following form (* = to scan):
                        //
                        // . . . * * * . . .
                        //  . . . * * . . . .
                        //
                        switch (i)
                        {
                            case 0:
                                dx = x;
                                dy = y;
                                dh = sh;
                                break;

                            case 1:
                                dx = x - 1;
                                break;

                            case 2:
                                dx = (sh == 1 ? x : (x - 1));
                                dy = (sh == 1 ? (y + 1) : y);
                                dh = 1 - sh;
                                break;

                            case 3:
                                dx = x + 1;
                                dy = y;
                                dh = sh;
                                break;

                            case 4:
                                dx = (sh == 1 ? (x + 1) : x);
                                dy = (sh == 1 ? (y + 1) : y);
                                dh = 1 - sh;
                                break;
                        }

                        // bottom
                        {
                            int logicHeight = palMap.GetMapTileLogicHeight(dx, dy, dh, ETileLayer.Bottom);
                            int tilePixelY = palMap.GetMapTilePixelYCoord(dx, dy, dh, ETileLayer.Bottom);
                            //Debug.Log($"tile (x:{dx},y:{dy},h:{dh},l:{ETileLayer.Bottom}) height = {logicHeight}");

                            // @miao @temp , for debugging
                            if (logicHeight > 0 && tilePixelY >= sy)
                            {
                                Debug.Log($"tile (x:{dx},y:{dy},h:{dh},l:{ETileLayer.Bottom}) height = {logicHeight}");
                                
                                MapTileCoord coord = new MapTileCoord();
                                coord.TileX = dx;
                                coord.TileY = dy;
                                coord.TileH = dh;
                                coord.TileLayer = ETileLayer.Bottom;
                                // _coverSpriteTileCoords.Add(coord);
                                _renderOrderManager.CollectTileNode(coord,logicHeight,viewportX,viewportY);
                            }
                        }

                        // top
                        {
                            int logicHeight = palMap.GetMapTileLogicHeight(dx, dy, dh, ETileLayer.Top);
                            int tilePixelY = palMap.GetMapTilePixelYCoord(dx, dy, dh, ETileLayer.Top);
                            //Debug.Log($"tile (x:{dx},y:{dy},h:{dh},l:{ETileLayer.Top}) height = {logicHeight}");
                            
                            // @miao @temp , for debugging
                            if (logicHeight > 0 && tilePixelY >= sy)
                            {
                                Debug.Log($"tile (x:{dx},y:{dy},h:{dh},l:{ETileLayer.Top}) height = {logicHeight}");
                                
                                MapTileCoord coord = new MapTileCoord();
                                coord.TileX = dx;
                                coord.TileY = dy;
                                coord.TileH = dh;
                                coord.TileLayer = ETileLayer.Top;
                                //_coverSpriteTileCoords.Add(coord);

                                _renderOrderManager.CollectTileNode(coord,logicHeight,viewportX,viewportY);
                            }
                        }
                                          
                    }
                }
            }
            //Debug.Log("ayy-end cover tiles");
        }
    }
}

