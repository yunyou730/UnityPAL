using System.IO;
using System.Net.Mime;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class ConfigDataService : Service,IInitializable,IDestroyable
    {
        private MKFLoader _data = null;
        public void Init()
        {
            _data = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "DATA.MKF"));
        }
        
        public void Destroy()
        {
            
        }
    }
}

