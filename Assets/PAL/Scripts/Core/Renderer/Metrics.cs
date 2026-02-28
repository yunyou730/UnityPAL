using UnityEngine;

namespace ayy.pal
{
    public class Metrics
    {
        // 每32个像素, 对应1个 Unity单位
        public static float kPixelsToUnit = 1.0f / 32.0f;
        
        // 原版游戏默认是 320x200 分辨率
        public static Vector2Int kViewportSize = new Vector2Int(320, 200);
        
        public static float ConvertPixelsToUnit(int pixels)
        {
            float units = pixels * kPixelsToUnit;
            return units;
        }
        
        public static Vector2 ConvertPixelPosToUnitPos(int pixelCoordX,int pixelCoordY)
        {
            // @miao @todo
            Vector2 posUnits = Vector2.zero;
            float ox = ConvertPixelsToUnit(pixelCoordX);
            float oy = ConvertPixelsToUnit(pixelCoordY);
            posUnits.x = ox;
            posUnits.y = -oy;
            return posUnits;
        }
        
        public static void ConvertWorldSpacePixelCoordToTileCoord (
            int worldPixelX,
            int worldPixelY,
            out MapTileCoord mapCoord)
        {
            // int worldPixelX = viewportX + pixelX - layer / 2;
            // int worldPixelY = viewportY + pixelY - layer;

            // int tileX = 0;
            // int tileY = 0;
            // int tileH = 0;
            // 把 世界空间下的 pixel 坐标,转换为 tile x,y,h 坐标
            // tileX = worldPixelX / 32;
            // tileY = (worldPixelY - 15) / 16;
            // tileH = (worldPixelX % 32 != 0) ? 1 : 0;

            // 看起来好像计算对了,但是为什么总感觉,到了我这, tileX,tileY 是反过来的...
            mapCoord.TileX = (worldPixelY - 15) / 16;
            mapCoord.TileY = worldPixelX / 32;
            mapCoord.TileH = (worldPixelY % 32 != 0) ? 1 : 0;
        }
    }


    public struct MapTileCoord
    {
        public int TileX;
        public int TileY;
        public int TileH;
    }
}

