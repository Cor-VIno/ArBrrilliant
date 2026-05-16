using UnityEngine;

namespace JingHongLu.Visuals
{
    [CreateAssetMenu(
        fileName = "SkillVisualData",
        menuName = "JingHongLu/Visuals/Skill Visual Data")]
    public sealed class SkillVisualData : ScriptableObject
    {
        [SerializeField] private VisualCueData castStartedCue = new VisualCueData();
        [SerializeField] private VisualCueData executedCue = new VisualCueData();
        [SerializeField] private VisualCueData castFinishedCue = new VisualCueData();

        public VisualCueData CastStartedCue => castStartedCue;
        public VisualCueData ExecutedCue => executedCue;
        public VisualCueData CastFinishedCue => castFinishedCue;
    }
}
