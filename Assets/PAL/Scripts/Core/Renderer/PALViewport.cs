using UnityEngine;

namespace ayy.pal
{
    public class PALViewport : MonoBehaviour
    {
        private Mesh _mesh = null;
        private Material _material = null;
        
        private float _width = 0f;
        private float _height = 0f;
        private float _z = -5.0f;

        void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _material = GetComponent<Renderer>().material;
        }

        void Start()
        {
            _width = Metrics.ConvertPixelsToUnit(Metrics.kViewportSize.x);
            _height = Metrics.ConvertPixelsToUnit(Metrics.kViewportSize.y);
            transform.localScale = new Vector3(_width, _height, 1.0f);
            RefreshCoord(0, 0);
        }
        
        void Update()
        {
            
        }
        
        /*
         * 这个函数, 约等于就是, pixelX 和 pixelY ,
         * 转换为 unity 坐标.
         *
         * 锚点在中心 pivot(0.5,0.5) ,pixelY越大,则越往下; pixelX越大, 则越往右;
         * 游戏里存档 和运行时描述的坐标 gpGlobals->viewport.x or gpGlobals->viewport.y
         * 实际上描述的,是 viewport 的中心坐标
         */
        public void RefreshCoord(int pixelX,int pixelY)
        {
            pixelX = pixelX + Metrics.kViewportSize.x / 2;
            pixelY = pixelY + Metrics.kViewportSize.y / 2;
            float x = Metrics.ConvertPixelsToUnit(pixelX);
            float y = -Metrics.ConvertPixelsToUnit(pixelY);
            transform.localPosition = new Vector3(x, y, _z);
        }
    }
    
}

