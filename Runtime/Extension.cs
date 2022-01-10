using UnityEngine;

namespace Mek.ObjectPooling
{
    public static class Extension
    {
        public static T Spawn<T>(this T prefab) where T : Object
        {
            return ObjectPooling.Instance.Spawn(prefab);
        }
        
        public static T Spawn<T>(this T prefab, Transform t, bool worldPositionStays = false) where T : Object
        {
            return ObjectPooling.Instance.Spawn(prefab, t, worldPositionStays);
        }
        
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Object
        {
            return ObjectPooling.Instance.Spawn(prefab, position, rotation);
        }
        
        public static T Spawn<T>(this T prefab, Transform t, Vector3 position, Quaternion rotation) where T : Object
        {
            return ObjectPooling.Instance.Spawn(prefab, t, position, rotation);
        }

        public static void Recycle<T>(this T item) where T : Object
        {
            ObjectPooling.Instance.Recycle(item);
        }
    }
}