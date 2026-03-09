namespace ayy.pal
{
    /*
     * 参考 global.h tagSCENE
     */
    public class PALSceneRecord
    {
        public int MapNum;                // number of map
        public int ScriptOnEnter;         // when entering this scene, execute script from here
        public int ScriptOnTeleport;      // when teleporting out of this scene, execute script from here
        public int EventObjectIndex;      // event objects in this scene begins from number wEventObjectIndex + 1
    }
}

