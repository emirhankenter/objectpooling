using System;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public class PooledParticleObject : MonoBehaviour
    {
        private ParticleSystem _particle;
        private PoolObject _poolObject;

        private Pool<ParticleSystem> _pool;

        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            _poolObject = GetComponent<PoolObject>();
            if (_poolObject == null)
            {
                Debug.LogError("PooledParticle object could not get component PoolObject!");
                return;
            }
            var prefabHashCode = _poolObject.GetPrefabHashCode();
            _pool = ObjectPooling.Instance.GetPool<ParticleSystem>(prefabHashCode);
        }

        private void OnParticleSystemStopped()
        {
            if (_pool == null)
            {
                _particle.Recycle();
            }
            else
            {
                _pool.Recycle(_particle);
            }
        }
    }
}
