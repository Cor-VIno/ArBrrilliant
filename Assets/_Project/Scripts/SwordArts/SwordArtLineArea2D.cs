using System.Collections.Generic;
using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public sealed class SwordArtLineArea2D : MonoBehaviour
    {
        private const int MaxOverlapCount = 32;

        [SerializeField] private GameObject owner;
        [SerializeField] private TeamId ownerTeam = TeamId.Player;
        [SerializeField] private Vector2 startPoint;
        [SerializeField] private Vector2 endPoint;
        [SerializeField] private float lineWidth = 0.8f;
        [SerializeField] private float duration = 3f;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private float tickDamage = 6f;
        [SerializeField] private float finalDamage = 24f;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private string sourceDisplayName = "沧浪诀";
        [SerializeField] private bool finalCanKnockUp = true;
        [SerializeField] private Vector2 finalKnockUpVelocity = new Vector2(3f, 7f);
        [SerializeField] private float finalAirborneDuration = 1.2f;
        [SerializeField] private Color gizmoColor = Color.cyan;

        private readonly Collider2D[] overlapResults =
            new Collider2D[MaxOverlapCount];
        private readonly HashSet<Damageable> scannedTargets =
            new HashSet<Damageable>();
        private ContactFilter2D contactFilter;
        private float elapsed;
        private float tickTimer;

        public void Initialize(
            GameObject owner,
            TeamId ownerTeam,
            Vector2 startPoint,
            Vector2 endPoint,
            float lineWidth,
            float duration,
            float tickInterval,
            float tickDamage,
            float finalDamage,
            LayerMask targetLayerMask,
            string sourceDisplayName,
            bool finalCanKnockUp,
            Vector2 finalKnockUpVelocity,
            float finalAirborneDuration,
            Color gizmoColor)
        {
            this.owner = owner;
            this.ownerTeam = ownerTeam;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.lineWidth = Mathf.Max(0.01f, lineWidth);
            this.duration = Mathf.Max(0.01f, duration);
            this.tickInterval = Mathf.Max(0.01f, tickInterval);
            this.tickDamage = Mathf.Max(0f, tickDamage);
            this.finalDamage = Mathf.Max(0f, finalDamage);
            this.targetLayerMask = targetLayerMask;
            this.sourceDisplayName = string.IsNullOrWhiteSpace(sourceDisplayName)
                ? "沧浪诀"
                : sourceDisplayName;
            this.finalCanKnockUp = finalCanKnockUp;
            this.finalKnockUpVelocity = finalKnockUpVelocity;
            this.finalAirborneDuration = finalAirborneDuration;
            this.gizmoColor = gizmoColor;

            ConfigureFilter();
        }

        private void Awake()
        {
            ConfigureFilter();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyTickDamage();
            }

            if (elapsed >= duration)
            {
                ApplyFinalDamage();
                Destroy(gameObject);
            }
        }

        private void ConfigureFilter()
        {
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = true;
            contactFilter.SetLayerMask(targetLayerMask);
        }

        private void ApplyTickDamage()
        {
            ApplyDamageInArea(
                damage: tickDamage,
                canKnockUp: false,
                knockUpVelocity: Vector2.zero,
                airborneDuration: 0f);
        }

        private void ApplyFinalDamage()
        {
            ApplyDamageInArea(
                damage: finalDamage,
                canKnockUp: finalCanKnockUp,
                knockUpVelocity: finalKnockUpVelocity,
                airborneDuration: finalAirborneDuration);
        }

        private void ApplyDamageInArea(
            float damage,
            bool canKnockUp,
            Vector2 knockUpVelocity,
            float airborneDuration)
        {
            if (damage <= 0f)
            {
                return;
            }

            int hitCount = ScanArea();
            scannedTargets.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = overlapResults[i];
                overlapResults[i] = null;

                if (hit == null)
                {
                    continue;
                }

                Hurtbox2D hurtbox = hit.GetComponent<Hurtbox2D>();

                if (hurtbox == null)
                {
                    hurtbox = hit.GetComponentInParent<Hurtbox2D>();
                }

                ApplyDamageToHurtbox(
                    hurtbox,
                    damage,
                    canKnockUp,
                    knockUpVelocity,
                    airborneDuration);
            }

            scannedTargets.Clear();
        }

        private int ScanArea()
        {
            Vector2 center = (startPoint + endPoint) * 0.5f;
            Vector2 direction = endPoint - startPoint;
            float length = Mathf.Max(0.01f, direction.magnitude);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector2 size = new Vector2(length, lineWidth);

            return Physics2D.OverlapBox(
                center,
                size,
                angle,
                contactFilter,
                overlapResults);
        }

        private void ApplyDamageToHurtbox(
            Hurtbox2D hurtbox,
            float damage,
            bool canKnockUp,
            Vector2 knockUpVelocity,
            float airborneDuration)
        {
            if (hurtbox == null || hurtbox.Damageable == null)
            {
                return;
            }

            Damageable target = hurtbox.Damageable;

            if (target.Team == ownerTeam || scannedTargets.Contains(target))
            {
                return;
            }

            scannedTargets.Add(target);

            Vector2 targetPosition = hurtbox.transform.position;
            Vector2 attackerPosition = owner != null
                ? (Vector2)owner.transform.position
                : startPoint;
            Vector2 knockbackDirection = targetPosition - attackerPosition;

            if (knockbackDirection.sqrMagnitude > 0.0001f)
            {
                knockbackDirection.Normalize();
            }

            DamageInfo damageInfo = new DamageInfo(
                attacker: owner,
                target: target,
                damage: damage,
                hitPoint: targetPosition,
                knockbackDirection: knockbackDirection,
                knockbackForce: 0f,
                canCritical: false,
                isCritical: false,
                sourceSkill: null,
                sourceDisplayName: sourceDisplayName,
                canKnockUp: canKnockUp,
                knockUpVelocity: knockUpVelocity,
                airborneDuration: airborneDuration);

            target.ApplyDamage(damageInfo);
        }

        private void OnDrawGizmos()
        {
            Vector2 direction = endPoint - startPoint;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector2 center = (startPoint + endPoint) * 0.5f;
            float length = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.color = gizmoColor;
            Gizmos.matrix =
                Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(length, lineWidth, 0f));

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
