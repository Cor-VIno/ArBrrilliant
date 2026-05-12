using UnityEngine;

namespace JingHongLu.Combat
{
    [CreateAssetMenu(
        fileName = "ProjectileData",
        menuName = "JingHongLu/Combat/Projectile Data")]
    public sealed class ProjectileData : ScriptableObject
    {
        [Header("Basic")]
        [Tooltip("Optional projectile prefab. If empty, a default GameObject is created at runtime.")]
        [SerializeField] private GameObject projectilePrefab = null;

        [Header("Motion")]
        [Tooltip("Projectile motion type. Linear travels straight; Parabolic applies gravity.")]
        [SerializeField] private ProjectileMotionType motionType = ProjectileMotionType.Linear;

        [Tooltip("Initial speed.")]
        [SerializeField] private float speed = 12f;

        [Tooltip("Projectile lifetime in seconds.")]
        [SerializeField] private float lifetime = 1.2f;

        [Tooltip("Used by Parabolic. Positive values accelerate downward.")]
        [SerializeField] private float gravity = 0f;

        [Tooltip("Spawn offset. X follows aim direction; Y is perpendicular to aim direction.")]
        [SerializeField] private Vector2 spawnOffset = new Vector2(0.8f, 0f);

        [Tooltip("Rotate projectile object to face current velocity.")]
        [SerializeField] private bool rotateToVelocity = true;

        [Header("Hit")]
        [Tooltip("Destroy projectile after hitting the first target.")]
        [SerializeField] private bool destroyOnFirstHit = false;

        [Header("Impact")]
        [Tooltip("Layers that destroy this projectile on contact, such as Ground or Wall.")]
        [SerializeField] private LayerMask impactLayerMask = default;

        [Tooltip("Radius used to check environment impact.")]
        [SerializeField] private float impactCheckRadius = 0.15f;

        [Tooltip("Destroy projectile when it touches impact layers.")]
        [SerializeField] private bool destroyOnImpact = true;

        public GameObject ProjectilePrefab => projectilePrefab;
        public ProjectileMotionType MotionType => motionType;
        public float Speed => speed;
        public float Lifetime => lifetime;
        public float Gravity => gravity;
        public Vector2 SpawnOffset => spawnOffset;
        public bool RotateToVelocity => rotateToVelocity;
        public bool DestroyOnFirstHit => destroyOnFirstHit;
        public LayerMask ImpactLayerMask => impactLayerMask;
        public float ImpactCheckRadius => impactCheckRadius;
        public bool DestroyOnImpact => destroyOnImpact;
    }
}
