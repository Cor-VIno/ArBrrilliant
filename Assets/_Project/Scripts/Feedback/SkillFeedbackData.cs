using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.Feedback
{
    [CreateAssetMenu(
        fileName = "SkillFeedbackData",
        menuName = "JingHongLu/Feedback/Skill Feedback Data")]
    public sealed class SkillFeedbackData : ScriptableObject
    {
        [SerializeField] private SkillData skill;
        [SerializeField] private FeedbackCue castStartedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue executedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue castFinishedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue chargeStartedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue chargeReleasedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue dashStartedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue dashFinishedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue projectileSpawnedCue = new FeedbackCue();

        public SkillData Skill => skill;
        public FeedbackCue CastStartedCue => castStartedCue;
        public FeedbackCue ExecutedCue => executedCue;
        public FeedbackCue CastFinishedCue => castFinishedCue;
        public FeedbackCue ChargeStartedCue => chargeStartedCue;
        public FeedbackCue ChargeReleasedCue => chargeReleasedCue;
        public FeedbackCue DashStartedCue => dashStartedCue;
        public FeedbackCue DashFinishedCue => dashFinishedCue;
        public FeedbackCue ProjectileSpawnedCue => projectileSpawnedCue;
    }
}
