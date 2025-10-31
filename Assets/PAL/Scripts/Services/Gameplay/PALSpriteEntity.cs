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
        public int ID = 0;  // 作为 map的 key, 来管理 sprite
        public SpritePresenter _spritePresenter = null;    // 展示 sprite frame
        public int _pixelX = 0;
        public int _pixelY = 0;
        public int _logicalLayer = 0;

        public SpriteEntity(int id,int spriteId)
        {
            ID = id;
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

        public void SwitchFrame(int frameIndex)
        {
            
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

    }
}

