using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class Hurtbox2D : MonoBehaviour
    {
        [SerializeField] private Damageable damageable;
        [SerializeField] private Collider2D hurtboxCollider;

        public Damageable Damageable => damageable;
        public Collider2D HurtboxCollider => hurtboxCollider;

        private void Awake()
        {
            ResolveReferences();
            WarnIfColliderIsNotTrigger();
        }

        private void OnValidate()
        {
            ResolveReferences();
            WarnIfColliderIsNotTrigger();
        }

        private void ResolveReferences()
        {
            if (hurtboxCollider == null)
            {
                TryGetComponent(out hurtboxCollider);
            }

            if (damageable == null)
            {
                damageable = GetComponent<Damageable>();
            }

            if (damageable == null)
            {
                damageable = GetComponentInParent<Damageable>();
            }
        }

        private void WarnIfColliderIsNotTrigger()
        {
            if (hurtboxCollider != null && !hurtboxCollider.isTrigger)
            {
                Debug.LogWarning(
                    $"{nameof(Hurtbox2D)} on {name} uses a Collider2D that is not marked as Trigger.",
                    this);
            }
        }
    }
}
