using ayy.pal.core;

namespace ayy.pal
{
    public class MapService : Service,IInitializable,IDestroyable
    {
        private PALMapWrapper _map = null;
        private MapWrapper _currentMap = null;
        public void Init()
        {
            _map = new PALMapWrapper();
            _map.Load();
        }
        
        public void Destroy()
        {
            
        }

        public PALMapWrapper GetMapWrapper()
        {
            return _map;
        }

        public void LoadMap(int mapIndex)
        {
            UnloadCurrentMap();
            _currentMap = new MapWrapper(_map,mapIndex);
            _currentMap.Load(EColorMode.PaletteLUT);
        }

        public void UnloadCurrentMap()
        {
            if (_currentMap != null)
            {
                _currentMap.Dispose();
                _currentMap = null;
            }
        }

        public MapWrapper GetCurrentMap()
        {
            return _currentMap;
        }
    }
}

