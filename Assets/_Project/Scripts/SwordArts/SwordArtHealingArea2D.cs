using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class SwordArtHealingArea2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Health targetHealth;
        [SerializeField] private Vector2 center;
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private float duration = 4f;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private float healAmountPerTick = 6f;
        [SerializeField] private string sourceDisplayName = "长生诀";
        [SerializeField] private Color gizmoColor = Color.green;

        private float elapsed;
        private float tickTimer;

        public void Initialize(
            Transform target,
            Health targetHealth,
            Vector2 center,
            float radius,
            float duration,
            float tickInterval,
            float healAmountPerTick,
            string sourceDisplayName,
            Color gizmoColor)
        {
            this.target = target;
            this.targetHealth = targetHealth;
            this.center = center;
            this.radius = Mathf.Max(0.01f, radius);
            this.duration = Mathf.Max(0.01f, duration);
            this.tickInterval = Mathf.Max(0.01f, tickInterval);
            this.healAmountPerTick = Mathf.Max(0f, healAmountPerTick);
            this.sourceDisplayName = string.IsNullOrWhiteSpace(sourceDisplayName)
                ? "长生诀"
                : sourceDisplayName;
            this.gizmoColor = gizmoColor;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                TryHeal();
            }

            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }

        private void TryHeal()
        {
            if (target == null || targetHealth == null || targetHealth.IsDead)
            {
                return;
            }

            float sqrDistance = ((Vector2)target.position - center).sqrMagnitude;

            if (sqrDistance > radius * radius)
            {
                return;
            }

            targetHealth.Heal(healAmountPerTick);
            Debug.Log(
                $"{sourceDisplayName} healed {target.name} for {healAmountPerTick}",
                this);
        }

        private void OnDrawGizmos()
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(center, radius);
            Gizmos.color = previousColor;
        }
    }
}
