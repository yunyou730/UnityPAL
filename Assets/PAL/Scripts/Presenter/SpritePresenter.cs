using System;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class SpritePresenter : MonoBehaviour
    {
        private int _spriteIndex = 0;
        private int _frameIndex = 0;
        private PALSprite _sprite = null;
        
        private MeshRenderer _meshRenderer;
        private Material _material;
        
        private SpriteService _spriteService;
        
        void Awake()
        {
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = _meshRenderer.material;
        }

        void Start()
        {
            
        }

        void OnDestroy()
        {
            
        }

        void Update()
        {
        
        }

        public void SwitchSpriteFrame(int spriteIndex,int frameIndex)
        {
            if (_spriteIndex != spriteIndex)
            {
                _spriteIndex = spriteIndex;
                _sprite = _spriteService.GetSprite(spriteIndex);
                _material.SetTexture(Shader.PropertyToID("_SpriteTex"), _sprite.GetTexture());                
            }
            SwitchFrame(frameIndex);
        }

        public void SwitchNextFrame()
        {
            if (_sprite != null)
            {
                int frameCount = _sprite.GetFrameCount();
                int nextFrameIndex = _frameIndex + 1;
                if (nextFrameIndex >= frameCount)
                {
                    nextFrameIndex = 0;
                }
                SwitchFrame(nextFrameIndex);
            }
        }

        public void SetPixelPos(int pixelX, int pixelY)
        {
            Vector2 pos = Metrics.ConvertPixelPosToUnitPos(pixelX, pixelY);
            float originZ = transform.localPosition.z;
            transform.localPosition = new Vector3(pos.x, pos.y, originZ);
        }

        private void SwitchFrame(int frameIndex)
        {
            _frameIndex = frameIndex;
            PALSpriteFrame frame = _sprite.GetFrame(_frameIndex);
            if (frame != null)
            {
                // 刷新 material 里的 uv , 显示对应帧
                int texWidth = _sprite.GetTexture().width;
                int texHeight = _sprite.GetTexture().height;
                float ox = (float)frame.U / (float)texWidth;
                float oy = (float)frame.V / (float)texHeight;
                float fw = (float)frame.W / (float)texWidth;
                float fh = (float)frame.H / (float)texHeight;

                Vector4 uvOffset = new Vector4(ox, oy, 0.0f, 0.0f);
                Vector4 uvScale = new Vector4(fw,fh,0.0f,0.0f);
                _material.SetVector(Shader.PropertyToID("_UVOffset"), uvOffset);
                _material.SetVector(Shader.PropertyToID("_UVScale"), uvScale);
                
                // 根据帧宽高, 通过设置 localScale 来调整 gameObject 的 size 
                // 因为只能调整 scale, 因此, 要求 prefab 的 size 默认得是 1?
                float unitWidth = Metrics.ConvertPixelsToUnit(frame.W);
                float unitHeight = Metrics.ConvertPixelsToUnit(frame.H);
                transform.localScale = new Vector3(unitWidth,unitHeight,1);
            }
        }
    }
    
}

