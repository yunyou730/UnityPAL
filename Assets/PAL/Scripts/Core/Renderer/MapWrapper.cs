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
         * 这里的值是固定的, 即: x方向 128个tile, y方向64个tile, h方向2个,
         * 即, 地图固定是由 128x128 个 tile 构成
         */
        private static int kTileCountX = 128;
        private static int kTileCountY = 64;
        private static int kTileCountH = 2;

        // 每个 tile 的 texture size, 这里是固定的
        private static int kTileW = 32;
        private static int kTileH = 15;
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
        // key: 字符串, x_y_h
        // value: 下标索引
        private Dictionary<string,int> _vertColorAttrBeginIndex = new Dictionary<string, int>();
        
        public MapWrapper(PALMapWrapper map,int mapIndex)
        {
            _palMap = map.LoadMapWithIndex(mapIndex);
            _mapIndex = mapIndex;

            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            
            // 顶点位置, 应该按照 32x16来计算, 而不是 32x15. 因此,这里需要在 kTileH (15)的基础上 + 1
            _tileMeshWidth = Metrics.ConvertPixelsToUnit(kTileW);
            _tileMeshHeight = Metrics.ConvertPixelsToUnit(kTileH + 1);
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
            _meshBottom = CreateTileMapMesh(ETileLayer.Bottom);
            _meshTop = CreateTileMapMesh(ETileLayer.Top);
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
                if (w != kTileW || h != kTileH)
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
                            ret.SetPixel(x + ox, y + ((kTileH - 1) - oy),data); // flip y here
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
            x = col * kTileW;
            y = row * (kTileH + 1);
        }

        public Texture2D GetTileMapTexture()
        {
            return _tilemapTexture;
        }
        
        private Mesh CreateTileMapMesh(ETileLayer tileType)
        {
            if (_palMap == null)
            {
                return null;
            }
            
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();     // 经常有 属性变动的mesh, 调用这个可以有写性能上的优化
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            
            // private static int kTileCountX = 128;
            // private static int kTileCountY = 64;
            // private static int kTileCountH = 2;
            for (int y = 0; y < kTileCountY; y++)
            {
                for (int h = 0; h < kTileCountH; h++)
                {
                    for (int x = 0; x < kTileCountX; x++)
                    {
                        AddMeshData(vertices, triangles, uvs, colors,x, y, h,tileType);
                    }
                }
            }
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uvs);
            
            // @miao @todo
            
            mesh.SetColors(colors);
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
                float zBottom = 0.0f;
                float zTop = -0.01f;
                float z = tileLayerType == ETileLayer.Top ? zTop : zBottom;
                
                // 这里根据 tile 的 logic Height , 来修改 tile 顶点的 unity 里的 z值
                // @miao @todo
                // 这里, 需要改成，如果 debug 地图, 则 直接使用 tileLogicHeight 来当作 z值；
                // 否则，改成 真正的 (y + tileLogicHeight) * 16 + h * 8; z值计算公式；
                //ETileLayer tileLayer = topOrBottom ? ELayer.Top : ELayer.Bottom;
                int tileLogicHeight = _palMap.GetMapTileLogicHeight(x, y, h, tileLayerType);
                float tileZ = (y + tileLogicHeight) * 16 + h * 8;
                //float logicDepthZ = z - tileZ * PalConst.Z_SCALE_FACTOR;
                
                // 顶点位置, 应该按照 32x16来计算, 而不是 32x15.
                // 在 tile 的 mesh 上, 额外增加了 0.005f倍数的冗余, 用于修正 地图中间会有间隙的问题
                Vector3 center = GetMapTilePos(y,x,h,ETileLayer.Bottom);
                float halfWidthWithExpand = _tileMeshWidth * 0.5f + _tileMeshWidth * 0.005f; 
                float halfHeightWithExpand = _tileMeshHeight * 0.5f + _tileMeshHeight * 0.005f;
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
                uvs.Add(new Vector2(ux + (float)(kTileW)/512.0f,uy));
                uvs.Add(new Vector2(ux,uy + (float)(kTileH)/512.0f));
                uvs.Add(new Vector2(ux + (float)(kTileW)/512.0f,uy + (float)(kTileH)/512.0f));
                
                // 用顶点色,承载更多数据, 比如 tile 是否能走. 用顶点色形式存储在mesh里,给shader 用于调试
                Color color = Color.white;
                color.a = tileZ;
                if (_palMap.IsTileBlocked(x, y, h))
                {
                    color = Color.red;
                    color.r = 1.0f;
                    color.g = 0.0f;
                    color.b = 0.0f;
                }
                
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

        private Vector3 GetMapTilePos(int x,int y,int h,ETileLayer layer)
        {
            float W = _tileMeshWidth;
            float H = _tileMeshHeight;
            float yCoord = -(y * H);
            float baseX = 0;
            if (h == 1)
            {
                baseX = baseX + W / 2;
                yCoord = yCoord - H / 2;
            }
            float xCoord = baseX + ( x * W);
            float zCoord = 0.0f;
            return new Vector3(xCoord,yCoord,zCoord);
        }
        
        private string GetTileKey(int x,int y,int h)
        {
            return $"{x}_{y}_{h}";
        }
    }    
}


