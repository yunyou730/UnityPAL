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
        private SpritePresenter _spritePresenter = null;    // 展示 sprite frame
        private int _pixelX = 0;
        private int _pixelY = 0;
        private int _logicalLayer = 0;
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

        public void GetPixelPos(out int px,out int py)
        {
            px = _pixelX;
            py = _pixelY;
        }

        public PALSpriteFrame GetCurrentSpriteFrame()
        {
            return _spritePresenter.GetCurrentSpriteFrame();
        }

        public int GetLogicLayer()
        {
            return _logicalLayer;
        }

        /*
         * 参考 scene.c
         * Draw all the sprites to the screen
         * x = p->pos.x
         * y = p->pos - SpriteFrame.H - p.layer
         */
        public void ApplyPixelPos(int viewportPixelX, int viewportPixelY,
                float z)
        {
            PALSpriteFrame frame = _spritePresenter.GetCurrentSpriteFrame();
            int ox = _pixelX;
            int oy = _pixelY - frame.H - _logicalLayer;

            int worldPixelX = viewportPixelX + ox;
            int worldPixelY = viewportPixelY + oy;
            _spritePresenter.SetPixelPos(worldPixelX,worldPixelY,z);
        }

        // public void SetSpriteZ(float z)
        // {
        //     _spritePresenter.SetPositionZ(z);
        // }

        public int GetPixelX() => _pixelX;
        public int GetPixelY() => _pixelY;
    }
}

