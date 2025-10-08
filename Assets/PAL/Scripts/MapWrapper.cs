using System;
using ayy.pal.core;
using UnityEngine;
using Renderer = ayy.pal.core.Renderer;

namespace ayy.pal
{
    public enum EColorMode
    {
        RawData,
        PaletteLUT,
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

        private Mesh _mesh = null;
        private Texture2D _tilemapTexture = null;
        
        private Map _map = null;
        private PALMap _palMap = null;
        private int _spriteFrameCount = 0;
        
        private PaletteService _paletteService = null;
        
        public MapWrapper(Map map,int mapIndex)
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
            CreateTileMapTexture(mode);
            CreateTileMapMesh();
        }

        private void CreateTileMapTexture(EColorMode mode)
        {
            if (_palMap == null)
            {
                return;
            }


            bool isSRGB = true;
            if (mode == EColorMode.RawData)
            {
                isSRGB = false;
            }

            _tilemapTexture = new Texture2D(
                kSpriteSheetTextureSize, 
                kSpriteSheetTextureSize,
                //TextureFormat.RG16,
                TextureFormat.ARGB32,
                false,
                !isSRGB);
            _tilemapTexture.filterMode = FilterMode.Point;
            for (int x = 0;x < kSpriteSheetTextureSize;x++)
            {
                for (int y = 0;y < kSpriteSheetTextureSize;y++)
                {
                    _tilemapTexture.SetPixel(x,y,new Color(0, 0, 0, 0));
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
                        Color32 data = tileColorData[ox, oy];
                        if (mode == EColorMode.RawData)
                        {
                            byte r = (byte)data.r;
                            byte a = (byte)data.a;
                            Color32 c = new Color32(r,0,0,a);
                            _tilemapTexture.SetPixel(x + ox, y + oy,c);
                        }
                        else if (mode == EColorMode.PaletteLUT)
                        {
                            _tilemapTexture.SetPixel(x + ox, y + oy,data);
                        }
                    }
                }
            }
            _tilemapTexture.Apply();
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
        
        private void CreateTileMapMesh()
        {
            Mesh mesh = new Mesh();
            
            //mesh.SetVertices();
        }
        
        public Mesh GetTileMapMesh()
        {
            return _mesh;
        }

    }    
}


