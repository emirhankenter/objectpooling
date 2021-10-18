using System.Collections.Generic;
using System.Linq;
using Mek.Interfaces;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public class ObjectPooling : MonoBehaviour
    {
        private Transform _poolParent;
        private Dictionary<int, List<Object>> _pool;

        private void Init()
        {
            _poolParent = transform;
            _pool = new Dictionary<int, List<Object>>();
        }

        #region Component

        public T Spawn<T>(T prefab) where T : Component
        {
            var hashCode = prefab.GetHashCode();
            var result = GetObj<T>(hashCode);
            T obj;
            if (result == null)
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
                obj = result as T;
                obj.gameObject.SetActive(true);
                obj.gameObject.transform.SetParent(null, true);
            }

            return obj;
        }
        
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            var objT = obj.transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        

        public T Spawn<T>(T prefab, Transform t, bool worldPositionStays = false) where T : Component
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            obj.transform.SetParent(t, worldPositionStays);
            return obj;
        }
        
        public T Spawn<T>(T prefab, Transform t, Vector3 position, Quaternion rotation) where T : Component
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

        #region GameObject
        
        public GameObject Spawn(GameObject prefab)
        {
            var hashCode = prefab.GetHashCode();
            var result = GetObj(hashCode);
            GameObject obj;
            if (result == null)
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
                obj = result as GameObject;
                obj.gameObject.SetActive(true);
                obj.transform.SetParent(null, true);
            }
        
            return obj;
        }
        
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            var objT = obj.transform;
            objT.position = position;
            objT.rotation = rotation;
            return obj;
        }
        

        public GameObject Spawn(GameObject prefab, Transform t, bool worldPositionStays = false)
        {
            var obj = Spawn(prefab);
            if (!obj) return default;
            obj.transform.SetParent(t, worldPositionStays);
            return obj;
        }
        
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

        private Object GetObj<T>(int hashCode) where T : Component
        {
            if (_pool.ContainsKey(hashCode))
            {
                var result = _pool[hashCode]
                    .Where(item =>
                    {
                        var component = item as T;
                        return !component.gameObject.activeSelf && component.transform.IsChildOf(_poolParent);
                    })
                    .ToList();

                return result.Count > 0 ? result[0] : null;
            }

            return null;
        }
        

        private Object GetObj(int hashCode)
        {
            if (_pool.ContainsKey(hashCode))
            {
                var result = _pool[hashCode]
                    .Where(item =>
                    {
                        var go = item as GameObject;
                        return !go.gameObject.activeSelf && go.transform.IsChildOf(_poolParent);
                    })
                    .ToList();

                return result.Count > 0 ? result[0] : null;
            }

            return null;
        }
        
        public void Recycle<T>(T obj) where T : Component
        {
            obj.transform.SetParent(_poolParent, true);
            obj.gameObject.SetActive(false);

            obj.TryGetComponent(out IRecycleCallbackReceiver recyclable);
            recyclable?.OnRecycle();
        }
        
        public void Recycle(GameObject obj)
        {
            obj.transform.SetParent(_poolParent, true);
            obj.gameObject.SetActive(false);

            obj.TryGetComponent(out IRecycleCallbackReceiver recyclable);
            recyclable?.OnRecycle();
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
