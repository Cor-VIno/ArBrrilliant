using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Enemies
{
    public sealed class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private EnemyBrain2D brain;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Collider2D[] collidersToDisable;
        [SerializeField] private Behaviour[] behavioursToDisable;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 0.8f;
        [SerializeField] private bool logDeath = true;

        private bool isDead;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (health != null)
            {
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void ResolveReferences()
        {
            if (health == null)
            {
                TryGetComponent(out health);
            }

            if (brain == null)
            {
                TryGetComponent(out brain);
            }

            if (body == null)
            {
                TryGetComponent(out body);
            }
        }

        private void HandleDied()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            if (logDeath)
            {
                Debug.Log($"{name} died.", this);
            }

            if (brain != null)
            {
                brain.enabled = false;
            }

            DisableBehaviours();
            DisableColliders();
            StopBody();

            if (destroyOnDeath)
            {
                Destroy(gameObject, Mathf.Max(0f, destroyDelay));
            }
        }

        private void DisableBehaviours()
        {
            if (behavioursToDisable == null)
            {
                return;
            }

            for (int i = 0; i < behavioursToDisable.Length; i++)
            {
                Behaviour behaviour = behavioursToDisable[i];

                if (behaviour != null && behaviour != this)
                {
                    behaviour.enabled = false;
                }
            }
        }

        private void DisableColliders()
        {
            if (collidersToDisable == null)
            {
                return;
            }

            for (int i = 0; i < collidersToDisable.Length; i++)
            {
                Collider2D targetCollider = collidersToDisable[i];

                if (targetCollider != null)
                {
                    targetCollider.enabled = false;
                }
            }
        }

        private void StopBody()
        {
            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }
    }
}
