namespace ayy.pal
{
    /*
     * 这里,存储 unity 版的 const 常量.
     * 和游戏gameplay 里的 const无关.
     * 游戏 gameplay 里的 const 在 PALGameConst.cs 里
     */
    class PalConst
    {
        // 相机 Default Z 
        public static readonly float CAMERA_DEFAULt_Z = -1000.0f;
        
        // Sprite Tile Z Offset Scale Factor , 处理 tiles 和 sprite 遮挡时, 动态调整 z值的缩放比例,
        public static readonly  float Z_SCALE_FACTOR = 0.01f;
        
    }
}