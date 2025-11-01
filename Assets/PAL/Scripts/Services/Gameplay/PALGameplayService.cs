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
        // 管理所有 加载过的 SpriteEntity
        private int _nextSpriteEntityKey = 1;
        private Dictionary<int,SpriteEntity> _spriteEntitiesMap = new Dictionary<int,SpriteEntity>();
        
        // 管理 地图 MapPresenter
        private MapPresenter _mapPresenter = null;
        
        // 主角所对应的 SpriteEntities
        private List<int> _partySpriteEntityKeys = new List<int>();
        
        // 地图 npc 所对应的 SpriteEntities
        // @miao @todo
        
        // Services
        private GameStateDataService _gameStateDataService = null;
        private PaletteService _paletteService = null;
        private ViewportService _viewportService = null;

        public void Init()
        {
            _gameStateDataService = PalGame.GetInstance().GetService<GameStateDataService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _viewportService = PalGame.GetInstance().GetService<ViewportService>();
            
            LoadPalette();
            CreateMapEntity();
            CreatePartySpriteEntities();
        }

        public void Destroy()
        {
            // cleanup sprites
            if (_spriteEntitiesMap != null)
            {
                foreach (var spriteEntity in _spriteEntitiesMap.Values)
                {
                    spriteEntity.Dispose();
                }
                _spriteEntitiesMap.Clear();
            }
            
            // cleanup map
            if (_mapPresenter != null)
            {
                GameObject.Destroy(_mapPresenter.gameObject);
                _mapPresenter = null;
            }
        }

        public void Update()
        {
            UpdateViewport();
            UpdateSpriteEntities();
        }

        public MapPresenter GetMapPresenter()
        {
            return _mapPresenter;
        }
        
        private void LoadPalette()
        {
            int paletteId = _gameStateDataService.CurrentPaletteId;
            bool useNight = _gameStateDataService.UseNightPalette;
            _paletteService.LoadPalette(paletteId,useNight);
        }

        private void CreateMapEntity()
        {
            int mapId = _gameStateDataService.SceneId;
            if (_mapPresenter == null)
            {
                var mapPrefab = PalGame.GetInstance().GetMapPrefab();
                var mapGameObject = GameObject.Instantiate(mapPrefab);
                mapGameObject.name = $"[PAL]map:{mapId}";
                _mapPresenter = mapGameObject.GetComponent<MapPresenter>();
                _mapPresenter.Load(mapId);
            }
        }

        private void CreatePartySpriteEntities()
        {
            _partySpriteEntityKeys.Clear();
            // 创建主角 party 的 sprite
            for (int i = 0;i <= _gameStateDataService.MaxPartyMemberIndex;i++)
            {
                Party party = _gameStateDataService.GetPlayerParty(i);
                int roleId = party.RoleId;
                int spriteId = _gameStateDataService.GetSpriteIdByRoleId(roleId);
                var spriteEntity = CreateSpriteEntity(spriteId);
                _spriteEntitiesMap.Add(spriteEntity.Key, spriteEntity);
                
                // 记录 主角Party 和 SpriteEntity 对应关系 
                _partySpriteEntityKeys.Add(spriteEntity.Key);
            }
        }

        private SpriteEntity CreateSpriteEntity(int spriteId)
        {
            int spriteEntityKey = _nextSpriteEntityKey++;
            var spriteEntity = new SpriteEntity(spriteEntityKey,spriteId);
            return spriteEntity;
        }

        private void UpdateViewport()
        {
            int viewportX = _gameStateDataService.ViewportX;
            int viewportY = _gameStateDataService.ViewportY;
            _viewportService.GetViewport().RefreshCoord(viewportX, viewportY);
        }

        private void UpdateSpriteEntities()
        {
            int viewportX = _gameStateDataService.ViewportX;
            int viewportY = _gameStateDataService.ViewportY;
            // sprite entity pos
            for (int i = 0; i <= _gameStateDataService.MaxPartyMemberIndex; i++)
            {
                Party party = _gameStateDataService.GetPlayerParty(i);
                int spriteEntityKey = _partySpriteEntityKeys[i];
                var spriteEntity = _spriteEntitiesMap[spriteEntityKey];
                int layer = _gameStateDataService.AtLayer;

                PALSpriteFrame spriteFrame = spriteEntity.SwitchFrame(party.FrameIndex);
                int pixelX = party.PixelX - spriteFrame.W / 2;
                int pixelY = party.PixelY + layer + 10; // hard code +10, 需要抽象为 枚举
                spriteEntity.SetPixelPosition(pixelX,pixelY);
                spriteEntity.SetLayer(layer + 6);   // hard code + 6, 需要抽象为枚举
                spriteEntity.ApplyPixelPos(viewportX,viewportY);
            }
        }
    }
}

