using System.Collections.Generic;
using System.IO;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class SpriteService : Service,IInitializable,IDestroyable
    {
        private MKFLoader _mgoMKF = null;
        private int _spriteCount = 0;
        private List<PALSprite> _sprites = null;
        
        public void Init()
        {
            _mgoMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "MGO.MKF"));
            _mgoMKF.Load();
            _spriteCount = _mgoMKF.GetChunkCount();
            _sprites = new List<PALSprite>(_spriteCount);
        }

        public void Destroy()
        {
            
        }

        public int GetSpriteCount()
        {
            return _spriteCount;
        }

        public MKFLoader GetMgoMKF()
        {
            return _mgoMKF;
        }

        public PALSprite GetSprite(int id)
        {
            if (id < 0 || id >= _spriteCount)
            {
                return null;
            }
            
            var sprite = _sprites[id];
            if (sprite == null)
            {
                sprite = LoadSprite(id);
                _sprites[id] = sprite;
            }
            return sprite;
        }

        private PALSprite LoadSprite(int id)
        {
            var sprite = new PALSprite(id);
            sprite.Load();
            return sprite;
        }
    }
}

