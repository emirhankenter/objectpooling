using System;
using System.Collections.Generic;
using Mek.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mek.ObjectPooling
{
    public delegate void PoolInitialized(PoolBase poolBase);
    public abstract class PoolBase
    {
        public static PoolInitialized PoolInitialized;
        public Object Object { get; protected set; }
        public string Id { get; protected set; } = "";
    }
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
            
            OnCreated(item);
            
            if (item is ICreationCallbackReceiver creationCallbackReceiver)
            {
                creationCallbackReceiver.OnCreated();
            }
            return item;
        }

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
    public class ObjectPooling : MonoBehaviour
    {
        private Transform _poolParent;
        private readonly Dictionary<int, List<Object>> _pool = new Dictionary<int, List<Object>>();
        private readonly Dictionary<int, PoolBase> _poolDictionary = new Dictionary<int, PoolBase>();


        [RuntimeInitializeOnLoadMethod]
        public static void OnRuntimeInitialized()
        {
            _instance = new GameObject("ObjectPool").AddComponent<ObjectPooling>();
            DontDestroyOnLoad(_instance);
            _instance.Init();
        }

        private void OnDestroy()
        {
            Dispose();
        }
        
        private void Init()
        {
            PoolBase.PoolInitialized += OnPoolInitialized;
            _poolParent = transform;
        }

        private void Dispose()
        {
            PoolBase.PoolInitialized -= OnPoolInitialized;
        }

        private void OnPoolInitialized(PoolBase poolBase)
        {
            var obj = poolBase.Object;
            var hashCode = obj.GetHashCode();
            if (_poolDictionary.ContainsKey(hashCode))
            {
                throw new ArgumentException("Tried to create a pool from outside, even tough there is already a pool created before! Aborting from creating!");
            }

            _poolDictionary[hashCode] = poolBase;
        }

        public Pool<T> CreatePool<T>(T prefab) where T : Object
        {
            var hashCode = prefab.GetHashCode();
            Pool<T> pool = null;
            if (_poolDictionary.TryGetValue(hashCode, out var poolBase))
            {
                throw new ArgumentException("Trying to create a pool even tough there is already a pool created before!");
            }
            
            pool = new Pool<T>(prefab, false);
            _poolDictionary[prefab.GetHashCode()] = pool;
            
            return pool;
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

        #region Component
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one
        /// </summary>
        /// <param name="prefab"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab) where T : Component
        {
            var hashCode = prefab.GetHashCode();
            var obj = GetObj<T>(hashCode);
            if (obj == null)
            {
                obj = Instantiate(prefab);
                if (!_pool.ContainsKey(hashCode))
                {
                    _pool.Add(hashCode, new List<Object>{obj});
                }
                else
                {
                    _pool[prefab.GetHashCode()].Add(obj);
                }
            }
            else
            {
                obj.gameObject.SetActive(true);
                obj.gameObject.transform.SetParent(null, true);
            }

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
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            var objT = obj.transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one inside a parent transform with SetParent(t, worldPositionStays);
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="t">parent</param>
        /// <param name="worldPositionStays"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>(T prefab, Transform t, bool worldPositionStays = false) where T : Component
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            obj.transform.SetParent(t, worldPositionStays);
            return obj;
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
        public T Spawn<T>(T prefab, Transform t, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Spawn(prefab, t);
            if (!obj) return default;
            var objT = obj.transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        
        #endregion

        #region GameObject
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefab)
        {
            var hashCode = prefab.GetHashCode();
            var obj = GetObj(hashCode);
            if (obj == null)
            {
                obj = Instantiate(prefab);
                if (!_pool.ContainsKey(hashCode))
                {
                    _pool.Add(hashCode, new List<Object>{obj.gameObject});
                }
                else
                {
                    _pool[prefab.GetHashCode()].Add(obj.gameObject);
                }
            }
            else
            {
                obj.gameObject.SetActive(true);
                obj.transform.SetParent(null, true);
            }
        
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one with desired global position an rotation
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            var objT = obj.transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one inside a parent transform with SetParent(t, worldPositionStays);
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="t">parent</param>
        /// <param name="worldPositionStays"></param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefab, Transform t, bool worldPositionStays = false)
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            obj.transform.SetParent(t, worldPositionStays);
            return obj;
        }
        
        /// <summary>
        /// Spawns prefab from the pool if any exists, if not instantiates new one inside a parent transform with SetParent(t, worldPositionStays); with desired global position an rotation
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="t">parent</param>
        /// <param name="position">global position</param>
        /// <param name="rotation">global rotation</param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefab, Transform t, Vector3 position, Quaternion rotation)
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            var objT = obj.transform;
            objT.SetParent(t, false);
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }

        #endregion

        #region Utils

        private T GetObj<T>(int hashCode) where T : Component
        {
            if (_pool.ContainsKey(hashCode))
            {
                if (!_pool.TryGetValue(hashCode, out var objects)) return null;

                var shouldValidate = false;

                for (int i = 0; i < objects.Count; i++)
                {
                    var obj = objects[i];
                    if (!obj && !shouldValidate)
                    {
                        shouldValidate = true;
                        continue;
                    }

                    var component = obj as T;

                    if (!component)
                    {
                        if (!shouldValidate)
                        {
                            shouldValidate = true;
                        }
                        continue;
                    }
                    if (component.gameObject.activeInHierarchy || !component.transform.IsChildOf(_poolParent)) continue;

                    if (shouldValidate)
                    {
                        Validate(hashCode);
                    }

                    return component;

                }

                if (shouldValidate)
                {
                    Validate(hashCode);
                }


                return null;
            }

            return null;
        }

        private GameObject GetObj(int hashCode)
        {
            if (_pool.ContainsKey(hashCode))
            {
                if (!_pool.TryGetValue(hashCode, out var objects)) return null;

                var shouldValidate = false;

                for (int i = 0; i < objects.Count; i++)
                {
                    var obj = objects[i];
                    if (!obj && !shouldValidate)
                    {
                        shouldValidate = true;
                        continue;
                    }

                    var go = obj as GameObject;

                    if (!go)
                    {
                        if (!go && !shouldValidate)
                        {
                            shouldValidate = true;
                        }
                        continue;
                    }

                    if (go.activeInHierarchy || !go.transform.IsChildOf(_poolParent)) continue;
                    
                    if (shouldValidate)
                    {
                        Validate(hashCode);
                    }
                    return go;
                }

                if (shouldValidate)
                {
                    Validate(hashCode);
                }

                return null;
            }

            return null;
        }
        
        /// <summary>
        /// Recycles the object to the pool to use it later
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        public void Recycle<T>(T obj) where T : Component
        {
            if (!obj) return;
            obj.transform.SetParent(_poolParent, true);
            obj.gameObject.SetActive(false);

            obj.TryGetComponent(out IRecycleCallbackReceiver recyclable);
            recyclable?.OnRecycle();
        }
        
        /// <summary>
        /// Recycles the object to the pool to use it later
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(GameObject obj)
        {
            if (!obj) return;
            obj.transform.SetParent(_poolParent, true);
            obj.gameObject.SetActive(false);

            obj.TryGetComponent(out IRecycleCallbackReceiver recyclable);
            recyclable?.OnRecycle();
        }

        private void Validate(int key)
        {
            if (!_pool.TryGetValue(key, out List<Object> objects)) return;
            var newObjects = new List<Object>();
            var count = objects.Count;
                
            for (int i = 0; i < count; i++)
            {
                var obj = objects[i];
                if (obj) newObjects.Add(obj);
            }

            _pool[key] = newObjects;
        }

        public void ValidateAll()
        {
            var keys = new List<int>(_pool.Keys);
            foreach (var key in keys)
            {
                Validate(key);
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
