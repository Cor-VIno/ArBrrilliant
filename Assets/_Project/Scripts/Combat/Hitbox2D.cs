using System.Collections.Generic;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Combat
{
    public sealed class Hitbox2D : MonoBehaviour
    {
        private const int MaxOverlapCount = 32;

        [SerializeField] private GameObject owner;
        [SerializeField] private TeamId ownerTeam;
        [SerializeField] private float damage;
        [SerializeField] private Vector2 size = Vector2.one;
        [SerializeField] private Vector2 direction = Vector2.right;
        [SerializeField] private float lifetime = 0.08f;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private bool hitOncePerTarget = true;
        [SerializeField] private bool canCritical;
        [SerializeField] private SkillData sourceSkill;
        [SerializeField] private Color gizmoColor = new Color(1f, 0.25f, 0.1f, 0.35f);
        [SerializeField] private HitboxShape shape = HitboxShape.Box;
        [SerializeField] private float angleDegrees;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float innerRadius = 0f;
        [SerializeField] private float arcAngle = 90f;
        [SerializeField] private bool destroyOnFirstHit = false;
        [SerializeField] private string sourceDisplayName;
        [SerializeField] private bool canKnockUp;
        [SerializeField] private Vector2 knockUpVelocity;
        [SerializeField] private float airborneDuration;
        [SerializeField] private bool canApplyHitStun;
        [SerializeField] private float hitStunDuration;
        [SerializeField] private AttackInterruptType interruptType;
        [SerializeField] private bool canBePerfectDodged;

        private readonly Collider2D[] overlapResults = new Collider2D[MaxOverlapCount];
        private readonly HashSet<Damageable> hitTargets = new HashSet<Damageable>();
        private ContactFilter2D contactFilter;
        private float remainingLifetime;

        public GameObject Owner => owner;
        public TeamId OwnerTeam => ownerTeam;
        public string SourceDisplayName => sourceDisplayName;
        public bool CanBePerfectDodged => canBePerfectDodged;

        public void Initialize(
            GameObject owner,
            TeamId ownerTeam,
            float damage,
            Vector2 size,
            Vector2 direction,
            float lifetime,
            LayerMask targetLayerMask,
            bool hitOncePerTarget,
            bool canCritical,
            SkillData sourceSkill,
            Color gizmoColor,
            HitboxShape shape,
            float angleDegrees,
            float radius,
            float innerRadius,
            float arcAngle,
            bool destroyOnFirstHit,
            string sourceDisplayName = null,
            bool canKnockUp = false,
            Vector2 knockUpVelocity = default,
            float airborneDuration = 0f,
            bool canApplyHitStun = false,
            float hitStunDuration = 0f,
            AttackInterruptType interruptType = AttackInterruptType.None,
            bool canBePerfectDodged = false)
        {
            this.owner = owner;
            this.ownerTeam = ownerTeam;
            this.damage = damage;
            this.size = size;
            this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            this.lifetime = lifetime;
            this.targetLayerMask = targetLayerMask;
            this.hitOncePerTarget = hitOncePerTarget;
            this.canCritical = canCritical;
            this.sourceSkill = sourceSkill;
            this.gizmoColor = gizmoColor;
            this.shape = shape;
            this.angleDegrees = angleDegrees;
            this.radius = Mathf.Max(0.01f, radius);
            this.innerRadius = Mathf.Max(0f, innerRadius);
            this.arcAngle = Mathf.Clamp(arcAngle, 0f, 360f);
            this.destroyOnFirstHit = destroyOnFirstHit;
            this.sourceDisplayName = sourceDisplayName;
            this.canKnockUp = canKnockUp;
            this.knockUpVelocity = knockUpVelocity;
            this.airborneDuration = Mathf.Max(0f, airborneDuration);
            this.canApplyHitStun = canApplyHitStun;
            this.hitStunDuration = Mathf.Max(0f, hitStunDuration);
            this.interruptType = interruptType;
            this.canBePerfectDodged = canBePerfectDodged;

            ConfigureContactFilter();
            ConfigurePerfectDodgeCollider();
            remainingLifetime = Mathf.Max(0.01f, lifetime);
        }

        private void Awake()
        {
            ConfigureContactFilter();
            ConfigurePerfectDodgeCollider();
            remainingLifetime = Mathf.Max(0.01f, lifetime);
        }

        private void Update()
        {
            ScanForTargets();

            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void ConfigureContactFilter()
        {
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = true;
            contactFilter.SetLayerMask(targetLayerMask);
        }

        private void ConfigurePerfectDodgeCollider()
        {
            if (!canBePerfectDodged)
            {
                return;
            }

            switch (shape)
            {
                case HitboxShape.Circle:
                case HitboxShape.Arc:
                    CircleCollider2D circle = GetComponent<CircleCollider2D>();

                    if (circle == null)
                    {
                        circle = gameObject.AddComponent<CircleCollider2D>();
                    }

                    circle.isTrigger = true;
                    circle.radius = Mathf.Max(0.01f, radius);
                    circle.offset = Vector2.zero;
                    circle.enabled = true;
                    break;
                case HitboxShape.Capsule:
                    CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();

                    if (capsule == null)
                    {
                        capsule = gameObject.AddComponent<CapsuleCollider2D>();
                    }

                    capsule.isTrigger = true;
                    capsule.size = size;
                    capsule.direction = CapsuleDirection2D.Horizontal;
                    capsule.offset = Vector2.zero;
                    capsule.enabled = true;
                    break;
                case HitboxShape.Box:
                default:
                    BoxCollider2D box = GetComponent<BoxCollider2D>();

                    if (box == null)
                    {
                        box = gameObject.AddComponent<BoxCollider2D>();
                    }

                    box.isTrigger = true;
                    box.size = size;
                    box.offset = Vector2.zero;
                    box.enabled = true;
                    break;
            }
        }

        private void ScanForTargets()
        {
            int hitCount = ScanOverlaps();

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = overlapResults[i];

                if (hit == null)
                {
                    continue;
                }

                if (shape == HitboxShape.Arc && !IsInsideArc(hit))
                {
                    continue;
                }

                Hurtbox2D hurtbox = hit.GetComponent<Hurtbox2D>();

                if (hurtbox == null)
                {
                    continue;
                }

                if (TryHit(hurtbox))
                {
                    break;
                }
            }

            for (int i = 0; i < hitCount; i++)
            {
                overlapResults[i] = null;
            }
        }

        private int ScanOverlaps()
        {
            switch (shape)
            {
                case HitboxShape.Circle:
                    return Physics2D.OverlapCircle(
                        transform.position,
                        radius,
                        contactFilter,
                        overlapResults);
                case HitboxShape.Capsule:
                    return Physics2D.OverlapCapsule(
                        transform.position,
                        size,
                        CapsuleDirection2D.Horizontal,
                        angleDegrees,
                        contactFilter,
                        overlapResults);
                case HitboxShape.Arc:
                    // Arc uses circle overlap first, then filters candidates by sampled bounds points.
                    return Physics2D.OverlapCircle(
                        transform.position,
                        radius,
                        contactFilter,
                        overlapResults);
                case HitboxShape.Box:
                default:
                    return Physics2D.OverlapBox(
                        transform.position,
                        size,
                        angleDegrees,
                        contactFilter,
                        overlapResults);
            }
        }

        private bool IsInsideArc(Collider2D hit)
        {
            Bounds bounds = hit.bounds;
            Vector2 closestToCenter = hit.ClosestPoint(transform.position);

            if (IsPointInsideArc(closestToCenter))
            {
                return true;
            }

            Vector2 boundsCenter = bounds.center;
            Vector2 boundsMin = bounds.min;
            Vector2 boundsMax = bounds.max;

            return IsPointInsideArc(boundsCenter)
                || IsPointInsideArc(boundsMin)
                || IsPointInsideArc(boundsMax)
                || IsPointInsideArc(new Vector2(boundsMin.x, boundsMax.y))
                || IsPointInsideArc(new Vector2(boundsMax.x, boundsMin.y))
                || IsPointInsideArc(new Vector2(boundsCenter.x, boundsMin.y))
                || IsPointInsideArc(new Vector2(boundsCenter.x, boundsMax.y))
                || IsPointInsideArc(new Vector2(boundsMin.x, boundsCenter.y))
                || IsPointInsideArc(new Vector2(boundsMax.x, boundsCenter.y));
        }

        private bool IsPointInsideArc(Vector2 point)
        {
            Vector2 center = transform.position;
            Vector2 toPoint = point - center;
            float distance = toPoint.magnitude;

            if (distance < innerRadius || distance > radius)
            {
                return false;
            }

            if (toPoint.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            Vector2 forward = AngleToDirection(angleDegrees);
            float angle = Vector2.Angle(forward, toPoint.normalized);

            return angle <= arcAngle * 0.5f;
        }

        private bool TryHit(Hurtbox2D hurtbox)
        {
            Damageable target = hurtbox.Damageable;

            if (target == null || target.Team == ownerTeam)
            {
                return false;
            }

            if (hitOncePerTarget && hitTargets.Contains(target))
            {
                return false;
            }

            hitTargets.Add(target);

            DamageInfo damageInfo = new DamageInfo(
                attacker: owner,
                target: target,
                damage: damage,
                hitPoint: transform.position,
                knockbackDirection: direction,
                knockbackForce: 0f,
                canCritical: canCritical,
                isCritical: false,
                sourceSkill: sourceSkill,
                sourceDisplayName: sourceDisplayName,
                canKnockUp: canKnockUp,
                knockUpVelocity: knockUpVelocity,
                airborneDuration: airborneDuration,
                canApplyHitStun: canApplyHitStun,
                hitStunDuration: hitStunDuration,
                interruptType: interruptType);

            target.ApplyDamage(damageInfo);

            if (destroyOnFirstHit)
            {
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            DrawShapeGizmo(filled: true);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            DrawShapeGizmo(filled: false);
        }

        private void DrawShapeGizmo(bool filled)
        {
            switch (shape)
            {
                case HitboxShape.Circle:
                    DrawCircleGizmo(radius);
                    break;
                case HitboxShape.Capsule:
                    DrawRotatedBoxGizmo(filled);
                    break;
                case HitboxShape.Arc:
                    DrawArcGizmo();
                    break;
                case HitboxShape.Box:
                default:
                    DrawRotatedBoxGizmo(filled);
                    break;
            }
        }

        private void DrawRotatedBoxGizmo(bool filled)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position,
                Quaternion.Euler(0f, 0f, angleDegrees),
                Vector3.one);

            if (filled)
            {
                Gizmos.DrawCube(Vector3.zero, size);
            }
            else
            {
                Gizmos.DrawWireCube(Vector3.zero, size);
            }

            Gizmos.matrix = previousMatrix;
        }

        private void DrawCircleGizmo(float circleRadius)
        {
            const int segments = 32;
            Vector3 center = transform.position;
            Vector3 previous = center + (Vector3)(AngleToDirection(0f) * circleRadius);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i / (float)segments * 360f;
                Vector3 current = center + (Vector3)(AngleToDirection(angle) * circleRadius);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        private void DrawArcGizmo()
        {
            const int segments = 24;
            Vector3 center = transform.position;
            float startAngle = angleDegrees - arcAngle * 0.5f;
            float endAngle = angleDegrees + arcAngle * 0.5f;

            Vector3 previousOuter = center + (Vector3)(AngleToDirection(startAngle) * radius);
            Gizmos.DrawLine(center, previousOuter);

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                Vector3 currentOuter = center + (Vector3)(AngleToDirection(angle) * radius);
                Gizmos.DrawLine(previousOuter, currentOuter);
                previousOuter = currentOuter;
            }

            Vector3 endOuter = center + (Vector3)(AngleToDirection(endAngle) * radius);
            Gizmos.DrawLine(center, endOuter);

            if (innerRadius > 0f)
            {
                DrawInnerArcGizmo(startAngle, endAngle, segments);
            }
        }

        private void DrawInnerArcGizmo(float startAngle, float endAngle, int segments)
        {
            Vector3 center = transform.position;
            Vector3 previousInner = center + (Vector3)(AngleToDirection(startAngle) * innerRadius);

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                Vector3 currentInner = center + (Vector3)(AngleToDirection(angle) * innerRadius);
                Gizmos.DrawLine(previousInner, currentInner);
                previousInner = currentInner;
            }
        }

        private static Vector2 AngleToDirection(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
