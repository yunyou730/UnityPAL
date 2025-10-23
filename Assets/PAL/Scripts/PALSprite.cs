using System;
using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;
using Renderer = ayy.pal.core.Renderer;

namespace ayy.pal
{
    /*
     * 一个 PALSprite 对象, 就有一个 sprite sheet 的 texture
     * 存储 texture , 以及 frameCount 等等 
     */
    public class PALSprite : IDisposable
    {
        private SpriteService _spriteService = null;
        private PaletteService _paletteService = null;
        
        private Texture2D _sheetTexture = null;
        private int _spriteId = 0;
        private int _frameCount = 0;
        
        // 整个 sprite sheet 的 size
        private int _textureWidth = 0;
        private int _textureHeight = 0;
        
        // 每个 frame 的 uv 坐标
        private List<PALSpriteFrame> _atlas = null;
        
        public PALSprite(int spriteId)
        {
            _spriteId = spriteId;
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
        }

        public void Load()
        {
            // @miao @todo
            MKFLoader mkf = _spriteService.GetMgoMKF();
            byte[] spriteBytes = mkf.GetDecompressedChunkData(_spriteId);
            //DebugHelper.CreateSprite(spriteBytes,out _sheetTexture,out _atlas,_paletteService.GetPaletteColors());
            
            DebugHelper.CreateSpriteV2(spriteBytes,_paletteService.GetPaletteColors(),out _sheetTexture,out _atlas);
        }

        public Texture2D GetTexture()
        {
            return _sheetTexture;
        }

        public void GetFrameUV(int frameIndex,out float u,out float v)
        {
            u = 0.0f;
            v = 0.0f;
        }

        public void Dispose()
        {
            if (_sheetTexture != null)
            {
                GameObject.Destroy(_sheetTexture);
                _sheetTexture = null;
            }
        }
    }


    public class PALSpriteFrame
    {
        public int U = 0;
        public int V = 0;
        public int W = 0;
        public int H = 0;
        
        public void SetFrameOffset(int ox,int oy)
        {
            U = ox;
            V = oy;
        }

        public void SetFrameSize(int width,int height)
        {
            W = width;
            H = height;
        }
    }
    
}


