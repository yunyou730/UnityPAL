using System;
using ayy.pal;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 scene.h
     * struct tagSPRITE_TO_DRAW
     * {
     *      LPCBITMAPRLE spriteFrame;   // sprite frame 的二进制数据
     *      PAL_POS pos;                // 在相机范围内的 pixel偏移
     *      int iLayer;                 // logical layer
     * }
     */
    public class SpriteEntity : IDisposable
    {
        public int Key = 0;  // 作为 map的 key, 来管理 sprite
        public SpritePresenter _spritePresenter = null;    // 展示 sprite frame
        public int _pixelX = 0;
        public int _pixelY = 0;
        public int _logicalLayer = 0;
        private int _spriteId = 0;

        public SpriteEntity(int key,int spriteId)
        {
            Key = key;
            _spriteId = spriteId;
            var prefab = PalGame.GetInstance().GetSpritePrefab();
            _spritePresenter = GameObject.Instantiate(prefab).GetComponent<SpritePresenter>();
            _spritePresenter.SwitchSpriteFrame(spriteId,0);
        }
        
        public void Dispose()
        {
            if (_spritePresenter != null)
            {
                GameObject.Destroy(_spritePresenter.gameObject);
                _spritePresenter = null;
            }
        }

        public PALSpriteFrame SwitchFrame(int frameIndex)
        {
            _spritePresenter.SwitchSpriteFrame(_spriteId,frameIndex);
            return _spritePresenter.GetCurrentSpriteFrame();
        }

        public void SetPixelPosition(int pixelX, int pixelY)
        {
            _pixelX = pixelX;
            _pixelY = pixelY;
        }

        public void SetLayer(int logicalLayer)
        {
            _logicalLayer = logicalLayer;
        }

        public void ApplyPixelPos(int viewportPixelX, int viewportPixelY)
        {
            PALSpriteFrame frame = _spritePresenter.GetCurrentSpriteFrame();
            int ox = _pixelX;
            int oy = _pixelY - frame.H - _logicalLayer;

            int x = viewportPixelX + ox;
            int y = viewportPixelY + oy;
            _spritePresenter.SetPixelPos(x,y);
        }
    }
}

