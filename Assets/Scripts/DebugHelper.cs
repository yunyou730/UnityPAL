
using System.Collections.Generic;
using ayy.pal;
using UnityEngine;
using ayy.pal.core;
using Renderer = ayy.pal.core.Renderer;

public class DebugHelper
{
    // public static byte[] GetMKFDecompressedBytesData(MKFLoader mkf,int chunkIndex)
    // {
    //     // 获取原始 sprite 数据 ,并解压缩
    //     //byte[] sprite = _spriteService.GetMgoMKF().ReadChunk(spriteIndex);
    //     int decompressedSize = mkf.GetDecompressedSize(spriteIndex);
    //     byte[] decompressedSprite = new byte[decompressedSize];
    //     fixed (byte* pChunkData = sprite)
    //     {
    //         fixed (byte* pDestData = decompressedSprite)
    //         {
    //             Yj1Decompressor.YJ1_Decompress(pChunkData, pDestData,decompressedSize);
    //         }
    //     }
    // }

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

    // 根据 sprite的 二进制数据, 返回一个大的包含每个frame的  Texture2D, 以及每个 frame 的UV 坐标
    public static void CreateSprite(byte[] spriteData,out Texture2D texture,out List<SpriteFrameUV> uvList,PaletteColor[] paletteColors)
    {
        // @miao @todo
        uvList = new List<SpriteFrameUV>();
        //texture = new Texture2D(1, 1);
        texture = null;

        int texWidth = 0;
        int texHeight = 0;
        int widthStep = 10; // 在大图里,尝试每隔10个像素,放置一个 SpriteFrame
        int frameCount = Renderer.GetSpriteFrameCount(spriteData);
        
        int currentX = 0;
        int currentY = 0;
        
        uvList = new List<SpriteFrameUV>(frameCount);
        for (int frameIndex = 0;frameIndex < frameCount;frameIndex++)
        {
            // 忽略最后一帧, 因为最后一帧是错乱的图像
            if (frameIndex == frameCount - 1)
            {
                break;
            }
            
            // 获取 frame 的 图像像素数据
            Color32[,] colorData = Renderer.GetSpriteFrameColorData(spriteData,frameIndex,paletteColors);
            
            // frame 宽高 
            int frameWidth = colorData.GetLength(0);
            int frameHeight = colorData.GetLength(1);

            if (frameWidth <= 0 || frameHeight <= 0)
            {
                // 忽略 非法的 frame index
                Debug.LogWarning($"invalid frame index:{frameIndex}");
                continue;
            }
            
            if (uvList.Count == 0)  // 第一个 有效帧
            {
                // 第一帧 frame, 要创建整个 texture sheet 
                GetSpriteTextureSheetSize(frameCount, frameWidth, frameHeight,widthStep,out texWidth,out texHeight);
                texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
                for (int x = 0;x < texWidth;x++)
                {
                    for (int y = 0;y < texHeight;y++)
                    {
                        texture.SetPixel(x,y,new Color(0, 0, 0, 0));
                    }
                }
                
                //currentY = texHeight - 1;
                //currentY = 5;       // @miao @temp
                currentY = 0;
            }

            // 把当前帧的图像数据, 设置到 texture sheet 上 
            for (int y = 0; y < frameHeight; y++)
            {
                for (int x = 0;x < frameWidth;x++)
                {
                    // 当前帧的 每个像素坐标, 存储在 texture 里 
                    int coordX = currentX + x;
                    int coordY = currentY + y;
                    coordY = texHeight - 1 - currentY - y;
                    texture.SetPixel(coordX,coordY,colorData[x,y]);
                    
                    // @miao @todo , 这里有问题，有可能涉及到 flip-y
                    // 每帧的UV数据,存储起来
                    var frameAtlas = new SpriteFrameUV();
                    frameAtlas.U = coordX;
                    frameAtlas.V = coordY;
                    uvList.Add(frameAtlas);
                }
            }
            currentX = currentX + frameWidth + widthStep;

        }
        
        if(texture != null)
        {
            texture.Apply();
        }
    }

    // texture 的宽度: 用第一帧的宽度 x 帧数量, 并增加 （frameCount - 1）个widthStep 间隔宽度
    // texture 的高度: 第一帧高度的 2倍, 看看是不是能放得下
    private static void GetSpriteTextureSheetSize(int frameCount,int firstFrameWidth,int firstFrameHeight,int frameWidthStep,out int textureWidth,out int textureHeight)
    {
        textureWidth = firstFrameWidth * frameCount + (frameCount - 1) * frameWidthStep;
        textureHeight = firstFrameHeight * 2;
    }
}
