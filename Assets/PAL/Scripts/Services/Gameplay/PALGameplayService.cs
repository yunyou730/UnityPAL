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
        private int _nextSpriteEntityId = 1;
        private Dictionary<int,SpriteEntity> _spriteEntitiesMap = new Dictionary<int,SpriteEntity>();

        private MapPresenter _mapPresenter = null;

        private GameStateDataService _gameStateDataService = null;
        private PaletteService _paletteService = null;

        public void Init()
        {
            _gameStateDataService = PalGame.GetInstance().GetService<GameStateDataService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            
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
            for (int i = 0;i <= _gameStateDataService.MaxPartyMemberIndex;i++)
            {
                var party = _gameStateDataService.GetPlayerParty(i);
                int roleId = party.RoleId;
                int spriteId = _gameStateDataService.GetSpriteIdByRoleId(roleId);
                var spriteEntity = CreateSpriteEntity(spriteId);
                _spriteEntitiesMap.Add(spriteEntity.ID, spriteEntity);
            }
        }

        private SpriteEntity CreateSpriteEntity(int spriteId)
        {
            int entityId = _nextSpriteEntityId++;
            var spriteEntity = new SpriteEntity(entityId,spriteId);
            return spriteEntity;
        }
        
        private void UpdateSpriteEntities()
        {
            // @miao @todo
            
        }
    }
    
    
}

