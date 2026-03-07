using System.Collections.Generic;
using ayy.pal.core;
using Unity.VisualScripting;
using UnityEngine;

namespace ayy.pal
{
    public enum ERenderNodeType
    {
        Sprite,
        Tile,
    }

    public struct RenderNode
    {
        public ERenderNodeType NodeType;

        //public int SpriteEntityKey;        // 仅 Sprite 用
        public SpriteEntity SpriteEntity;    // 仅 Sprite 用
        public MapTileCoord TileCoord;      // 仅 Tile 用

        public int ViewPixelX;
        public int ViewPixelY;
        public int AtLayer;
    }

    public class RenderOrderManager
    {
        private List<RenderNode> _renderNodes = new List<RenderNode>(); // 考虑 增加预设 capacity 避免扩容
        
        // 统计出来,所有 可能覆盖sprite 的 tiles
        // private List<MapTileCoord> _modifiedTileCoords = new List<MapTileCoord>();

        public void CollectSpriteNode(SpriteEntity spriteEntity)
        {
            RenderNode node = new RenderNode();
            node.NodeType =  ERenderNodeType.Sprite;
            node.SpriteEntity = spriteEntity;
            //node.SpriteEntityKey = spriteEntity.Key;

            // 不一定都用得上, 主要是 pixel Y 用的上应该是 
            node.ViewPixelX = spriteEntity.GetPixelX();
            node.ViewPixelY = spriteEntity.GetPixelY();
            node.AtLayer = spriteEntity.GetLogicLayer();

            _renderNodes.Add(node);
        }
        
        public void CollectTileNode(MapTileCoord mapTileCoord,int tileLogicHeight,int viewportX,int viewportY)
        {
            RenderNode node = new RenderNode();
            node.NodeType =  ERenderNodeType.Tile;
            node.TileCoord = mapTileCoord;
            
            // tile左上角的 世界像素坐标
            int mapLayer = (int)mapTileCoord.TileLayer;
            int atLayer = tileLogicHeight * 8 + mapLayer;
            
            
            /*
             * 这里,几个 magic number 做一下注释
             *
             * worldPixelX
             * 先计算 TileX * 32, 是因为 每个 tile 的 width 是 32;
             * 再用 TileH * 16,
             *      是因为 如果 TileH 是1， 会在  "横坐标" 上贡献 半个Tile.width ,也就是 32/2 = 16 个 横坐标；
             *      TileH坐标哦是 0, 则横坐标无贡献
             *
             * 最后减去 16, 是减去了 Tile.Width 的一半.
             * 相当于, 原本计算的 是 tile coord 菱形中心的坐标. 减去 16之后, 就变成了 左侧的 pixel 坐标 
             *
             * worldPixelY
             * 用 TileY * 16, 这里的 16 是  Tile.Height
             * + TileH * 8,
             *      是因为 TileH 坐标, 如果是0，则 y方向贡献为0;
             *      TileH 坐标，如果是 1， 则 y方向贡献为  半个 tile.height, 也就是 16/2 = 8
             * 最后 +7 , 这里 7 ,应该是为了  "向下" ,获取 tile 底部像素,的  pixelY. 相当于取的是 "Tile 左下角" 的 像素坐标
             * 再 + atLayer, 应该是逻辑上, 还需要这个 layer 来修正 
             */
            int worldPixelX = mapTileCoord.TileX * MapWrapper.kTilePixelsW + mapTileCoord.TileH * MapWrapper.kTilePixelsW / 2 - MapWrapper.kTilePixelsW / 2;
            int worldPixelY = mapTileCoord.TileY * MapWrapper.kTilePixelSizeH + mapTileCoord.TileH * MapWrapper.kTilePixelSizeH / 2 + MapWrapper.kTilePixelsH / 2 + atLayer;
            
            node.ViewPixelX = worldPixelX - viewportX;
            node.ViewPixelY = worldPixelY - viewportY;
            node.AtLayer = atLayer;

            _renderNodes.Add(node);
        }

        public void ClearRenderOrder(MapWrapper mapWrapper)
        {
            ResetModifiedTiles(mapWrapper);
            _renderNodes.Clear();
        }

        /*
         * 参考 SDLPAL scene.c
         * PAL_SceneDrawSprites()
         * "All sprites are now in our array; sort them by their vertical positions."
         */
        public void SortRenderOrder(
            PALMap palMap,
            MapWrapper mapWrapper,
            int viewportX,
            int viewportY )
        {
            //const float zScaleFactor = 0.02f;
            foreach (RenderNode renderNode in _renderNodes)
            {
                
                float z = -renderNode.ViewPixelY * PalConst.Z_SCALE_FACTOR;
                switch (renderNode.NodeType)
                {
                    case ERenderNodeType.Sprite:
                    {
                        // @miao @todo
                        SpriteEntity spriteEntity = renderNode.SpriteEntity;
                        //spriteEntity.SetSpriteZ(z);
                        spriteEntity.ApplyPixelPos(viewportX,viewportY,z);
                        
                        
                        break;
                    }
                    case ERenderNodeType.Tile:
                    {
                        MapTileCoord tileCoord = renderNode.TileCoord;
                        
                        // 顶点色
                        mapWrapper.SetTileVertexColor(ETileLayer.Top,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.yellow);
                        mapWrapper.SetTileVertexColor(ETileLayer.Bottom,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.yellow);
                        
                        // z
                        mapWrapper.SetTileVertPosZ(tileCoord.TileLayer, tileCoord.TileX, tileCoord.TileY, tileCoord.TileH, z);
                        break;
                    }
                }
            }
        }
        
        public void ApplyRenderOrder(PALMap palMap,MapWrapper mapWrapper)
        {
            // 所有 sprite ,都 apply 一次 ？还是过程中就 apply 了比较合适..
            // 对于 sprite 来说, 应该是后者 
            
            // 最后,集中 让 mapWrapper 的顶点属性, 生效一次. 避免每个 tile 独立生效
            mapWrapper.ApplyTileVertexColorsChange();
            mapWrapper.ApplyTileVertPosZChange();
        }

        
        private void ResetModifiedTiles(MapWrapper mapWrapper)
        {
            foreach (RenderNode renderNode in _renderNodes)
            {
                switch (renderNode.NodeType)
                {
                    case ERenderNodeType.Tile:
                    {
                        MapTileCoord tileCoord = renderNode.TileCoord;
                        
                        mapWrapper.SetTileVertexColor(ETileLayer.Top,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.black);
                        mapWrapper.SetTileVertexColor(ETileLayer.Bottom,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,Color.black);
                        
                        float z = mapWrapper.GetZForTileLayerType(tileCoord.TileLayer,false);
                        mapWrapper.SetTileVertPosZ(tileCoord.TileLayer,tileCoord.TileX,tileCoord.TileY,tileCoord.TileH,z);
                        
                        break;
                    }
                }
            }
        }
        
    }
}

