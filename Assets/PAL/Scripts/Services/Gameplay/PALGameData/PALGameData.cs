using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 SDLPal global.h 代码
     * 作为 tagGLOBALVARS 的 g 成员
     * 具体实现, 参考 glboal.h 的 tagGAMEDATA
     */
    public class PALGameData
    {
        // 玩家角色
        public PALPlayerRole[] PlayerRoles = new PALPlayerRole[PALGameConst.MAX_PLAYER_ROLES];

        // 场景数据
        PALSceneRecord[] SceneRecords = new PALSceneRecord[PALGameConst.MAX_SCENES];

        public PALGameData()
        {
            // init player roles
            for (int i = 0; i < PlayerRoles.Length; i++)
            {
                PlayerRoles[i] = new PALPlayerRole();
            }
            
            // init scene records
            // @miao @todo
            
        }
    }
}

