using UnityEngine;

namespace JingHongLu.Feedback
{
    [CreateAssetMenu(
        fileName = "EnemyCombatFeedbackData",
        menuName = "JingHongLu/Feedback/Enemy Combat Feedback Data")]
    public sealed class EnemyCombatFeedbackData : ScriptableObject
    {
        [SerializeField] private FeedbackCue hitCue = new FeedbackCue();
        [SerializeField] private FeedbackCue shieldHitCue = new FeedbackCue();
        [SerializeField] private FeedbackCue shieldBlockCue = new FeedbackCue();
        [SerializeField] private FeedbackCue shieldBreakCue = new FeedbackCue();
        [SerializeField] private FeedbackCue toughnessBreakCue = new FeedbackCue();

        [Header("Camera Shake")]
        [SerializeField] private bool shakeOnHit;
        [SerializeField] private float hitShakeDuration = 0.08f;
        [SerializeField] private float hitShakeStrength = 0.04f;
        [SerializeField] private bool shakeOnShieldBreak = true;
        [SerializeField] private float shieldBreakShakeDuration = 0.12f;
        [SerializeField] private float shieldBreakShakeStrength = 0.06f;
        [SerializeField] private bool shakeOnToughnessBreak = true;
        [SerializeField] private float toughnessBreakShakeDuration = 0.16f;
        [SerializeField] private float toughnessBreakShakeStrength = 0.08f;

        public FeedbackCue HitCue => hitCue;
        public FeedbackCue ShieldHitCue => shieldHitCue;
        public FeedbackCue ShieldBlockCue => shieldBlockCue;
        public FeedbackCue ShieldBreakCue => shieldBreakCue;
        public FeedbackCue ToughnessBreakCue => toughnessBreakCue;
        public bool ShakeOnHit => shakeOnHit;
        public float HitShakeDuration => hitShakeDuration;
        public float HitShakeStrength => hitShakeStrength;
        public bool ShakeOnShieldBreak => shakeOnShieldBreak;
        public float ShieldBreakShakeDuration => shieldBreakShakeDuration;
        public float ShieldBreakShakeStrength => shieldBreakShakeStrength;
        public bool ShakeOnToughnessBreak => shakeOnToughnessBreak;
        public float ToughnessBreakShakeDuration => toughnessBreakShakeDuration;
        public float ToughnessBreakShakeStrength => toughnessBreakShakeStrength;
    }
}
