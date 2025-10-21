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
        private Texture2D _sheetTexture = null;
        private int _spriteId = 0;
        private int _frameCount = 0;
        
        // 整个 sprite sheet 的 size
        private int _textureWidth = 0;
        private int _textureHeight = 0;
        
        // 每个 frame 的 uv 坐标
        
        
        public PALSprite(int spriteId)
        {
            _spriteId = spriteId;
        }

        public void Load()
        {
            
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


    class SpriteFrameUV
    {
        public int U;
        public int V;
    }
    
}


