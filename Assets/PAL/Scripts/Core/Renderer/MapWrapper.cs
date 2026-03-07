using System;
using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;
using Renderer = ayy.pal.core.Renderer;

namespace ayy.pal
{
    /*
     * 1. 如果使用 RawData 模式, 则从 原始 Sprite 数据里,每个像素读取的图像像素值是 palette index.
     * 在 shader 里把 palette index 和 Palette Texture 做影射,得到最终显示的像素值
     *
     * 2. 如果使用 PaletteLUT 模式, 则在C#代码里做完上面的事。即,读取时读取的是 palette index.
     * 这个 index 配合 PaletteColor[]数组, 换算成真正的像素颜色值.
     * 写入 Texture 像素的时候, 直接写入查询 palette 完毕之后的最终像素值
     *
     * 3. 地图里面 每个tile图像 固定是 32 x 15.
     * map tile 是 32x15 的 rect, 中间菱形部分有图像
     *
     * 4. MapWrapper 会按照规则, 构建一个 mesh, mesh 大小取决于 kTileWidth 和 kTileHeight
     */
    public enum EColorMode
    {
        RawData,
        PaletteLUT,
    }

    public enum ETileLayer
    {
        Bottom,
        Top,
    }

    public class MapWrapper : IDisposable
    {
        /*
         * 横向、纵向,各有多少个 tile
         * 这里的值是固定的, 即:
         *      纵向,有 128个tile,
         *      横向,有 64个列,
         *      每一列, h 有两个子列 . h 为0 或者 1 
         * 即, 地图固定是由 128x128 个 tile 构成
         */
        
        private static readonly int kTileCountY = 128;   // 有 128行
        private static readonly int kTileCountX = 64;    // 有 64列
        private static readonly int kTileCountH = 2;     // 每一列, 都有交错 2个小列

        // 每个 tile 的 texture size, 这里是固定的
        public static readonly int kTilePixelsW = 32;
        public static readonly int kTilePixelsH = 15;
        
        public static readonly int kTilePixelSizeH = 16;
        //private static int kTileH = 16;
        

        // 每个tile 在 unity 里的 mesh tile 的 size
        // 在初始化的时候, 需要根据 Metrics 的 size转换功能, 做一次转换
        private float _tileMeshWidth;
        private float _tileMeshHeight;
        
        // 地图编号
        private int _mapIndex = 0;
        
        /*
         * 用一个 512x512的 texture 来当作 SpriteSheet 的 Texture
         * 因为每个 tile 的 texture size 是固定的 32x15,
         * 并且 通常 tile 的数量 也不会太多,
         * 因此用一个 512x512 的 texture 来装载所有 tile 的纹理,
         * 是够用的
         */
        private static int kSpriteSheetTextureSize = 512;

        private Mesh _meshBottom = null;
        private Mesh _meshTop = null;
        private Texture2D _tilemapTexture = null;
        
        
        private PALMap _palMap = null;
        private int _spriteFrameCount = 0;
        
        private PaletteService _paletteService = null;


        // 记录每个 tile , 它的顶点颜色属性, 开始的下标
        // key: 字符串, tileLayer_x_y_h
        // value: 下标索引
        private Dictionary<string,int> _vertColorAttrBeginIndex = new Dictionary<string, int>();
        private List<Color> _colorsCacheBottom = new List<Color>();
        private List<Color> _colorsCacheTop = new List<Color>();
        // private bool _colorDirtyFlag = false;
        
        
        // 记录每个 tile, 顶点position属性,开始的下标
        // key: 字符串, tileLayer_x_y_h
        // value: 下标索引
        // 用于能方便的 更新顶点位置
        private Dictionary<string,int> _vertPosAttrBeginIndex = new Dictionary<string, int>();
        private List<Vector3> _vertPosCacheBottom = new List<Vector3>();
        private List<Vector3> _vertPosCacheTop = new List<Vector3>();
        
        public MapWrapper(PALMapWrapper map,int mapIndex)
        {
            _palMap = map.LoadMapWithIndex(mapIndex);
            _mapIndex = mapIndex;

            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            
            // 顶点位置, 应该按照 32x16来计算, 而不是 32x15. 因此,这里需要在 kTileH (15)的基础上 + 1
            _tileMeshWidth = Metrics.ConvertPixelsToUnit(kTilePixelsW);
            _tileMeshHeight = Metrics.ConvertPixelsToUnit(kTilePixelSizeH);
            //_tileMeshHeight = Metrics.ConvertPixelsToUnit(kTilePixelsH + 1);
        }
        
