using System;
using System.Collections.Generic;
using DG.Tweening;
using PinkBlob.Gameplay.Ability;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PinkBlob.Gameplay.Suck
{
    public class SuckableBlock : MonoBehaviour, ISuckable
    {
        public event Action<AbilityType> CompleteSucking;

        private const float SuckTime = 0.2f;

        private float suckTimer = 0;
        
        [SerializeField]
        private Transform visual = default;
        
        [Min(0)]
        [SerializeField]
        private float suckHealth = 3f;

        private float currentSuckHealth;

        [Min(0)]
        [SerializeField]
        private float inhaleTime = 0.2f;

        [SerializeField]
        private Ease inhaleEase = Ease.InQuint;

        private bool isInhaled = false;

        [SerializeField]
        private AbilityType abilityType = AbilityType.Normal;

        [Title("Shake")]

        [Min(0)]
        [SerializeField]
        private float intensity = 0.1f;

        [Min(0)]
        [SerializeField]
        private float speed = 10f;

        private Vector3 shakeSeed;

        [Title("Colliders")]

        [SerializeField]
        private List<Collider> colliders = new List<Collider>();

        private bool isSucking;

        private void Awake()
        {
            currentSuckHealth = suckHealth;
            shakeSeed = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        }

        public void EnterSucking()
        {
            isSucking = true;
            currentSuckHealth = suckHealth;
            visual.localPosition = Vector3.zero;
        }

        public void UpdateSucking(Vector3 source)
        {
            if (isInhaled)
            {
                return;
            }

            suckTimer -= Time.deltaTime;

            float randX = Mathf.Lerp(-1, 1, Mathf.PerlinNoise((Time.time * speed) + shakeSeed.x, (Time.deltaTime * speed) + shakeSeed.x));
            float randY = Mathf.Lerp(-1, 1, Mathf.PerlinNoise((Time.time * speed) + shakeSeed.y, (Time.deltaTime * speed) + shakeSeed.y));
            float randZ = Mathf.Lerp(-1, 1, Mathf.PerlinNoise((Time.time * speed) + shakeSeed.z, (Time.deltaTime * speed) + shakeSeed.z));

            var shake = new Vector3(randX * intensity, randY * intensity, randZ * intensity);

            visual.localPosition = shake;

            if (suckTimer <= 0)
            {
                suckTimer = SuckTime;
                currentSuckHealth--;

                if (currentSuckHealth <= 0)
                {
                    CompleteSuck(source);
                }
            }
        }

        public void ExitSucking()
        {
            isSucking = false;
            currentSuckHealth = suckHealth;
            visual.localPosition = Vector3.zero;
        }

        private void CompleteSuck(Vector3 destination)
        {
            isInhaled = true;

            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            transform.DOMove(destination, inhaleTime)
                     .SetEase(inhaleEase)
                     .OnComplete(() =>
                                 {
                                     CompleteSucking?.Invoke(abilityType);
                                     Destroy(gameObject);
                                 });
            
            transform.DOScale(Vector3.one * 0.1f, inhaleTime).SetEase(inhaleEase);
        }

        public bool IsSucking()
        {
            return isSucking;
        }
    }
}
