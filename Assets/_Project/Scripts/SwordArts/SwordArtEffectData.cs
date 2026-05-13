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

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.cyan;

        public SwordArtEffectType EffectType => effectType;
        public int Damage => damage;
        public bool CanCritical => canCritical;
        public HitboxShape HitboxShape => hitboxShape;
        public Vector2 HitboxSize => hitboxSize;
        public Vector2 HitboxOffset => hitboxOffset;
        public float HitboxDuration => hitboxDuration;
        public LayerMask TargetLayerMask => targetLayerMask;
        public float HitboxRadius => hitboxRadius;
        public float HitboxInnerRadius => hitboxInnerRadius;
        public float HitboxArcAngle => hitboxArcAngle;
        public float HitboxRotationOffset => hitboxRotationOffset;
        public Color GizmoColor => gizmoColor;
    }
}