        public void Dispose()
        {
            if (_tilemapTexture != null)
            {
                GameObject.Destroy(_tilemapTexture);
                _tilemapTexture = null;
            }
            
            if (_meshBottom != null)
            {
                GameObject.Destroy(_meshBottom);
                _meshBottom = null;
            }

            if (_meshTop != null)
            {
                GameObject.Destroy(_meshTop);
                _meshTop = null;
            }
        }

        public void Load(EColorMode mode)
        {
            _tilemapTexture = CreateTileMapTexture(mode);
            
            // 清空数据. 顶点颜色数据
            _vertColorAttrBeginIndex.Clear();
            _colorsCacheBottom.Clear();
            _colorsCacheTop.Clear();
            
            // 清空数据, 顶点位置数据
            _vertPosAttrBeginIndex.Clear();
            _vertPosCacheBottom.Clear();
            _vertPosCacheTop.Clear();
            
            // 开始构建 mesh
            _meshBottom = CreateTileMapMesh(ETileLayer.Bottom,_vertPosCacheBottom,_colorsCacheBottom);
            _meshTop = CreateTileMapMesh(ETileLayer.Top,_vertPosCacheTop,_colorsCacheTop);
        }

        private Texture2D CreateTileMapTexture(EColorMode mode)
        {
            if (_palMap == null)
            {
                return null;
            }
            
            // 当使用 rawdata 作为 texture 像素,在 shader里索引 paletteLUT 的时候,就不应该开启sRGB;
            // 如果不使用 rawdata, 而是在 C# 里索引 paletteLUT 的时候, 就姚开启 sRGB
            bool isSRGB = true;
            if (mode == EColorMode.RawData)
            {
                isSRGB = false;
            }

            var ret = new Texture2D(
                kSpriteSheetTextureSize, 
                kSpriteSheetTextureSize,
                //TextureFormat.RG16,
                TextureFormat.ARGB32,
                false,
                !isSRGB);
            ret.filterMode = FilterMode.Point;
            for (int x = 0;x < kSpriteSheetTextureSize;x++)
            {
                for (int y = 0;y < kSpriteSheetTextureSize;y++)
                {
                    ret.SetPixel(x,y,new Color(0, 0, 0, 0));
                }
            }
            
            // 根据返回值 设置颜色
            _spriteFrameCount = Renderer.GetSpriteFrameCount(_palMap.TileSprite);
            for (int frameIndex = 0;frameIndex < _spriteFrameCount;frameIndex++)
            {
                // 是否在获取颜色的时候,使用上 Palette LUT
                PaletteColor[] paletteColors = null;
                if (mode == EColorMode.PaletteLUT)
                {
                    paletteColors = _paletteService.GetPaletteColors();
                }
                
                Color32[,] tileColorData = Renderer.GetSpriteFrameColorData(_palMap.TileSprite, frameIndex,paletteColors);
                if (tileColorData == null)
                {
                    Debug.LogWarning($"invalid tile at frame index:{frameIndex}");
                    continue;
                }

                
                int w = tileColorData.GetLength(0);
                int h = tileColorData.GetLength(1);
                if (w != kTilePixelsW || h != kTilePixelsH)
                {
                    Debug.LogWarning($"invalid tile at frame index:{frameIndex}");
                    continue;
                }

                int x, y;
                GetFrameIndexPixelCoord(frameIndex,out x,out y);
                for (int ox = 0;ox < w;ox++)
                {
                    for (int oy = 0;oy < h;oy++)
                    {
                        Color32 data = tileColorData[ox,oy];
                        if (mode == EColorMode.RawData)
                        {
                            byte r = (byte)data.r;
                            byte a = (byte)data.a;
                            Color32 c = new Color32(r,0,0,a);
                            ret.SetPixel(x + ox, y + oy,c);
                        }
                        else if (mode == EColorMode.PaletteLUT)
                        {
                            //ret.SetPixel(x + ox, y + oy,data);
                            ret.SetPixel(x + ox, y + ((kTilePixelsH - 1) - oy),data); // flip y here
                        }
                    }
                }
            }
            ret.Apply();
            return ret;
        }

        
        private void GetFrameIndexPixelCoord(int frameIndex,out int x, out int y)
        {
            int row = frameIndex / 16;
            int col = frameIndex % 16;
            x = col * kTilePixelsW;
            y = row * (kTilePixelSizeH);
        }

