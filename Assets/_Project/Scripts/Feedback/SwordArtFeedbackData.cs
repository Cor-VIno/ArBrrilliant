using JingHongLu.SwordArts;
using UnityEngine;

namespace JingHongLu.Feedback
{
    [CreateAssetMenu(
        fileName = "SwordArtFeedbackData",
        menuName = "JingHongLu/Feedback/Sword Art Feedback Data")]
    public sealed class SwordArtFeedbackData : ScriptableObject
    {
        [SerializeField] private SwordArtData swordArt;
        [SerializeField] private FeedbackCue executionStartedCue = new FeedbackCue();
        [SerializeField] private FeedbackCue executionFinishedCue = new FeedbackCue();

        public SwordArtData SwordArt => swordArt;
        public FeedbackCue ExecutionStartedCue => executionStartedCue;
        public FeedbackCue ExecutionFinishedCue => executionFinishedCue;
    }
}
