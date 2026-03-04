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
        
        public static void ConvertWorldSpacePixelCoordToTileCoord(
            int worldPixelX,
            int worldPixelY,
            out MapTileCoord mapCoord)
        {
            // 32是一个 tile 的 宽度. worldPixelX / 32 相当于在计算, tile的 x坐标
            mapCoord.TileX = worldPixelX / 32;
            
            // 16 是一个 tile 的高度, worldPixelY / 16 相当于是计算, tile的 y坐标
            mapCoord.TileY = (worldPixelY - 15) / 16;
            
            // 要根据 worldPixelY % 32 是否能除尽, 如果能,则 h是 0; 否则 h是1 。
            // h是 tile坐标体系里的 y坐标扩展  
            mapCoord.TileH = (worldPixelY % 32 != 0) ? 1 : 0;

            // 临时随便写一个, 大部分时候用不上
            mapCoord.TileLayer = ETileLayer.Bottom;
        }
    }


    public struct MapTileCoord
    {
        public int TileX;
        public int TileY;
        public int TileH;
        public ETileLayer TileLayer;
    }
}