        public Texture2D GetTileMapTexture()
        {
            return _tilemapTexture;
        }
        
        private Mesh CreateTileMapMesh(ETileLayer tileType,List<Vector3> vertPosCache,List<Color> colorsCache)
        {
            if (_palMap == null)
            {
                return null;
            }
            
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();     // 经常有 属性变动的mesh, 调用这个可以有写性能上的优化
            //List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            //List<Color> colors = new List<Color>();
            
            for (int x = 0; x < kTileCountX; x++)       // 64列 
            {
                for (int h = 0; h < kTileCountH; h++)   // 每一列的 2个子列
                {
                    for (int y = 0; y < kTileCountY; y++)   // 每一行,共 128行
                    {
                        AddMeshData(vertPosCache, triangles, uvs, colorsCache,x, y, h,tileType);
                    }
                }
            }
            mesh.SetVertices(vertPosCache);
            mesh.SetUVs(0,uvs);
            
            // @miao @todo
            mesh.SetColors(colorsCache);
            mesh.SetIndices(triangles,MeshTopology.Triangles, 0,false);
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        private void AddMeshData(
            List<Vector3> vertices, 
            List<int> triangles,
            List<Vector2> uvs,
            List<Color> colors,
            int x,int y,int h,
            ETileLayer tileLayerType)
        {
            int frameIndex = -1;
            switch (tileLayerType)
            {
                case ETileLayer.Bottom:
                    frameIndex = _palMap.GetSpriteIndexBottomLayer(x,y,h);
                    break;
                case ETileLayer.Top:
                    frameIndex = _palMap.GetSpriteIndexTopLayer(x,y,h);
                    break;
            }

            if (frameIndex >= 0 && frameIndex < _spriteFrameCount)
            {
                // 临时存储 当前tile 的 顶点属性, 用于后面做修改
                string tileKey = GetTileKey(tileLayerType, x, y, h);
                float z = GetZForTileLayerType(tileLayerType,false);
                
                
                // 这里根据 tile 的 logic Height , 来修改 tile 顶点的 unity 里的 z值
                // 这里, 需要改成，如果 debug 地图, 则 直接使用 tileLogicHeight 来当作 z值；
                // 否则，改成 真正的 (y + tileLogicHeight) * 16 + h * 8; z值计算公式；
                //ETileLayer tileLayer = topOrBottom ? ELayer.Top : ELayer.Bottom;
                int tileLogicHeight = _palMap.GetMapTileLogicHeight(x, y, h, tileLayerType);
                float tileZ = (y + tileLogicHeight) * 16 + h * 8;
                //float logicDepthZ = z - tileZ * PalConst.Z_SCALE_FACTOR;
                
                // 顶点位置, 应该按照 32x16来计算, 而不是 32x15.
                Vector3 center = GetMapTileCenterPos(x,y,h);
                
                // 在 tile 的 mesh 上, 额外增加了 0.005f倍数的冗余, 用于修正 地图中间会有间隙的问题
                float halfWidthWithExpand = _tileMeshWidth * 0.5f + _tileMeshWidth * 0.005f; 
                float halfHeightWithExpand = _tileMeshHeight * 0.5f + _tileMeshHeight * 0.005f;
                
                // 把顶点数据, 缓存起来
                _vertPosAttrBeginIndex.Add(tileKey,vertices.Count);   //  用于快速定位 某个tile 的 pos属性
                
                vertices.Add(new Vector3(center.x - halfWidthWithExpand,center.y - halfHeightWithExpand,z));
                vertices.Add(new Vector3(center.x + halfWidthWithExpand,center.y - halfHeightWithExpand,z));
                vertices.Add(new Vector3(center.x - halfWidthWithExpand,center.y + halfHeightWithExpand,z));
                vertices.Add(new Vector3(center.x + halfWidthWithExpand,center.y + halfHeightWithExpand,z));
                
                int ox, oy;
                GetFrameIndexPixelCoord(frameIndex,out ox,out oy);
                float ux = (float)ox / (float)kSpriteSheetTextureSize;
                float uy = (float)oy / (float)kSpriteSheetTextureSize;

                // uv 应该按照 32x15 每个tile 来计算
                //private static int kTileW = 32;
                //private static int kTileH = 15;
                uvs.Add(new Vector2(ux,uy));
                uvs.Add(new Vector2(ux + (float)(kTilePixelsW)/512.0f,uy));
                uvs.Add(new Vector2(ux,uy + (float)(kTilePixelsH)/512.0f));
                uvs.Add(new Vector2(ux + (float)(kTilePixelsW)/512.0f,uy + (float)(kTilePixelsH)/512.0f));
                
                // 用顶点色,承载更多数据, 比如 tile 是否能走. 用顶点色形式存储在mesh里,给shader 用于调试
                Color color = Color.black;
                color.a = tileZ;
                if (_palMap.IsTileBlocked(x, y, h))
                {
                    color = Color.red;
                    color.r = 1.0f;
                    color.g = 0.0f;
                    color.b = 0.0f;
                }

                _vertColorAttrBeginIndex.Add(tileKey,colors.Count); //  用于快速定位 某个tile 的 color属性

                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);

                int cnt = vertices.Count;
                triangles.Add(cnt-4);
                triangles.Add(cnt-3);
                triangles.Add(cnt-2);
                triangles.Add(cnt-3);
                triangles.Add(cnt-1);
                triangles.Add(cnt-2);
            }
        }

