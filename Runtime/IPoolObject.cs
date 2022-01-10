using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public interface IPoolObject
    {
        int GetPrefabHashCode();
    }
}
