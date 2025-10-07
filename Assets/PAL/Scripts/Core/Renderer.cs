using ayy.pal.core;
using UnityEngine;

namespace ayy.pal.core
{
    public unsafe class Renderer
    {
        public static int GetSpriteFrameCount(byte[] sprite)
        {
            int frameCount = sprite[0] | sprite[1] << 8;
            return frameCount;
        }

        private static int GetSpriteFrameOffset(byte[] sprite,int frameIndex)
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

        // 作用: 传入 sprite 的 byte[] 数据, 以及 frame index, 把对应 frame index 的图像数据,保存为 Texture2D
        // 参数:
        //  参数1 sprite 的原始 byte[] 数据
        //  参数2 frame index,从 0 开始
        //  参数3 palette 调色板色值: 根据 颜色索引查找到的颜色值
        public static Texture2D CreateTexture(byte[] sprite,int frameIndex,PaletteColor[] palette)
        {
            int offset = GetSpriteFrameOffset(sprite,frameIndex);
            if (offset < 0)
            {
                return null;
            }

            Texture2D texture = null;
            fixed (byte* ptr = sprite)
            {
                byte* bitmapRLE = ptr + offset;

                // skip the 0x00000002 in the file header
                if (*bitmapRLE == 0x02
                    && *(bitmapRLE + 1) == 0x00
                    && *(bitmapRLE + 2) == 0x00
                    && *(bitmapRLE + 3) == 0x00)
                {
                    bitmapRLE += 4;
                }

                int bitmapWidth = *(bitmapRLE) | *(bitmapRLE + 1) << 8;
                int bitmapHeight = *(bitmapRLE + 2) | *(bitmapRLE + 3) << 8;
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


                bitmapRLE += 4;     // 此时,指针指向像素数据

                
                int srcX = 0;
                int dy = 0;

                int i = 0;
                while(i < bitmapLen)
                {
                    byte T = *bitmapRLE;
                    bitmapRLE++;

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
                            byte rawColor = *(bitmapRLE + j);
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
                        bitmapRLE += T;
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
        
        
        // 返回值颜色数组,
        //  R通道的 int值为 0-255 的颜色索引,
        //  A通道0表示原本无色,1表示有色 
        public static Color32[,] GetSpriteFrameColorData(byte[] sprite,int frameIndex)
        {
            int offset = GetSpriteFrameOffset(sprite,frameIndex);
            if (offset < 0)
            {
                return null;
            }

            Color32[,] ret = null;
            fixed (byte* ptr = sprite)
            {
                byte* bitmapRLE = ptr + offset;
                // skip the 0x00000002 in the file header
                if (*bitmapRLE == 0x02
                    && *(bitmapRLE + 1) == 0x00
                    && *(bitmapRLE + 2) == 0x00
                    && *(bitmapRLE + 3) == 0x00)
                {
                    bitmapRLE += 4;
                }

                int bitmapWidth = *(bitmapRLE) | *(bitmapRLE + 1) << 8;
                int bitmapHeight = *(bitmapRLE + 2) | *(bitmapRLE + 3) << 8;
                int bitmapLen = bitmapWidth * bitmapHeight;     // 一共有多少个像素
                ret = new Color32[bitmapWidth,bitmapHeight];
                
                Debug.Log("[sprite]bitmapWidth:" + bitmapWidth + " bitmapHeight:" + bitmapHeight);
                bitmapRLE += 4;     // 此时,指针指向像素数据

                
                int srcX = 0;
                int dy = 0;
                int i = 0;
                while(i < bitmapLen)
                {
                    byte T = *bitmapRLE;
                    bitmapRLE++;

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
                            // 提取原始颜色数据
                            byte rawColor = *(bitmapRLE + j);
                            
                            // @miao @todo
                            if (pixelX >= ret.GetLength(0) || pixelY >= ret.GetLength(1) || pixelX < 0 || pixelY < 0)
                            {
                                Debug.LogWarning($"pixelX:{pixelX},pixelY:{pixelY},lenD0:{ret.GetLength(0)},lenD1:{ret.GetLength(1)}");
                            }
                            else
                            {
                                ret[pixelX, pixelY] = new Color32(rawColor,0,0,255);                                
                            }

                            //float data = (float)rawColor / 255.0f;
                            //Color color = new Color(data,0.0f,0.0f,1.0f);
                            //texture.SetPixel(pixelX, pixelY,color);
                            //ret[pixelX, pixelY] = color;
                            
                            
                            
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
                        bitmapRLE += T;
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
                //Debug.Log("[sprite] sum i:" + i);
            }
            return ret;
        }
    }
}
