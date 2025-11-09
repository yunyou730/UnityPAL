using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    /*
     * 参考 palcommon.h 里的常量
     * 
     */
    class PALGameConst
    {
        // maximum number of players in party
        public static int MAX_PLAYERS_IN_PARTY = 3;
        // total number of possible player roles
        public static int MAX_PLAYER_ROLES = 6;
        // totally number of playable player roles
        public static int MAX_PLAYABLE_PLAYER_ROLES = 5;
        // maximum entries of inventory
        public static int MAX_INVENTORY = 256;
        // maximum items in a store
        public static int MAX_STORE_ITEM = 9;
        // total number of magic attributes
        public static int NUM_MAGIC_ELEMENTAL = 5;
        // maximum number of enemies in a team
        public static int MAX_ENEMIES_IN_TEAM = 5;
        // maximum number of equipments for a player
        public static int MAX_PLAYER_EQUIPMENTS = 6;
        // maximum number of magics for a player
        public static int MAX_PLAYER_MAGICS = 32;
        // maximum number of scenes
        public static int MAX_SCENES = 300;
        // maximum number of objects
        public static int MAX_OBJECTS = 600;
        // maximum number of event objects (should be somewhat more than the original,
        // as there are some modified versions which has more)
        public static int MAX_EVENT_OBJECTS = 5500;
        // maximum number of effective poisons to players
        public static int MAX_POISONS = 16;
        // maximum number of level
        public static int MAX_LEVELS = 99;
        
        public static int MINIMAL_WORD_COUNT = (MAX_OBJECTS + 13);
        public static int PAL_CDTRACK_BASE = 10000;
        public static int PAL_RLEBUFSIZE = 64000;
    }
    
    public enum EPALDirection
    {
        South = 0,
        West,
        North,
        East,
        Unknown
    }
}

