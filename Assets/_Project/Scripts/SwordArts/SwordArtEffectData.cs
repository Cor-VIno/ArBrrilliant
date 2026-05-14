using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    [CreateAssetMenu(
        fileName = "SwordArtEffectData",
        menuName = "JingHongLu/SwordArts/Sword Art Effect Data")]
    public sealed class SwordArtEffectData : ScriptableObject
    {
        [Header("Basic")]
        [SerializeField] private SwordArtEffectType effectType = SwordArtEffectType.InstantHitbox;

        [Header("Damage")]
        [SerializeField] private int damage = 30;
        [SerializeField] private bool canCritical = true;

        [Header("Knock Up")]
        [SerializeField] private bool canKnockUp = false;
        [SerializeField] private Vector2 knockUpVelocity = new Vector2(3f, 8f);
        [SerializeField] private float airborneDuration = 1.5f;

        [Header("Hitbox")]
        [SerializeField] private HitboxShape hitboxShape = HitboxShape.Arc;
        [SerializeField] private Vector2 hitboxSize = new Vector2(2.5f, 1.5f);
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField] private float hitboxDuration = 0.16f;
        [SerializeField] private LayerMask targetLayerMask;

        [Header("Hitbox Shape")]
        [SerializeField] private float hitboxRadius = 2.6f;
        [SerializeField] private float hitboxInnerRadius = 0.2f;
        [SerializeField] private float hitboxArcAngle = 160f;
        [SerializeField] private float hitboxRotationOffset = 0f;

        [Header("Line Area")]
        [SerializeField] private float lineMaxLength = 8f;
        [SerializeField] private float lineWidth = 0.8f;
        [SerializeField] private float areaDuration = 3f;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private float tickDamage = 6f;
        [SerializeField] private float finalDamage = 24f;
        [SerializeField] private bool finalCanKnockUp = true;
        [SerializeField] private Vector2 finalKnockUpVelocity = new Vector2(3f, 7f);
        [SerializeField] private float finalAirborneDuration = 1.2f;

        [Header("Healing Area")]
        [SerializeField] private float healingRadius = 2.5f;
        [SerializeField] private float healingDuration = 4f;
        [SerializeField] private float healingTickInterval = 0.5f;
        [SerializeField] private float healingAmountPerTick = 6f;

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.cyan;

        public SwordArtEffectType EffectType => effectType;
        public int Damage => damage;
        public bool CanCritical => canCritical;
        public bool CanKnockUp => canKnockUp;
        public Vector2 KnockUpVelocity => knockUpVelocity;
        public float AirborneDuration => airborneDuration;
        public HitboxShape HitboxShape => hitboxShape;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float HitboxDuration => hitboxDuration;
        public LayerMask TargetLayerMask => targetLayerMask;
        public float HitboxRadius => hitboxRadius;
        public float HitboxInnerRadius => hitboxInnerRadius;
        public float HitboxArcAngle => hitboxArcAngle;
        public float HitboxRotationOffset => hitboxRotationOffset;
        public float LineMaxLength => lineMaxLength;
        public float LineWidth => lineWidth;
        public float AreaDuration => areaDuration;
        public float TickInterval => tickInterval;
        public float TickDamage => tickDamage;
        public float FinalDamage => finalDamage;
        public bool FinalCanKnockUp => finalCanKnockUp;
        public Vector2 FinalKnockUpVelocity => finalKnockUpVelocity;
        public float FinalAirborneDuration => finalAirborneDuration;
        public float HealingRadius => healingRadius;
        public float HealingDuration => healingDuration;
        public float HealingTickInterval => healingTickInterval;
        public float HealingAmountPerTick => healingAmountPerTick;
        public Color GizmoColor => gizmoColor;
    }
}