        public Mesh GetTileMapMeshBottom()
        {
            return _meshBottom;
        }

        public Mesh GetTileMapMeshTop()
        {
            return _meshTop;
        }

        public float GetZForTileLayerType(ETileLayer tileLayerType,bool coverSprite)
        {
            float zBottom = 0.0f;
            float zTop = -0.01f;
            float z = tileLayerType == ETileLayer.Top ? zTop : zBottom;
            if (coverSprite)
            {
                // @miao @todo
                z = z - 10.0f;
            }
            return z;
        }


        private Vector3 GetMapTileCenterPos(int tileX,int tileY,int tileH)
        {
            float W = _tileMeshWidth;
            float H = _tileMeshHeight;
            float yCoord = -(tileY * H);
            float baseX = 0;
            if (tileH == 1)
            {
                baseX = baseX + W / 2;
                yCoord = yCoord - H / 2;
            }
            float xCoord = baseX + (tileX * W);
            float zCoord = 0.0f;
            return new Vector3(xCoord,yCoord,zCoord);
        }

        private string GetTileKey(ETileLayer tileLayer,int x,int y,int h)
        {
            return $"{tileLayer}_{x}_{y}_{h}";
        }


        private Mesh GetMeshAtTileLayer(ETileLayer tileLayer)
        {
            switch (tileLayer)
            {
                case ETileLayer.Bottom:
                    return _meshBottom;
                case ETileLayer.Top:
                    return _meshTop;
            }
            return null;
        }

        public void SetTileVertexColor(ETileLayer tileLayer,int x,int y,int h,Color color)
        {
            string tileKey = GetTileKey(tileLayer, x, y, h);
            if (!_vertColorAttrBeginIndex.ContainsKey(tileKey))
            {
                return;
            }

            List<Color> colorsCache = null;
            switch (tileLayer)
            {
                case ETileLayer.Bottom:
                    colorsCache = _colorsCacheBottom;
                    break;
                case ETileLayer.Top:
                    colorsCache = _colorsCacheTop;
                    break;
            }
            
            int beginIndex = _vertColorAttrBeginIndex[tileKey];
            int endIndex = beginIndex + 4;
            for (int i = beginIndex; i < endIndex; i++)
            {
                colorsCache[i] = color;
            }
        }

        public void ApplyTileVertexColorsChange()
        {
            _meshBottom.SetColors(_colorsCacheBottom);
            _meshTop.SetColors(_colorsCacheTop);
        }

        public void SetTileVertPosZ(ETileLayer tileLayer,int x,int y,int h,float z)
        {
            string tileKey = GetTileKey(tileLayer, x, y, h);
            if (!_vertColorAttrBeginIndex.ContainsKey(tileKey))
            {
                return;
            }

            List<Vector3> posCache = null;
            switch (tileLayer)
            {
                case ETileLayer.Bottom:
                    posCache = _vertPosCacheBottom;
                    break;
                case ETileLayer.Top:
                    posCache = _vertPosCacheTop;
                    break;
            }
            
            int beginIndex = _vertPosAttrBeginIndex[tileKey];
            int endIndex = beginIndex + 4;
            for (int i = beginIndex; i < endIndex; i++)
            {
                Vector3 pos = posCache[i];
                pos.z = z;
                posCache[i] = pos;
            }
        }

        public void ApplyTileVertPosZChange()
        {
            _meshBottom.SetVertices(_vertPosCacheBottom);
            _meshTop.SetVertices(_vertPosCacheTop);
        }

        public PALMap GetPalMap()
        {
            return _palMap;
        }
    }
}


