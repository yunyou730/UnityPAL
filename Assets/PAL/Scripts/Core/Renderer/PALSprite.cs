using System;
using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;
using Renderer = ayy.pal.core.Renderer;

namespace ayy.pal
{
    /*
     * class PALSprite
     *  _sheetTexture 是把所有 frame的图像拼接在一起的大图;
     *  _frameCount 有多少个 PALSpriteFrame
     *  _spriteFrames 存储每个 sprite frame信息,主要是 在大图里左下角的坐标, 以及frame像素宽度、像素高度
     *
     * 每个 sprite frame 并非固定大小.
     *  比如 李逍遥在地图上的sprite 是序号2, 宽高是 22x50;
     *  赵灵儿在地图上的 sprite 是序号3, 宽高是 26x46;
     *
     * 即是是同一个 sprite, 里面每一个 sprite frame 的 size 也不相同,
     *  比如丁秀兰 sprite 是序号4,有的帧是 20x46, 有的帧是 23x47
     *
     * 这些 size 记录在此做参考
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
        
        // 每个 frame 的 uv,wh 信息
        private List<PALSpriteFrame> _spriteFrames = null;
        
        public PALSprite(int spriteId)
        {
            _spriteId = spriteId;
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
        }

        public void Load()
        {
            MKFLoader mkf = _spriteService.GetMgoMKF();
            byte[] spriteBytes = mkf.GetDecompressedChunkData(_spriteId);
            SpriteTextureHelper.CreateSpriteV2(spriteBytes,_paletteService.GetPaletteColors(),out _sheetTexture,out _spriteFrames);
        }

        public Texture2D GetTexture()
        {
            return _sheetTexture;
        }

        public PALSpriteFrame GetFrame(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _spriteFrames.Count)
            {
                return null;
            }
            return _spriteFrames[frameIndex];
        }

        public int GetFrameCount()
        {
            return _spriteFrames.Count;
        }

        public void Dispose()
        {
            if (_sheetTexture != null)
            {
                GameObject.Destroy(_sheetTexture);
                _sheetTexture = null;
            }
            if (_spriteFrames != null)
            {
                _spriteFrames.Clear();
                _spriteFrames = null;
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
            
            Debug.Log($"frame size:{W}x{H}");
        }
    }
    
}


