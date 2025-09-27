using Unity.VisualScripting;
using UnityEngine;

namespace ayy.pal
{
    //
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
        public uint[,,] Tiles = new uint[128, 64, 2];   // each element:unsigned int, 4 bytes,32 bits
        public int MapIndex;        // map index, 4 bytes,32 bits
        public byte[] TileSprite = null;     // 8 bits pointer
    }

    public unsafe class Map
    {
        private PALMap _palMap = null;
        public void LoadMap(int mapIndex,MKFLoader mapMKF,MKFLoader gopMKF)
        {
            int mkfCount = mapMKF.GetChunkCount();
            int gopCount = gopMKF.GetChunkCount();
            Debug.Log(mkfCount + " : " + gopCount);
            if (mapIndex >= mkfCount || mapIndex >= gopCount || mapIndex <= 0)
            {
                return;
            }
            
            // tile data
            int size = mapMKF.GetChunkSize(mapIndex);
            //byte[] tileData = new byte[size];
            
            
            _palMap = new PALMap();

            byte[] mapChunkData = mapMKF.ReadChunk(mapIndex);
            
            //byte[] mapDecompressedData = new byte
            fixed (uint* pTilesData = _palMap.Tiles)
            {
                //Decompress.Do(mapChunkData, pTilesData);
                byte* pTilesDataBytes = (byte*)pTilesData;


                int sizeInByte = _palMap.Tiles.Length * sizeof(uint) / sizeof(byte);
                fixed (byte* pMapChunkData = mapChunkData)
                {
                    Yj1Decompressor.YJ1_Decompress(pMapChunkData, pTilesDataBytes, sizeInByte);
                }

                for (int i = 0;i < 128;i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if (_palMap.Tiles[i,j,0] > 0 || _palMap.Tiles[i,j,1] > 0)
                        {
                            Debug.Log($"map->Tiles:{i},{j}");
                        }
                    }
                }
            }
            
            
            
            // Load Bitmap
            // @miao @todo
            size = gopMKF.GetChunkSize(mapIndex);
            if (size <= 0)
            {
                return;
            }

            //_palMap.TileSprite = new byte[size];
            // if (_palMap.TileSprite == null)
            // {
            //     return;
            // }
            _palMap.TileSprite = gopMKF.ReadChunk(mapIndex);
            _palMap.MapIndex = mapIndex;

            Debug.Log("test111");


            //
            // Load the tile bitmaps.
            // map->pTileSprite = (LPSPRITE)malloc(size);
            // if (map->pTileSprite == NULL)
            // {
            //     free(map);
            //     return NULL;
            // }
            // if (PAL_MKFReadChunk(map->pTileSprite, size, iMapNum, fpGopMKF) < 0)
            // {
            //     free(map);
            //     return NULL;
            // }
            //
            // //
            // // Done.
            // //
            // map->iMapNum = iMapNum;


        }
    }
    
}

