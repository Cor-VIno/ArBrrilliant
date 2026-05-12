using UnityEngine;

namespace JingHongLu.Input
{
    [CreateAssetMenu(
        fileName = "InputBindingProfile",
        menuName = "JingHongLu/Input/Input Binding Profile")]
    public sealed class InputBindingProfile : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode alternateMoveLeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode alternateMoveRightKey = KeyCode.RightArrow;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dodgeKey = KeyCode.LeftShift;

        [Header("Skill Slots")]
        [SerializeField] private KeyCode skillSlot1Key = KeyCode.Mouse0;
        [SerializeField] private KeyCode skillSlot2Key = KeyCode.Mouse1;
        [SerializeField] private KeyCode skillSlot3Key = KeyCode.Q;
        [SerializeField] private KeyCode skillSlot4Key = KeyCode.E;

        public KeyCode MoveLeftKey => moveLeftKey;
        public KeyCode MoveRightKey => moveRightKey;
        public KeyCode AlternateMoveLeftKey => alternateMoveLeftKey;
        public KeyCode AlternateMoveRightKey => alternateMoveRightKey;
        public KeyCode JumpKey => jumpKey;
        public KeyCode DodgeKey => dodgeKey;
        public KeyCode SkillSlot1Key => skillSlot1Key;
        public KeyCode SkillSlot2Key => skillSlot2Key;
        public KeyCode SkillSlot3Key => skillSlot3Key;
        public KeyCode SkillSlot4Key => skillSlot4Key;

        public void ResetToDefaultBindings()
        {
            moveLeftKey = KeyCode.A;
            moveRightKey = KeyCode.D;
            alternateMoveLeftKey = KeyCode.LeftArrow;
            alternateMoveRightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.Space;
            dodgeKey = KeyCode.LeftShift;
            skillSlot1Key = KeyCode.Mouse0;
            skillSlot2Key = KeyCode.Mouse1;
            skillSlot3Key = KeyCode.Q;
            skillSlot4Key = KeyCode.E;
        }

        public KeyCode GetPrimaryKey(GameplayInputAction action)
        {
            return action switch
            {
                GameplayInputAction.MoveLeft => moveLeftKey,
                GameplayInputAction.MoveRight => moveRightKey,
                GameplayInputAction.Jump => jumpKey,
                GameplayInputAction.Dodge => dodgeKey,
                GameplayInputAction.SkillSlot1 => skillSlot1Key,
                GameplayInputAction.SkillSlot2 => skillSlot2Key,
                GameplayInputAction.SkillSlot3 => skillSlot3Key,
                GameplayInputAction.SkillSlot4 => skillSlot4Key,
                _ => KeyCode.None
            };
        }

        public KeyCode GetAlternateKey(GameplayInputAction action)
        {
            return action switch
            {
                GameplayInputAction.MoveLeft => alternateMoveLeftKey,
                GameplayInputAction.MoveRight => alternateMoveRightKey,
                _ => KeyCode.None
            };
        }

        public void SetPrimaryKey(GameplayInputAction action, KeyCode key)
        {
            switch (action)
            {
                case GameplayInputAction.MoveLeft:
                    moveLeftKey = key;
                    break;
                case GameplayInputAction.MoveRight:
                    moveRightKey = key;
                    break;
                case GameplayInputAction.Jump:
                    jumpKey = key;
                    break;
                case GameplayInputAction.Dodge:
                    dodgeKey = key;
                    break;
                case GameplayInputAction.SkillSlot1:
                    skillSlot1Key = key;
                    break;
                case GameplayInputAction.SkillSlot2:
                    skillSlot2Key = key;
                    break;
                case GameplayInputAction.SkillSlot3:
                    skillSlot3Key = key;
                    break;
                case GameplayInputAction.SkillSlot4:
                    skillSlot4Key = key;
                    break;
            }
        }

        public void SetAlternateKey(GameplayInputAction action, KeyCode key)
        {
            switch (action)
            {
                case GameplayInputAction.MoveLeft:
                    alternateMoveLeftKey = key;
                    break;
                case GameplayInputAction.MoveRight:
                    alternateMoveRightKey = key;
                    break;
            }
        }
    }
}
