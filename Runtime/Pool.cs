using System;
using System.Collections.Generic;
using Mek.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mek.ObjectPooling
{
    public class Pool<T> : PoolBase where T : Object
    {
        private Transform Parent { get; set; }
        private T Prefab { get; }

        private readonly List<T> _activeItems = new List<T>();
        private readonly Stack<T> _inactiveItems = new Stack<T>();

        public Pool(T prefab, bool invokeInitializeEvent = true)
        {
            Object = prefab;
            Prefab = prefab;
            if (invokeInitializeEvent)
            {
                PoolInitialized?.Invoke(this);
            }
        }

        public Pool<T> AsParent(Transform parent)
        {
            Parent = parent;
            return this;
        }

        public Pool<T> WithInitialSize(int initialSize = 0)
        {
            var size = _activeItems.Count + _inactiveItems.Count;
            if (size < initialSize)
            {
                var delta = initialSize - size;

                for (int i = 0; i < delta; i++)
                {
                    //todo: spawn as inactive
                    var item = Create();
                    SetGameObjectOfItemActive(item, false);
                }
            }

            return this;
        }

        public Pool<T> WithId(string id)
        {
            if (Id == "")
            {
                Id = id;
            }

            return this;
        }

        private T Create(bool asInactive = true)
        {
            T item = null;
            item = Object.Instantiate(Prefab, Parent);
            if (asInactive)
            {
                _inactiveItems.Push(item);
            }

            var go = GetGameObject(item);
            var poolObject = go.AddComponent<PoolObject>();
            poolObject.Init(Prefab.GetHashCode());
            
            OnCreated(item);
            
            if (item is ICreationCallbackReceiver creationCallbackReceiver)
            {
                creationCallbackReceiver.OnCreated();
            }
            return item;
        }

        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn()
        {
            T item = null;
            if (_inactiveItems.Count == 0)
            {
                item = Create(false);
            }
            else
            {
                while (item == null && _inactiveItems.Count > 0) // Cleanup loop for destroyed items
                {
                    item = _inactiveItems.Pop();
                }

                if (item == null)
                {
                    item = Create();
                }
            }
            
            SetGameObjectOfItemActive(item, true);
            _activeItems.Add(item);
            
            OnSpawned(item);
            
            if (item is ISpawnCallbackReceiver spawnCallbackReceiver)
            {
                spawnCallbackReceiver.OnSpawned();
            }
            
            return item;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one with desired global position an rotation
        /// </summary>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn(Vector3 position, Quaternion rotation)
        {
            var obj = Spawn();
            if (!obj) return default;
            var objT = GetGameObject(obj).transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool inside a parent transform with SetParent(t, worldPositionStays);
        /// </summary>
        /// <param name="t">parent</param>
        /// <param name="worldPositionStays"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn(Transform t, bool worldPositionStays = false)
        {
            var obj = Spawn();
            if (!obj) return default;
            GetGameObject(obj).transform.SetParent(t, worldPositionStays);
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool inside a parent transform with desired global position an rotation
        /// </summary>
        /// <param name="t"></param>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn(Transform t, Vector3 position, Quaternion rotation)
        {
            var obj = Spawn(t);
            if (!obj) return default;
            var objT = GetGameObject(obj).transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        
        public void Recycle(T item)
        {
            if (_inactiveItems.Contains(item) && !_activeItems.Contains(item))
            {
                Debug.LogError("Item is already recycled!");
                return;
            }

            if (!_activeItems.Contains(item))
            {
                throw new ArgumentException("Item seems inactivated! But not recycled!");
            }

            _activeItems.Remove(item);
            _inactiveItems.Push(item);
            
            SetGameObjectOfItemActive(item, false);
            
            OnRecycled(item);
            
            if (item is IRecycleCallbackReceiver recycleCallbackReceiver)
            {
                recycleCallbackReceiver.OnRecycle();
            }
        }

        private void SetGameObjectOfItemActive(T item, bool state)
        {
            var go = GetGameObject(item);

            go.SetActive(state);

            if (!state)
            {
                go.transform.SetParent(Parent, false);
            }
        }

        private GameObject GetGameObject(T item)
        {            
            switch (item)
            {
                case GameObject go:
                    return go;
                case Component component:
                    return component.gameObject;
                default:
                    throw new ArgumentException("Item could not be received because it is neither GameObject nor Component!");
            }
        }
        
        protected virtual void OnCreated(T item){} // Optional
        protected virtual void OnSpawned(T item){} // Optional
        protected virtual void OnRecycled(T item){} // Optional
    }
}