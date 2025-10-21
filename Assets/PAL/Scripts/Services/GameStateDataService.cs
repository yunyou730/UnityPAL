using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 global.h struct tagGLOBALVARS 结构 
     */
    public class GameStateDataService : Service,IInitializable,IDestroyable
    {
        
        // 最多有效的主角 index 下标是多少. 0代表1个人, 1代表2个人,以此类推
        public int MaxPartyMemberIndex = 0;

        // 是否使用 夜晚 调色板
        public bool UseNightPalette = false;

        // 当前场景Id
        public int SceneId = 1;
        
        public void Init()
        {
            
        }

        public void Destroy()
        {
            
        }
    }
    
    // 参考 Global.h strucct tagPARTY
    class Party
    {
        public int RoleId = 0;  // player role
        public int CoordX = 0;  // position
        public int CoordY = 0;  
        public int FrameIndex = 6;  // current frame number
        public int FrameOffset = 0; // not used ?
    }
}

