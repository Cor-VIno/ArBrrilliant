using UnityEngine;

namespace JingHongLu.Input
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputBindingProfile bindingProfile;

        private InputBindingProfile runtimeDefaultProfile;

        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DodgePressed { get; private set; }
        public bool SkillSlot1Pressed { get; private set; }
        public bool SkillSlot2Pressed { get; private set; }
        public bool SkillSlot3Pressed { get; private set; }
        public bool SkillSlot4Pressed { get; private set; }

        public InputBindingProfile ActiveBindingProfile => bindingProfile != null
            ? bindingProfile
            : runtimeDefaultProfile;

        private void Awake()
        {
            EnsureBindingProfile();
        }

        private void Update()
        {
            ResetFrameInput();

            InputBindingProfile profile = EnsureBindingProfile();
            ReadMovement(profile);
            ReadActions(profile);
        }

        public void SetBindingProfile(InputBindingProfile newBindingProfile)
        {
            bindingProfile = newBindingProfile;
            EnsureBindingProfile();
        }

        private InputBindingProfile EnsureBindingProfile()
        {
            if (bindingProfile != null)
            {
                return bindingProfile;
            }

            if (runtimeDefaultProfile == null)
            {
                runtimeDefaultProfile = ScriptableObject.CreateInstance<InputBindingProfile>();
                runtimeDefaultProfile.ResetToDefaultBindings();
                Debug.LogWarning(
                    $"{nameof(PlayerInputReader)} on {name} has no {nameof(InputBindingProfile)} assigned. Runtime default bindings are being used.",
                    this);
            }

            return runtimeDefaultProfile;
        }

        private void ResetFrameInput()
        {
            JumpPressed = false;
            DodgePressed = false;
            SkillSlot1Pressed = false;
            SkillSlot2Pressed = false;
            SkillSlot3Pressed = false;
            SkillSlot4Pressed = false;
        }

        private void ReadMovement(InputBindingProfile profile)
        {
            int horizontal = 0;
            KeyCode moveLeftKey = profile.GetPrimaryKey(GameplayInputAction.MoveLeft);
            KeyCode moveRightKey = profile.GetPrimaryKey(GameplayInputAction.MoveRight);
            KeyCode alternateMoveLeftKey = profile.GetAlternateKey(GameplayInputAction.MoveLeft);
            KeyCode alternateMoveRightKey = profile.GetAlternateKey(GameplayInputAction.MoveRight);

            if (IsHeld(moveLeftKey) || IsHeld(alternateMoveLeftKey))
            {
                horizontal--;
            }

            if (IsHeld(moveRightKey) || IsHeld(alternateMoveRightKey))
            {
                horizontal++;
            }

            MoveInput = new Vector2(Mathf.Clamp(horizontal, -1, 1), 0f);
        }

        private void ReadActions(InputBindingProfile profile)
        {
            KeyCode jumpKey = profile.GetPrimaryKey(GameplayInputAction.Jump);

            JumpPressed = IsPressed(jumpKey);
            JumpHeld = IsHeld(jumpKey);
            DodgePressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.Dodge));
            SkillSlot1Pressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot1));
            SkillSlot2Pressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot2));
            SkillSlot3Pressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot3));
            SkillSlot4Pressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot4));
        }

        private static bool IsPressed(KeyCode key)
        {
            return key != KeyCode.None && global::UnityEngine.Input.GetKeyDown(key);
        }

        private static bool IsHeld(KeyCode key)
        {
            return key != KeyCode.None && global::UnityEngine.Input.GetKey(key);
        }
    }
}
