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
        
        // 坐标信息
        public Vector2Int Viewport = Vector2Int.zero;       // 原游戏里,相机的坐标.应该是以像素为单位
        public Vector2Int PartyOffset = Vector2Int.zero;    // 原游戏里,用于表示 主角和viewport 的坐标偏移?应该是以像素为单位
        public int AtLayer = 0;                             // 原游戏里的 wLayer, 猜测和 主角所处的 map 哪个layer 有关. 每个 map 有 0,1 两层
        
        // 队伍信息
        public int MaxPartyMemberIndex = 0;   // 最大有效的 player index, 取值范围 [0,MAX_PLAYERS_IN_PARTY)
        
        public bool UseNightPalette = false;    // 是否使用 夜晚 调色板
        public int SceneId = 1;     // 当前场景Id
        
        public void Init()
        {
            
        }

        public void Destroy()
        {
            
        }

        public void SetDebugCoord(Vector2Int viewportCoord,Vector2Int partyOffset)
        {
            Viewport = viewportCoord;
            PartyOffset = partyOffset;
        }
    }
    
    // 参考 Global.h struct tagPARTY
    class Party
    {
        public int RoleId = 0;  // player role
        public int CoordX = 0;  // position
        public int CoordY = 0;  
        public int FrameIndex = 6;  // current frame number
        public int FrameOffset = 0; // not used ?
    }

    // 参考 Global.h struct tagTRAIL
    class Trail
    {
        public int X;
        public int Y;
        public int Direction;       // 应该有4个枚举方向
    }
}

