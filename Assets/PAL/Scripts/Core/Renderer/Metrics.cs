namespace ayy.pal
{
    public class Metrics
    {
        // 每32个像素, 对应1个 Unity单位
        public static float kPixelsToUnit = 1.0f / 32.0f;   
        public static float ConvertPixelsToUnit(int pixels)
        {
            float units = pixels * kPixelsToUnit;
            return units;
        }
    }
}

