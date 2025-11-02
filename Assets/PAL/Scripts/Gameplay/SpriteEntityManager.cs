using System.Collections.Generic;
using UnityEngine;

namespace ayy.pal
{
    // 管理所有 SpriteEntity
    public class SpriteEntityManager : Service,IInitializable,IDestroyable
    {
        private int _nextSpriteEntityKey = 1;
        private Dictionary<int, SpriteEntity> _spriteEntitiesMap = null;
        
        public void Init()
        {
            Debug.Log("SpriteEntityManager::Init called");
            _spriteEntitiesMap = new Dictionary<int,SpriteEntity>();
        }
        
        public void Destroy()
        {
            // cleanup sprites
            if (_spriteEntitiesMap != null)
            {
                foreach (var spriteEntity in _spriteEntitiesMap.Values)
                {
                    spriteEntity.Dispose();
                }
                _spriteEntitiesMap.Clear();
            }
        }
        
        public int CreateSpriteEntity(int spriteId)
        {
            int spriteEntityKey = _nextSpriteEntityKey++;
            var spriteEntity = new SpriteEntity(spriteEntityKey,spriteId);
            _spriteEntitiesMap.Add(spriteEntity.Key, spriteEntity);
            return spriteEntityKey;
        }

        public SpriteEntity GetSpriteEntity(int spriteEntityKey)
        {
            if (_spriteEntitiesMap != null && _spriteEntitiesMap.ContainsKey(spriteEntityKey))
            {
                return _spriteEntitiesMap[spriteEntityKey];
            }
            return null;
        }
    }
}

