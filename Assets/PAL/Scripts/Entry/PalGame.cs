using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ayy.pal
{
    public class PalGame : MonoBehaviour
    {
        [SerializeField] private GameObject _viewportPrefab = null;
        
        private static PalGame sInstance = null;
        Dictionary<Type,Service> _services = new Dictionary<Type, Service>();
        
        List<IInitializable> _initializables = new List<IInitializable>();
        List<IUpdateable> _updateables = new List<IUpdateable>();
        List<IDestroyable> _destroyables = new List<IDestroyable>();

        void Awake()
        {
            sInstance = this;
            AddService<PaletteService>(new PaletteService());
            AddService<MapService>(new MapService());
            AddService<SpriteService>(new SpriteService());
            AddService<ViewportService>(new ViewportService(_viewportPrefab));
            AddService<GameStateDataService>(new GameStateDataService());
            AddService<TestStateDataService>(new TestStateDataService());
            AddService<LoadGameService>(new LoadGameService());
            
            foreach (var service in _initializables)
            {
                service.Init();
            }
        }

        void OnDestroy()
        {
            foreach (var service in _destroyables)
            {
                service.Destroy();
            }
            sInstance = null;
        }

        void Start()
        {
        }
        
        void Update()
        {
            foreach (var service in _updateables)
            {
                service.Update();
            }
        }

        public static PalGame GetInstance()
        {
            return sInstance;
        }

        public T GetService<T>() where T : Service
        {
            Type t = typeof(T);
            if (_services.ContainsKey(t))
            {
                return _services[t] as T;
            }
            return null;
        }

        private void AddService<T>(T service) where T : Service
        {
            Type t = typeof(T);
            _services.Add(t,service);

            if(service is IInitializable)
            {
                _initializables.Add((IInitializable)service);
            }
            if (service is IUpdateable)
            {
                _updateables.Add((IUpdateable)service);
            }

            if (service is IDestroyable)
            {
                _destroyables.Add((IDestroyable)service);
            }
        }
    }

}

