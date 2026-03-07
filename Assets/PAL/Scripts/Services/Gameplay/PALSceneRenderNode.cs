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
            int worldPixelX = mapTileCoord.TileX * 32 + mapTileCoord.TileH * 16 - 16;
            int worldPixelY = mapTileCoord.TileY * 16 + mapTileCoord.TileH * 8 + 7 + atLayer;
            
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
            const float zScaleFactor = 0.02f;
            foreach (RenderNode renderNode in _renderNodes)
            {
                
                float z = -renderNode.ViewPixelY * zScaleFactor;
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

