using System.Collections.Generic;
using System.Linq;
using Mek.Interfaces;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public class ObjectPooling : MonoBehaviour
    {
        private Transform _poolParent;
        private Dictionary<int, List<Object>> _pool = new Dictionary<int, List<Object>>();

        private void Init()
        {
            _poolParent = transform;
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
