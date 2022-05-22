using UnityEngine;

namespace Mek.ObjectPooling
{
    public delegate void PoolInitialized(PoolBase poolBase);
    public abstract class PoolBase
    {
        public Object Object { get; protected set; }
        public string Id { get; protected set; } = "";
    }
}