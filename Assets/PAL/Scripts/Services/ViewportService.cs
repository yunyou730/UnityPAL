using System.Collections.Generic;
using System.IO;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class ViewportService : Service,IInitializable,IDestroyable
    {
        private PALViewport _palViewport = null;
        private GameObject _prefab = null;
        public ViewportService(GameObject prefab)
        {
            _prefab = prefab;
        }

        public void Init()
        {
            _palViewport = GameObject.Instantiate(_prefab).GetComponent<PALViewport>();
            _palViewport.name = "[PAL-Viewport]";
        }

        public void Destroy()
        {
            
        }

        // public void RefreshCoord(int pixelX, int pixelY)
        // {
        //     _palViewport.RefreshCoord(pixelX, pixelY);
        // }

        public PALViewport GetViewport()
        {
            return _palViewport;
        }

    }
}

