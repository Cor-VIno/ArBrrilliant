using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    public readonly struct StrokeRecord
    {
        public StrokeRecord(StrokeType strokeType, SkillData sourceSkill, float time)
        {
            StrokeType = strokeType;
            SourceSkill = sourceSkill;
            Time = time;
        }

        public StrokeType StrokeType { get; }
        public SkillData SourceSkill { get; }
        public float Time { get; }
    }
}
