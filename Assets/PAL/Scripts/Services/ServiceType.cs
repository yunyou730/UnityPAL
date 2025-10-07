using UnityEngine;

namespace ayy.pal
{
    public interface IInitializable
    {
        public void Init();
    }

    public interface IUpdateable
    {
        public void Update();
    }

    public interface IDestroyable
    {
        public void Destroy();
    }
}

