using System.Collections.Generic;
using UnityEngine;

namespace ayy.pal
{
    public class InputManager : Service,IInitializable,IUpdateable,IDestroyable
    {
        public enum EInputDir
        {
            South = 0,
            West,
            North,
            East,
            Unknown
        }
        
        private EInputDir _inputDir = EInputDir.Unknown;

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
            _inputDir = EInputDir.Unknown;
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _inputDir = EInputDir.South;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _inputDir = EInputDir.West;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _inputDir = EInputDir.North;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _inputDir = EInputDir.East;
            }
        }
        
        

        public void ApplyInput()
        {
            ApplyMoveByDirection();
        }


        private void ApplyMoveByDirection()
        {
            if (_inputDir == EInputDir.Unknown)
            {
                return;
            }
            int xOffset = (_inputDir == EInputDir.West || _inputDir == EInputDir.South) ? -16 : 16;
            int yOffset = ((_inputDir == EInputDir.West || _inputDir == EInputDir.North) ? -8 : 8);
            
            //int xSource = _gameStateDataService.ViewportX + _gameStateDataService.


            int prevX = _gameStateDataService.ViewportX;
            int prevY = _gameStateDataService.ViewportY;
            int nextX = prevX + xOffset;
            int nextY = prevY + yOffset;
            _gameStateDataService.SetViewportXY(nextX, nextY);
        }
    }
}

