using System.Collections.Generic;
using JingHongLu.Skills;
using UnityEngine;

namespace JingHongLu.SwordArts
{
    [CreateAssetMenu(
        fileName = "SwordArtData",
        menuName = "JingHongLu/SwordArts/Sword Art Data")]
    public sealed class SwordArtData : ScriptableObject
    {
        [Header("Basic")]
        [SerializeField] private string swordArtId = string.Empty;
        [SerializeField] private string displayName = string.Empty;

        [Header("Sequence")]
        [SerializeField] private StrokeType[] requiredSequence;

        [Header("Match")]
        [SerializeField] private SwordArtMatchMode matchMode =
            SwordArtMatchMode.UnorderedCounts;

        [Header("Trigger")]
        [SerializeField] private bool consumeMatchedStrokes = true;
        [SerializeField] private float cooldown = 0f;

        [Header("Timing")]
        [SerializeField] private float executionDelay = 0.25f;

        [Header("Effect")]
        [SerializeField] private SwordArtEffectData effectData;

        public string SwordArtId => swordArtId;
        public string DisplayName => displayName;
        public IReadOnlyList<StrokeType> RequiredSequence => requiredSequence;
        public SwordArtMatchMode MatchMode => matchMode;
        public bool ConsumeMatchedStrokes => consumeMatchedStrokes;
        public float Cooldown => cooldown;
        public float ExecutionDelay => executionDelay;
        public SwordArtEffectData EffectData => effectData;
    }
}
