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
        public PALPlayerRole[] PlayerRoles = new PALPlayerRole[PALGameConst.MAX_PLAYER_ROLES];

        public PALGameData()
        {
            for (int i = 0; i < PlayerRoles.Length; i++)
            {
                PlayerRoles[i] = new PALPlayerRole();
            }
        }
    }

    /* 参考 global.h tagPLAYERROLES
     * 但差异是，SDLPal 这里,是一个 class ,里面所有成员都是数组
     * 这里， 是一个 class只保存1个成员的数据, 整体作为一个数组
     */
    public class PALPlayerRole
    {
        public int Avatar;              // avatar, shown in status view
        public int SpriteNumInBattle;   // sprite displayed in battle (in F.MKF)
        public int SpriteNum;           // sprite displayed in normal scene (in MOG.MKF)
        public int Name;                // name of player class (in WORD.DAT)
        public int AttackAll;           // whether player can attack everyone in a bulk or not
        public int Unknown1;            // FIXME: ???
        public int Level;               // level
        public int MaxHP;               // maximum HP
        public int rgwHP;               // current HP
        public int MP;                  // current MP
   // WORD               rgwEquipment[MAX_PLAYER_EQUIPMENTS][MAX_PLAYER_ROLES]; // equipments
   // PLAYERS            rgwAttackStrength;     // normal attack strength
   // PLAYERS            rgwMagicStrength;      // magical attack strength
   // PLAYERS            rgwDefense;            // resistance to all kinds of attacking
   // PLAYERS            rgwDexterity;          // dexterity
   // PLAYERS            rgwFleeRate;           // chance of successful fleeing
   // PLAYERS            rgwPoisonResistance;   // resistance to poison
   // WORD               rgwElementalResistance[NUM_MAGIC_ELEMENTAL][MAX_PLAYER_ROLES]; // resistance to elemental magics
   // PLAYERS            rgwUnknown2;           // FIXME: ???
   // PLAYERS            rgwUnknown3;           // FIXME: ???
   // PLAYERS            rgwUnknown4;           // FIXME: ???
   // PLAYERS            rgwCoveredBy;          // who will cover me when I am low of HP or not sane
   // WORD               rgwMagic[MAX_PLAYER_MAGICS][MAX_PLAYER_ROLES]; // magics
        public int WalkFrames;         // walk frame (???)
   // PLAYERS            rgwCooperativeMagic;   // cooperative magic
   // PLAYERS            rgwUnknown5;           // FIXME: ???
   // PLAYERS            rgwUnknown6;           // FIXME: ???
   // PLAYERS            rgwDeathSound;         // sound played when player dies
   // PLAYERS            rgwAttackSound;        // sound played when player attacks
   // PLAYERS            rgwWeaponSound;        // weapon sound (???)
   // PLAYERS            rgwCriticalSound;      // sound played when player make critical hits
   // PLAYERS            rgwMagicSound;         // sound played when player is casting a magic
   // PLAYERS            rgwCoverSound;         // sound played when player cover others
   // PLAYERS            rgwDyingSound;         // sound played when player is dying


        // 走路动画有多少个 frames. 可能是 4, 也可能是3 
       public int GetWalkFramesCount()
       {
           int ret = WalkFrames;
           if (ret == 0)
           {
               ret = 3;
           }
           return ret;   
       }
    }
}

