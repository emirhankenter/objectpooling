using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mek.ObjectPooling
{
    public class ObjectPooling : MonoBehaviour
    {
        private Transform _poolParent;
        private readonly Dictionary<int, PoolBase> _poolDictionary = new Dictionary<int, PoolBase>();

        private void Init()
        {
            _poolParent = transform;
        }

        public Pool<T> CreatePool<T>(T prefab) where T : Object
        {
            var hashCode = prefab.GetHashCode();
            Pool<T> pool = null;
            if (_poolDictionary.TryGetValue(hashCode, out var poolBase))
            {
                throw new ArgumentException("Trying to create a pool even tough there is already a pool created before!");
            }
            
            pool = new Pool<T>(prefab)
                .AsParent(transform);
            _poolDictionary[prefab.GetHashCode()] = pool;
            
            return pool;
        }

        public T InitializePool<T>(PoolBase poolBase) where T : PoolBase
        {
            var hashCode = poolBase.Object.GetHashCode();
            if (_poolDictionary.ContainsKey(hashCode))
            {
                throw new ArgumentException($"Pool with hashcode {hashCode}, initialized before!");
            }

            _poolDictionary[hashCode] = poolBase;
            return poolBase as T;
        }

        public Pool<T> GetPoolWithId<T>(string id) where T : Object
        {
            foreach (var pair in _poolDictionary)
            {
                if (pair.Value.Id != id) continue;
                if (pair.Value is Pool<T> pool)
                {
                    return pool;
                }

                throw new InvalidCastException(
                    $"Type of pool with ID: {pair.Value.Id} is not equal with id ({id})");
            }

            throw new ArgumentOutOfRangeException($"Could not found pool with desired id: ({id})");
        }

        public Pool<T> GetPool<T>(T prefab) where T : Object
        {
            Pool<T> pool = null;
            
            if (_poolDictionary.TryGetValue(prefab.GetHashCode(), out PoolBase poolbase))
            {
                pool = poolbase as Pool<T>;
                return pool;
            }

            return CreatePool(prefab);
        }

        public Pool<T> GetPool<T>(int hashCode) where T : Object
        {
            Pool<T> pool = null;
            
            if (_poolDictionary.TryGetValue(hashCode, out PoolBase poolbase))
            {
                pool = poolbase as Pool<T>;
            }

            return pool;
        }

        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one
        /// </summary>
        /// <param name="prefab"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab) where T : Object
        {
            Pool<T> pool = GetPool(prefab);

            if (pool == null)
            {
                throw new ArgumentException($"Could not create pool of type {typeof(T)}");
            }
            
            var obj = pool.Spawn();
            
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one with desired global position an rotation
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object
        {
            Pool<T> pool = GetPool(prefab);

            if (pool == null)
            {
                throw new ArgumentException($"Could not create pool of type {typeof(T)}");
            }
            
            return pool.Spawn(position, rotation);
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one inside a parent transform with SetParent(t, worldPositionStays);
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="t">parent</param>
        /// <param name="worldPositionStays"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab, Transform t, bool worldPositionStays = false) where T : Object
        {
            Pool<T> pool = GetPool(prefab);

            if (pool == null)
            {
                throw new ArgumentException($"Could not create pool of type {typeof(T)}");
            }
            
            return pool.Spawn(t, worldPositionStays);
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one inside a parent transform with desired global position an rotation
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="t"></param>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab, Transform t, Vector3 position, Quaternion rotation) where T : Object
        {
            Pool<T> pool = GetPool(prefab);

            if (pool == null)
            {
                throw new ArgumentException($"Could not create pool of type {typeof(T)}");
            }
            
            return pool.Spawn(t, position, rotation);
        }

        /// <summary>
        /// Recycles the object to the pool to use it later
        /// ***Best Practice: Calling Recycle() directly from the related pool is recommended in concern of performance!***
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public void Recycle<T>(T item) where T : Object
        {
            if (!item) return;
            
            var go = GetGameObject(item);

            if (go.TryGetComponent<IPoolObject>(out var poolObject))
            {
                var prefabHashCode = poolObject.GetPrefabHashCode();

                if (_poolDictionary.TryGetValue(prefabHashCode, out PoolBase poolBase))
                {
                    var pool = poolBase as Pool<T>;

                    if (pool == null)
                    {
                        throw new ArgumentNullException(
                            $"Trying to return item to the pool while there is no proper pool!");
                    }
                    pool.Recycle(item);
                }
                else
                {
                    throw new ArgumentException($"Could not return item to the pool! Pool Type: {typeof(T)}");
                }
            }
            else
            {
                throw new ArgumentException($"Could not return item to the pool! Pool Type: {typeof(T)}");
            }
        }

        #region Utils
        
        private GameObject GetGameObject<T>(T item)
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
        
        #endregion
        
        #region Singleton

        private static ObjectPooling _instance;

        public static ObjectPooling Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                _instance = new GameObject("ObjectPool").AddComponent<ObjectPooling>();
                DontDestroyOnLoad(_instance);
                _instance.Init();

                return _instance;
            }
        }

        #endregion
    }
}
