using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class ProjectileImpact2D : MonoBehaviour
    {
        private const int MaxImpactHits = 4;

        [SerializeField] private LayerMask impactLayerMask;
        [SerializeField] private float checkRadius = 0.15f;
        [SerializeField] private bool destroyOnImpact = true;

        private readonly Collider2D[] impactHits = new Collider2D[MaxImpactHits];
        private ContactFilter2D impactFilter;

        public void Initialize(LayerMask impactLayerMask, float checkRadius, bool destroyOnImpact)
        {
            this.impactLayerMask = impactLayerMask;
            this.checkRadius = Mathf.Max(0.01f, checkRadius);
            this.destroyOnImpact = destroyOnImpact;
            ConfigureFilter();
        }

        private void Awake()
        {
            ConfigureFilter();
        }

        private void Update()
        {
            CheckImpact();
        }

        private void ConfigureFilter()
        {
            impactFilter.useLayerMask = true;
            impactFilter.useTriggers = false;
            impactFilter.SetLayerMask(impactLayerMask);
        }

        private void CheckImpact()
        {
            if (!destroyOnImpact)
            {
                return;
            }

            int hitCount = Physics2D.OverlapCircle(
                transform.position,
                checkRadius,
                impactFilter,
                impactHits);

            if (hitCount > 0)
            {
                Destroy(gameObject);
            }

            for (int i = 0; i < hitCount; i++)
            {
                impactHits[i] = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
    }
}
