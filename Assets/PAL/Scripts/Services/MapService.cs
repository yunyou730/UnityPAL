using ayy.pal.core;

namespace ayy.pal
{
    public class MapServices : Service,IInitializable,IDestroyable
    {
        private Map _map = null;
        private MapWrapper _currentMap = null;
        public void Init()
        {
            _map = new Map();
            _map.Load();
        }
        public void Destroy()
        {
            
        }

        public Map GetMapManager()
        {
            return _map;
        }

        public void LoadMap(int mapIndex)
        {
            if (_currentMap != null)
            {
                _currentMap.Dispose();
                _currentMap = null;
            }
            _currentMap = new MapWrapper(_map,mapIndex);
            _currentMap.Load();
        }

        public MapWrapper GetCurrentMap()
        {
            return _currentMap;
        }
    }
}

