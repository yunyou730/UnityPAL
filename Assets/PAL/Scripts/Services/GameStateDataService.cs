using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 global.h struct tagGLOBALVARS 结构 
     */
    public class GameStateDataService : Service,IInitializable,IDestroyable
    {
        // 常量
        public static int MAX_PLAYERS_IN_PARTY = 3;         // 队伍里最多有3个人
        public static int MAX_PLAYER_ROLE = 5;              // 最多有多少个主角
        
        // 坐标信息
        public Vector2Int Viewport = Vector2Int.zero;       // 原游戏里,相机的坐标.应该是以像素为单位
        public Vector2Int PartyOffset = Vector2Int.zero;    // 原游戏里,用于表示 主角和viewport 的坐标偏移?应该是以像素为单位
        public int AtLayer = 0;                             // 原游戏里的 wLayer, 猜测和 主角所处的 map 哪个layer 有关. 每个 map 有 0,1 两层
        
        // 队伍信息
        public int MaxPartyMemberIndex = 0;   // 当前最大有效的 player index, 取值范围 [0,MAX_PLAYERS_IN_PARTY)
        
        // 调色板
        public int CurrentPaletteId = 0;        // 当前调色盘Id wNumPalette  
        public bool UseNightPalette = false;    // 是否使用 夜晚 调色板
        
        // 当前场景地图Id
        public int SceneId = 1;      // // 当前场景地图Id wNumScene
        
        // 主角队伍信息
        Party[] parties = new Party[MAX_PLAYER_ROLE];
        
        // RoleID 和 Sprite资源ID 的映射关系
        // key: roleId, value: spriteId sprite的资源id
        // 看起来应该是读的配置,是固定的
        private Dictionary<int, int> _roleIdToSpriteId = new Dictionary<int, int>
        {
            {0,2},
            {1,3},
            {2,7},
            {3,525},
            {4,5},
            {5,26},
        };
        
        public void Init()
        {
            // 主角队伍 目前只有1个人, 所以最大 sub index = 0
            MaxPartyMemberIndex = 0;
            
            // 主角队伍信息
            parties[0] = new Party(0,160,112,6);
            parties[1] = new Party(0,176,104,0);
            parties[2] = new Party(0,192,96,0);
            parties[3] = new Party(0,208,88,0);
            parties[4] = new Party(0,224,80,0);
        }

        public void Destroy()
        {
            
        }

        public void SetDebugCoord(Vector2Int viewportCoord,Vector2Int partyOffset)
        {
            Viewport = viewportCoord;
            PartyOffset = partyOffset;
        }

        public int GetSpriteIdByRoleId(int roleId)
        {
            if (_roleIdToSpriteId.ContainsKey(roleId))
            {
                return _roleIdToSpriteId[roleId];
            }
            return 0;
        }

        public Party GetPlayerParty(int index)
        {
            if (index > MaxPartyMemberIndex || index < 0)
            {
                return null;
            }
            return parties[index];
        }
    }
    
    // 参考 Global.h struct tagPARTY
    public class Party
    {
        public int RoleId = 0;  // player role
        public int CoordX = 0;  // position
        public int CoordY = 0;  
        public int FrameIndex = 6;  // current frame number
        public int FrameOffset = 0; // 原版注释: FIXME?? 所以猜测没用上

        public Party(int roleId,int coordX,int coordY,int frameIndex)
        {
            RoleId = roleId;
            CoordX = coordX;
            CoordY = coordY;
            FrameIndex = frameIndex;
        }
    }

    // 参考 Global.h struct tagTRAIL
    class Trail
    {
        public int X;
        public int Y;
        public int Direction;       // 应该有4个枚举方向
    }
}

