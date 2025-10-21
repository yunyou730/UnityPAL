using System.Collections.Generic;
using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class TestStateDataService : Service,IInitializable,IDestroyable
    {
        private List<Party> _parties = null;
        public void Init()
        {
            _parties = new List<Party>();
            
            var party = new Party();
            party.RoleId = 0;
            party.CoordX = 160;
            party.CoordY = 112;
            party.FrameIndex = 6;
            party.FrameOffset = 0;
        }

        public void Destroy()
        {
            
        }
    }
}

