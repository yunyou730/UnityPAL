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
     */
    public enum EColorMode
    {
        RawData,
        PaletteLUT,
    }

    public enum ELayer
    {
        Bottom,
        Top,
    }

    public class MapWrapper : IDisposable
    {
        // 横向、纵向,各有多少个 tile
        private static int kTileCountX = 128;
        private static int kTileCountY = 64;
        private static int kTileCountH = 2;

        // 每个 tile 的 texture size
        private static int kTileW = 32;
        private static int kTileH = 15;

        // 每个tile 在 unity 里的 mesh tile 的 size
        private static float kTileWidth = 1.0f;
        private static float kTileHeight = 15.0f / 32.0f;

        // 调色盘 能提供多少种颜色
        private static int kPaletteSize = 256;
        
        // 地图编号
        private int _mapIndex = 0;
        
        // 用一个 512x512的 texture 来当作 SpriteSheet 的 Texture
        private static int kSpriteSheetTextureSize = 512;

        private Mesh _meshBottom = null;
        private Mesh _meshTop = null;
        private Texture2D _tilemapTexture = null;
        
        private PALMapWrapper _map = null;
        private PALMap _palMap = null;
        private int _spriteFrameCount = 0;
        
        private PaletteService _paletteService = null;
        
        public MapWrapper(PALMapWrapper map,int mapIndex)
        {
            _map = map;
            _palMap = _map.LoadMapWithIndex(mapIndex);
            _mapIndex = mapIndex;

            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
        }
        
        public void Dispose()
        {
            
        }

        public void Load(EColorMode mode)
        {
            _tilemapTexture = CreateTileMapTexture(mode);
            _meshBottom = CreateTileMapMesh(false);
            _meshTop = CreateTileMapMesh(true);
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
        
        private Mesh CreateTileMapMesh(bool topOrBottom)
        {
            if (_palMap == null)
            {
                return null;
            }
            
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            for (int y = 0; y < 64; y++)
            {
                for (int h = 0; h < 2; h++)
                {
                    for (int x = 0; x < 128; x++)
                    {
                        AddMeshData(vertices, triangles, uvs, x, y, h,topOrBottom);
                    }
                }
            }
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uvs);
            mesh.SetIndices(triangles,MeshTopology.Triangles, 0,false);
            return mesh;
        }
        
        private void AddMeshData(
            List<Vector3> vertices, 
            List<int> triangles,
            List<Vector2> uvs,
            int x,int y,int h,bool topOrBottom)
        {
            int frameIndex = -1;
            if (!topOrBottom)
            {
                frameIndex = _palMap.GetSpriteIndexBottomLayer(x,y,h);
            }
            else
            {
                frameIndex = _palMap.GetSpriteIndexTopLayer(x,y,h);
            }

            if (frameIndex >= 0 && frameIndex < _spriteFrameCount)
            {
                float zBottom = 0.0f;
                float zTop = -0.01f;
                float z = topOrBottom ? zTop : zBottom;
                
                Vector3 center = GetMapTilePos(y,x,h,ELayer.Bottom);
                vertices.Add(new Vector3(center.x - kTileWidth * 0.5f,center.y - kTileHeight * 0.5f,z));
                vertices.Add(new Vector3(center.x + kTileWidth * 0.5f,center.y - kTileHeight * 0.5f,z));
                vertices.Add(new Vector3(center.x - kTileWidth * 0.5f,center.y + kTileHeight * 0.5f,z));
                vertices.Add(new Vector3(center.x + kTileWidth * 0.5f,center.y + kTileHeight * 0.5f,z));
                
                int ox, oy;
                GetFrameIndexPixelCoord(frameIndex,out ox,out oy);
                float ux = (float)ox / (float)kSpriteSheetTextureSize;
                float uy = (float)oy / (float)kSpriteSheetTextureSize;

                uvs.Add(new Vector2(ux,uy));
                uvs.Add(new Vector2(ux + 32.0f/512.0f,uy));
                uvs.Add(new Vector2(ux,uy + 15.0f/512.0f));
                uvs.Add(new Vector2(ux + 32.0f/512.0f,uy + 15.0f/512.0f));
                        
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

        private Vector3 GetMapTilePos(int x,int y,int h,ELayer layer)
        {
            float W = kTileWidth;
            float H = kTileHeight;
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
    }    
}


