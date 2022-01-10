using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public class PoolObject : MonoBehaviour, IPoolObject
    {
        private int _prefabHashCode;

        public void Init(int prefabHashcode)
        {
            _prefabHashCode = prefabHashcode;
        }

        public int GetPrefabHashCode() => _prefabHashCode;
    }
}
