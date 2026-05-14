using UnityEngine;

namespace JingHongLu.Skills
{
    [CreateAssetMenu(
        fileName = "DashData",
        menuName = "JingHongLu/Skills/Dash Data")]
    public sealed class DashData : ScriptableObject
    {
        [Header("Motion")]
        [Tooltip("Total dash distance.")]
        [SerializeField] private float distance = 3f;

        [Tooltip("Dash duration in seconds.")]
        [SerializeField] private float duration = 0.18f;

        [Tooltip("How much dash velocity remains after dash ends. 0 stops immediately; 0.2 keeps 20%.")]
        [Range(0f, 1f)]
        [SerializeField] private float endVelocityMultiplier = 0f;

        [Header("Defense")]
        [Tooltip("Whether the owner is invincible during dash.")]
        [SerializeField] private bool invincibleDuringDash = true;

        [Header("Airborne Homing")]
        [SerializeField] private bool enableAirborneHoming = false;
        [SerializeField] private float airborneHomingSearchRadius = 5f;
        [SerializeField] private LayerMask airborneTargetLayerMask;
        [SerializeField] private float homingStopDistance = 0.3f;

        public float Distance => Mathf.Max(0.01f, distance);
        public float Duration => Mathf.Max(0.01f, duration);
        public float EndVelocityMultiplier => Mathf.Clamp01(endVelocityMultiplier);
        public bool InvincibleDuringDash => invincibleDuringDash;
        public bool EnableAirborneHoming => enableAirborneHoming;
        public float AirborneHomingSearchRadius => airborneHomingSearchRadius;
        public LayerMask AirborneTargetLayerMask => airborneTargetLayerMask;
        public float HomingStopDistance => homingStopDistance;
    }
}
