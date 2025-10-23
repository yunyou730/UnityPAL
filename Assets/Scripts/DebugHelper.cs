
using System.Collections.Generic;
using ayy.pal;
using UnityEngine;
using ayy.pal.core;
using Renderer = ayy.pal.core.Renderer;

public class DebugHelper
{
    public static void CreateSpriteFramesGameObjects(byte[] sprite,PaletteColor[] paletteColors,GameObject prefab,Transform root)
    {
        int spriteFrameCount = Renderer.GetSpriteFrameCount(sprite);
        Texture2D[] textures = new Texture2D[spriteFrameCount];
        float baseY = 0.0f;
        for (int frameIndex = 0; frameIndex < spriteFrameCount; frameIndex++)
        {
            Texture2D tex = Renderer.CreateTexture(sprite, frameIndex,paletteColors);
            if (tex != null)
            {
                var go = GameObject.Instantiate(prefab);
                go.name = "sprite_frame[" + frameIndex + "]";
                go.transform.SetParent(root);
                float sizeX = go.transform.localScale.x;
                float sizeY = tex.height / (float)tex.width * sizeX;
                go.transform.localPosition = new Vector3(0, baseY + frameIndex * sizeY, 0);
                go.transform.localScale = new Vector3(sizeX, sizeY, 1.0f);
                var mat = go.GetComponent<MeshRenderer>().material;
                mat.SetTexture(Shader.PropertyToID("_Texture2D"), tex);
            }
            else
            {
                Debug.LogWarning("null sprite frame");
            }
        }
    }
    
    public static void CreateSpriteV2(byte[] spriteData,
        PaletteColor[] paletteColors,
        out Texture2D texture,
        out List<PALSpriteFrame> spriteFrameList)
    {
        texture = null;

        int texWidth = 0;
        int texHeight = 0;
        int widthStep = 10; // 在大图里,尝试每隔10个像素,放置一个 SpriteFrame
        int frameCount = Renderer.GetSpriteFrameCount(spriteData);
        int validFramesCount = 0;
        
        // 先遍历每一个 frame ,收集 maxFrameWidth, maxFrameHeight 信息
        int maxFrameWidth = 0;
        int maxFrameHeight = 0;
        List<Color32[,]> framesColorData = new List<Color32[,]>(); // 存储每帧的颜色数据
        spriteFrameList = new List<PALSpriteFrame>();
        int totalValidFramesWidth = 0;
        for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) // 忽略最后一帧,最后一帧通常是错误帧
        {
            // 获取数据
            int frameWidth,frameHeight;
            Renderer.GetSpriteFrameSize(spriteData,frameIndex,out frameWidth,out frameHeight);
            
            // 这里判断,当前帧是否合法
            bool bValidFrame = (frameWidth <= 1024 && frameHeight <= 1024 && frameWidth > 0 && frameHeight > 0); // @miao @todo, 这里临时用这种方式来判断!!
            if (bValidFrame && frameIndex == frameCount - 1 && spriteFrameList.Count > 0)   // 针对最后1帧做额外判断,看是否合法
            {
                int firstFrameWidth = spriteFrameList[0].W;
                int firstFrameHeight = spriteFrameList[0].H;
                // 如果某一个 frame 的高度,超过了第一个 frame高度的10倍以上,则认为是非法frame 
                if (frameHeight > 2 * firstFrameHeight || frameWidth > 2 * firstFrameWidth) // @miao @todo, 这里临时写死，如果 > 2倍,则视为非法
                {
                    bValidFrame = false;
                }
            }

            if (!bValidFrame)
            {
                Debug.LogWarning("we meet invalid sprite frame,height:" + frameHeight);
                break;
            }

            validFramesCount++;
            totalValidFramesWidth += frameWidth;
            
            // 暂存颜色数据
            Color32[,] colorData = Renderer.GetSpriteFrameColorData(spriteData, frameIndex, paletteColors);
            framesColorData.Add(colorData);
            
            // 添加 SpriteFrame 数据
            PALSpriteFrame frame = new PALSpriteFrame();
            frame.SetFrameSize(frameWidth, frameHeight);
            spriteFrameList.Add(frame);

            // 更新 max frame size
            maxFrameWidth = frameWidth > maxFrameWidth ? frameWidth : maxFrameWidth;
            maxFrameHeight = frameHeight > maxFrameHeight ? frameHeight : maxFrameHeight;
        }


        int frameMargin = 5;
        GetSpriteTextureSheetSizeV2(validFramesCount,
            totalValidFramesWidth,
            maxFrameHeight,
            frameMargin,
            out texWidth,
            out texHeight);

        if (texWidth == 0 || texHeight == 0)
        {
            texture = null;
            Debug.LogWarning("tex width or height is zero,ignore sprite");
            return;
        }

        texture = CreateEmptyTexture(texWidth, texHeight);
        
        int curX = 0;
        int curY = 0;
        for (int frameIndex = 0;frameIndex < validFramesCount;frameIndex++)
        {
            PALSpriteFrame frame = spriteFrameList[frameIndex];
            
            // @miao @todo
            // 这里给 frame 补充 UV 数据
            
            // 这里要把 frameData的图像数据, copy 到 texture 里
            Color32[,] frameColorData = framesColorData[frameIndex];
            int frameWidth = frame.W;
            int frameHeight = frame.H;

            for (int ox = 0;ox < frameWidth;ox++)
            {
                for (int oy = 0;oy < frameHeight;oy++)
                {
                    int coordX = curX + ox;
                    int coordY = curY + oy;
                    coordY = texHeight - 1 - curY - oy;
                    texture.SetPixel(coordX,coordY,frameColorData[ox,oy]);
                }
            }
            curX = curX + frameWidth + frameMargin;
        }
        texture.Apply();
    }
    
    private static void GetSpriteTextureSheetSizeV2(int validFramesCount,int totalFrameWidth,int maxFrameHeight,int frameMargin,out int textureWidth,out int textureHeight)
    {
        textureWidth = totalFrameWidth + (validFramesCount - 1) * frameMargin;  // 所有帧宽度 + (帧数-1) x 帧间隙
        textureHeight = maxFrameHeight;
    }
    
    private static Texture2D CreateEmptyTexture(int texWidth,int texHeight)
    {
        Texture2D texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        for (int x = 0;x < texWidth;x++)
        {
            for (int y = 0;y < texHeight;y++)
            {
                texture.SetPixel(x,y,new Color(0, 0, 0, 0));
            }
        }
        return texture;
    }
}
