using System.Collections.Generic;
using UnityEngine;

namespace ayy.pal
{
    public class InputManager : Service,IInitializable,IUpdateable,IDestroyable
    {
        private EPALDirection _inputDir = EPALDirection.Unknown;
        private GameStateDataService _gameStateDataService = null;

        public void Init()
        {
            _gameStateDataService = PalGame.GetInstance().GetService<GameStateDataService>();
        }

        public void Destroy()
        {
            
        }

        public void Update()
        {
            _inputDir = EPALDirection.Unknown;
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _inputDir = EPALDirection.South;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _inputDir = EPALDirection.West;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _inputDir = EPALDirection.North;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _inputDir = EPALDirection.East;
            }
        }
        
        
        public EPALDirection GetInputDir()
        {
            return _inputDir;
        }
    }
}

