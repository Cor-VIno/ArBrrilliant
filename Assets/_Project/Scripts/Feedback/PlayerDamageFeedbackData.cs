using UnityEngine;

namespace JingHongLu.Feedback
{
    [CreateAssetMenu(
        fileName = "PlayerDamageFeedbackData",
        menuName = "JingHongLu/Feedback/Player Damage Feedback Data")]
    public sealed class PlayerDamageFeedbackData : ScriptableObject
    {
        [SerializeField] private FeedbackCue hitCue = new FeedbackCue();
        [SerializeField] private FeedbackCue deathCue = new FeedbackCue();

        [Header("Camera Shake")]
        [SerializeField] private bool shakeOnHit;
        [SerializeField] private float hitShakeDuration = 0.08f;
        [SerializeField] private float hitShakeStrength = 0.04f;

        public FeedbackCue HitCue => hitCue;
        public FeedbackCue DeathCue => deathCue;
        public bool ShakeOnHit => shakeOnHit;
        public float HitShakeDuration => hitShakeDuration;
        public float HitShakeStrength => hitShakeStrength;
    }
}
