using System.Collections.Generic;
using UnityEngine;

namespace Mek.ObjectPooling
{
    public class ParticlePooling : MonoBehaviour
    {
        private List<ParticleSystem> _particles = new List<ParticleSystem>();

        public ParticleSystem Spawn(ParticleSystem particle)
        {
            var p = ObjectPooling.Instance.Spawn(particle);

            if (!_particles.Contains(p))
            {
                _particles.Add(p);
            }

            return p;
        }

        public ParticleSystem Spawn(ParticleSystem particle, Transform t)
        {
            var pose = new Pose(particle.transform.localPosition, particle.transform.rotation);
            var selectedParticle = Spawn(particle);
            selectedParticle.transform.SetParent(t, false);
            selectedParticle.transform.localPosition = pose.position;
            selectedParticle.transform.localRotation = pose.rotation;
            return selectedParticle;
        }

        public ParticleSystem Spawn(ParticleSystem particle, Vector3 position, Quaternion rotation, bool keepPrefabsInitialRotation = false)
        {
            var rotationDifference = particle.transform.rotation * Quaternion.Inverse(rotation);
            var selectedParticle = Spawn(particle);
            selectedParticle.transform.position = position;
            selectedParticle.transform.rotation = keepPrefabsInitialRotation ? rotationDifference : rotation;
            return selectedParticle;
        }

        public void Recycle(ParticleSystem particle)
        {
            particle.Stop();
            particle.Clear();
            ObjectPooling.Instance.Recycle(particle);
        }

        private static ParticlePooling _instance;
        public static ParticlePooling Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ParticlePooling>();

                    if (_instance == null)
                    {
                        if (ObjectPooling.Instance)
                        {
                            var instance = ObjectPooling.Instance.gameObject.AddComponent<ParticlePooling>();
                            _instance = instance;
                        }
                    }
                }
                return _instance;
            }
        }
    }
}