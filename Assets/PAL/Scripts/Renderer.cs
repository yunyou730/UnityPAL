using UnityEngine;

namespace ayy.pal
{
    public unsafe class Renderer
    {
        public int GetSpriteFrameCount(byte[] sprite)
        {
            int frameCount = sprite[0] | sprite[1] << 8;
            return frameCount;
        }

        private int GetSpriteFrameOffset(byte[] sprite,int frameIndex)
        {
            int frameCount = GetSpriteFrameCount(sprite);
            if (frameIndex < 0 || frameIndex >= frameCount)
            {
                return -1;
            }
            
            frameIndex <<= 1;
            int offset = (sprite[frameIndex] | sprite[frameIndex + 1] << 8) << 1;
            if (offset == 0x18444)
            {
                offset &= 0xFFFF;   // 截断只保留低位
            }

            return offset;
        }

        public Texture2D CreateTexture(byte[] sprite,int frameIndex,PaletteColor[] palette)
        {
            int offset = GetSpriteFrameOffset(sprite,frameIndex);
            if (offset < 0)
            {
                return null;
            }

            Texture2D texture = null;
            fixed (byte* ptr = sprite)
            {
                byte* bitmapOLE = ptr + offset;

                // skip the 0x00000002 in the file header
                if (*bitmapOLE == 0x02
                    && *(bitmapOLE + 1) == 0x00
                    && *(bitmapOLE + 2) == 0x00
                    && *(bitmapOLE + 3) == 0x00)
                {
                    bitmapOLE += 4;
                }

                int bitmapWidth = *(bitmapOLE) | *(bitmapOLE + 1) << 8;
                int bitmapHeight = *(bitmapOLE + 2) | *(bitmapOLE + 3) << 8;
                int bitmapLen = bitmapWidth * bitmapHeight;     // 一共有多少个像素
                Debug.Log("[sprite]bitmapWidth:" + bitmapWidth + " bitmapHeight:" + bitmapHeight);
                
                // 初始化为全透明
                texture = new Texture2D(bitmapWidth, bitmapHeight, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                for (int w = 0; w < bitmapWidth; w++)
                {
                    for (int h = 0; h < bitmapHeight; h++)
                    {
                        texture.SetPixel(w,h,new Color(0,0,0,0));
                    }
                }


                bitmapOLE += 4;     // 此时,指针指向像素数据

                
                int srcX = 0;
                int dy = 0;

                int i = 0;
                while(i < bitmapLen)
                {
                    byte T = *bitmapOLE;
                    bitmapOLE++;

                    if ((T & 0x80) > 0 && T <= 0x80 + bitmapWidth)
                    {
                        // 跳过操作, 仅更新坐标,不提取像素
                        int skipCount = T - 0x80;
                        i += skipCount;
                        srcX += skipCount;
                        
                        // 处理换行
                        while (srcX >= bitmapWidth)
                        {
                            srcX -= bitmapWidth;
                            dy++;
                        }
                    }
                    else
                    {
                        int pixelX = srcX;
                        int pixelY = dy;
                        int sx = srcX;
                        
                        for (int j = 0;j < T;j++)
                        {
                            // 绘制操作:提取原始颜色数据
                            byte rawColor = *(bitmapOLE + j);
                            //byte rawColor = (byte)(*(bitmapOLE + j) & 0x0F);
                            
                            
                            // 根据调色板,查询颜色
                            Color color = new Color();
                            PaletteColor paletteColor = palette[rawColor];
                            color.r = paletteColor.r / 255.0f;
                            color.g = paletteColor.g / 255.0f;
                            color.b = paletteColor.b / 255.0f;
                            color.a = 1.0f;
                            texture.SetPixel(pixelX, pixelY,color);
                            //Debug.Log($"[sprite]Pixel:({pixelX},{pixelY}) RawColor:{rawColor} ");

                            pixelX++;
                            sx++;

                            // 处理换行
                            if (sx >= bitmapWidth)
                            {
                                sx -= bitmapWidth;
                                pixelX = 0;
                                pixelY++;
                            }
                        }


                        // 移动指针,更新字节数
                        bitmapOLE += T;
                        i += T;
                        srcX += T;
                                                
                        // 处理换行
                        while (srcX >= bitmapWidth)
                        {
                            srcX -= bitmapWidth;
                            dy++;
                        }
                    }
                }
                
                Debug.Log("[sprite] sum i:" + i);
            }

            if (texture != null)
            {
                texture.Apply();
            }

            return texture;
        }
    }
}
