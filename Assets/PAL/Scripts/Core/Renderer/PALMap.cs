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
        public static int kMaxX = 128;
        public static int kMaxY = 64;
        public static int kMaxH = 2;
        
        public uint[,,] Tiles = new uint[128, 64, 2];   // each element:unsigned int, 4 bytes,32 bits
        public int MapIndex;        // map index, 4 bytes,32 bits
        public byte[] TileSprite = null;     // 8 bits pointer

        public int GetSpriteIndexBottomLayer(int x,int y,int h)
        {
            int d = (int)Tiles[x,y,h];
            return (d & 0xFF) | ((d >> 4) & 0x100);
        }

        public int GetSpriteIndexTopLayer(int x, int y, int h)
        {
            int d = (int)Tiles[x,y,h];
            d = d >> 16;
            d = ((d & 0xFF) | ((d >> 4) & 0x100)) - 1;
            return d;
        }

        public bool IsTileBlocked(int x, int y, int h)
        {
            if(x >= 128 || y >= 64 || h > 1)
            {
                return false;
            }
            int d = (int)Tiles[x, y, h];
            d = (d & 0x2000) >> 13;
            return d > 0;
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

