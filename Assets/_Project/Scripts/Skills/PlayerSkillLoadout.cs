using UnityEngine;

namespace JingHongLu.Skills
{
    [CreateAssetMenu(
        fileName = "PlayerSkillLoadout",
        menuName = "JingHongLu/Skills/Player Skill Loadout")]
    public sealed class PlayerSkillLoadout : ScriptableObject
    {
        [SerializeField] private SkillData slot1Skill = null;
        [SerializeField] private SkillData slot2Skill = null;
        [SerializeField] private SkillData slot3Skill = null;
        [SerializeField] private SkillData slot4Skill = null;

        public SkillData Slot1Skill => slot1Skill;
        public SkillData Slot2Skill => slot2Skill;
        public SkillData Slot3Skill => slot3Skill;
        public SkillData Slot4Skill => slot4Skill;

        public SkillData GetSkill(SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Slot1 => slot1Skill,
                SkillSlot.Slot2 => slot2Skill,
                SkillSlot.Slot3 => slot3Skill,
                SkillSlot.Slot4 => slot4Skill,
                _ => null
            };
        }
    }
}
