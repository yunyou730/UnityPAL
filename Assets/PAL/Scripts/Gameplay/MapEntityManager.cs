using System.Collections.Generic;
using UnityEngine;

namespace ayy.pal
{
    // 管理所有 SpriteEntity
    public class MapEntityManager : Service,IInitializable,IDestroyable
    {
        // 管理 地图 MapPresenter
        private MapPresenter _mapPresenter = null;
        
        public void Init()
        {
            var mapPrefab = PalGame.GetInstance().GetMapPrefab();
            var mapGameObject = GameObject.Instantiate(mapPrefab);
            _mapPresenter = mapGameObject.GetComponent<MapPresenter>();
            mapGameObject.name = "[PAL]Map";
        }

        public void Destroy()
        {
            if (_mapPresenter != null)
            {
                GameObject.Destroy(_mapPresenter.gameObject);
                _mapPresenter = null;
            }
        }

        public void SwitchMapById(int mapId)
        {
            if (_mapPresenter != null && _mapPresenter.GetMapIndex() != mapId)
            {
                _mapPresenter.Unload();
                _mapPresenter.Load(mapId);
                _mapPresenter.gameObject.name = $"[PAL]Map[{mapId}]";
            }
        }

        public void ToggleDisplayTileInfo(bool display)
        {
            if (_mapPresenter != null)
            {
                _mapPresenter.ToggleDebugTileInfo(display);
            }
        }
    }
}

