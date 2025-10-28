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
    }
}

