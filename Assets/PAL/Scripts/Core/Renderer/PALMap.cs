using System.IO;
using UnityEngine;

namespace ayy.pal.core
{
    // 参考 map.h, PALMAP
    // Map format:
    //
    // +----------------------------------------------> x
    // | * * * * * * * * * * ... * * * * * * * * * *  (y = 0, h = 0)
    // |  * * * * * * * * * * ... * * * * * * * * * * (y = 0, h = 1)
    // | * * * * * * * * * * ... * * * * * * * * * *  (y = 1, h = 0)
    // |  * * * * * * * * * * ... * * * * * * * * * * (y = 1, h = 1)
    // | * * * * * * * * * * ... * * * * * * * * * *  (y = 2, h = 0)
    // |  * * * * * * * * * * ... * * * * * * * * * * (y = 2, h = 1)
    // | ............................................
    // v
    // y
    //
    // Note:
    //
    // Tiles are in diamond shape (32x15).
    //
    // Each tile is represented with a DWORD value, which contains information
    // about the tile bitmap, block flag, height, etc.
    //
    // Bottom layer sprite index:
    //  (d & 0xFF) | ((d >> 4) & 0x100)
    //
    // Top layer sprite index:
    //  d >>= 16;
    //  ((d & 0xFF) | ((d >> 4) & 0x100)) - 1)
    //
    // Block flag (player cannot walk through this tile):
    //  d & 0x2000
    //
    public class PALMap
    {
        /*
         *  Tiles [ 128行, 64列, 2个子列]
         */   
        public uint[,,] Tiles = new uint[128, 64, 2];   // each element:unsigned int, 4 bytes,32 bits
        public int MapIndex;        // map index, 4 bytes,32 bits
        public byte[] TileSprite = null;     // 8 bits pointer

        
        /*
         * 参考  map.c PAL_MapGetTileBitmap
         */
        public int GetSpriteIndexBottomLayer(int x,int y,int h)
        {   
            if (x >= 64 || y >= 128 || h > 1)
            {
                return -1;
            }
            int d = (int)Tiles[y,x,h];
            return (d & 0xFF) | ((d >> 4) & 0x100);
        }
        
        /*
         * 参考  map.c PAL_MapGetTileBitmap
         */
        public int GetSpriteIndexTopLayer(int x, int y, int h)
        {
            if (x >= 64 || y >= 128 || h > 1)
            {
                return -1;
            }
            
            int d = (int)Tiles[y,x,h];
            d = d >> 16;
            d = ((d & 0xFF) | ((d >> 4) & 0x100)) - 1;
            return d;
        }

        public bool IsTileBlocked(int x, int y, int h)
        {
            if(y >= 128 || x >= 64 || h > 1)
            {
                return false;
            }
            int d = (int)Tiles[y, x, h];
            d = (d & 0x2000) >> 13;
            return d > 0;
        }
        
        /**
         * 参考 map.c PAL_MapGetTileHeight()
         * 获取 tile 的 "逻辑高度"
         * 取值范围是 [0,15]
         */
        public int GetMapTileLogicHeight(int x,int y,int h,ETileLayer layer)
        {
            if(y >= 128 || x >= 64 || h > 1 || y < 0 || x < 0)
            {
                return 0;
            }
            uint d = Tiles[y,x, h];        
            if (layer == ETileLayer.Top)
            {
                d = d >> 16;
            }
            d = d >> 8;
            return ((int)d & 0xf);
        }


        /*
         * 参考 scene.c, 把 tile 的像素坐标 y值, 和 sprite 的底部像素坐标 y值,做比较的逻辑 
         * (ty + tileHeight) * 16  + th * 8
         */
        public int GetMapTilePixelYCoord(int tx,int ty,int th,ETileLayer layer)
        {
            int tileLogicHeight = GetMapTileLogicHeight(tx,ty,th,layer);
            if (tileLogicHeight == 0)
            {
                return 0;
            }
            // ty: tile 处于第几行.
            // ty * 16, 这里的 16 是每个 tile 的 height
            // (ty + tileLogicHeight) * 16, 这里的 tileLogicHeight,
            //      是每个 tile 的逻辑高度. 在 像素坐标y的基础上拉高.
            //      这个值应该是 8的倍数
            // th * 8, 这里的 th , 是 tile 的 h坐标, 0或者1
            //  这里的 8 , 是 tile坐标 x,y相同时, 相邻的 h = 0 和 h = 1, 所相差 的 半个tile.height (16) 的像素距离 8 
            int ret = (ty + tileLogicHeight) * 16 + th * 8;
            return ret;
        }
    }

    public unsafe class PALMapWrapper
    {
        private MKFLoader _mapMKF = null;   // 地图tile数据
        private MKFLoader _gopMKF = null;   // 地图sprite数据
        
        public void Load()
        {
            _mapMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "MAP.MKF"));
            _gopMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "GOP.MKF"));
            _mapMKF.Load();
            _gopMKF.Load();
        }

        public PALMap LoadMapWithIndex(int mapIndex)
        {
            int mkfCount = _mapMKF.GetChunkCount();
            int gopCount = _gopMKF.GetChunkCount();
            Debug.Log(mkfCount + " : " + gopCount);
            if (mapIndex >= mkfCount || mapIndex >= gopCount || mapIndex <= 0)
            {
                return null;
            }
            
            // tile data
            int size = _mapMKF.GetChunkSize(mapIndex);
            
            var palMap = new PALMap();
            
            byte[] mapChunkData = _mapMKF.ReadChunk(mapIndex);
            fixed (uint* pTilesData = palMap.Tiles)
            {
                byte* pTilesDataBytes = (byte*)pTilesData;
                int sizeInByte = palMap.Tiles.Length * sizeof(uint) / sizeof(byte);
                fixed (byte* pMapChunkData = mapChunkData)
                {
                    Yj1Decompressor.YJ1_Decompress(pMapChunkData, pTilesDataBytes, sizeInByte);
                }
            }
            
            // Load Bitmap
            size = _gopMKF.GetChunkSize(mapIndex);
            if (size <= 0)
            {
                return null;
            }

            palMap.TileSprite = _gopMKF.ReadChunk(mapIndex);
            palMap.MapIndex = mapIndex;
            
            return palMap;
        }
        
        public int GetMapCount()
        {
            int ret = _mapMKF.GetChunkCount();
            return ret;
        }
    }
    
}

