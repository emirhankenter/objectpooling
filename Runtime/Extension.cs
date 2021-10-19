using UnityEngine;

namespace Mek.ObjectPooling
{
    public static class Extension
    {
        public static T Spawn<T>(this T prefab) where T : Component
        {
            return ObjectPooling.Instance.Spawn(prefab);
        }
        
        public static T Spawn<T>(this T prefab, Transform t, bool worldPositionStays = false) where T : Component
        {
            return ObjectPooling.Instance.Spawn(prefab, t, worldPositionStays);
        }
        
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPooling.Instance.Spawn(prefab, position, rotation);
        }
        
        public static T Spawn<T>(this T prefab, Transform t, Vector3 position, Quaternion rotation) where T : Component
        {
            return ObjectPooling.Instance.Spawn(prefab, t, position, rotation);
        }

        public static GameObject Spawn(this GameObject obj)
        {
            return ObjectPooling.Instance.Spawn(obj);
        }
        
        public static GameObject Spawn(this GameObject prefab, Transform t, bool worldPositionStays = false)
        {
            return ObjectPooling.Instance.Spawn(prefab, t, worldPositionStays);
        }
        
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return ObjectPooling.Instance.Spawn(prefab, position, rotation);
        }
        
        public static GameObject Spawn(this GameObject prefab, Transform t, Vector3 position, Quaternion rotation)
        {
            return ObjectPooling.Instance.Spawn(prefab, t, position, rotation);
        }

        public static void Recycle<T>(this T component) where T : Component
        {
            ObjectPooling.Instance.Recycle<T>(component);
        }
        
        public static void Recycle(this GameObject obj)
        {
            ObjectPooling.Instance.Recycle(obj);
        }
    }
}