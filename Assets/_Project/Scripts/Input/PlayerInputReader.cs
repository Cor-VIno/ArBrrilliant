using System;
using JingHongLu.Player;
using UnityEngine;

namespace JingHongLu.Input
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputBindingProfile bindingProfile;
        [SerializeField] private PlayerControlLockController controlLock;

        private InputBindingProfile runtimeDefaultProfile;
        private readonly object gameplayInputBlockSource = new object();
        private bool gameplayInputBlocked;

        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DodgePressed { get; private set; }
        public bool SkillSlot1Pressed { get; private set; }
        public bool SkillSlot2Pressed { get; private set; }
        public bool SkillSlot3Pressed { get; private set; }
        public bool SkillSlot4Pressed { get; private set; }
        public bool SkillSlot1Held { get; private set; }
        public bool SkillSlot2Held { get; private set; }
        public bool SkillSlot3Held { get; private set; }
        public bool SkillSlot4Held { get; private set; }
        public bool SkillSlot1Released { get; private set; }
        public bool SkillSlot2Released { get; private set; }
        public bool SkillSlot3Released { get; private set; }
        public bool SkillSlot4Released { get; private set; }

        public event Action OnSwordArtReleasePressed;
        public event Action OnCancelPressed;
        public event Action OnBlockedGameplayInputPressed;

        public InputBindingProfile ActiveBindingProfile => bindingProfile != null
            ? bindingProfile
            : runtimeDefaultProfile;
        public bool GameplayInputBlocked => gameplayInputBlocked ||
            (controlLock != null &&
            (controlLock.IsMoveLocked ||
                controlLock.IsJumpLocked ||
                controlLock.IsBasicSkillLocked ||
                controlLock.IsDashLocked));

        private void Awake()
        {
            ResolveReferences();
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

        public void SetGameplayInputBlocked(bool blocked)
        {
            gameplayInputBlocked = blocked;
            ResolveReferences();

            if (controlLock != null)
            {
                if (blocked)
                {
                    controlLock.AddLock(
                        gameplayInputBlockSource,
                        PlayerControlLockFlags.Gameplay);
                    ClearGameplayInputs();
                }
                else
                {
                    controlLock.RemoveLock(gameplayInputBlockSource);
                }

                return;
            }

            if (blocked)
            {
                ClearGameplayInputs();
            }
        }

        private void ResolveReferences()
        {
            if (controlLock == null)
            {
                controlLock = GetComponentInParent<PlayerControlLockController>();
            }
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
            SkillSlot1Released = false;
            SkillSlot2Released = false;
            SkillSlot3Released = false;
            SkillSlot4Released = false;
        }

        private void ReadMovement(InputBindingProfile profile)
        {
            if (IsMoveInputLocked())
            {
                MoveInput = Vector2.zero;
                return;
            }

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
            bool gameplayLocked = IsLegacyGameplayBlocked();
            bool jumpLocked = gameplayLocked || IsJumpLocked();
            bool basicSkillLocked = gameplayLocked || IsBasicSkillLocked();
            bool dashLocked = gameplayLocked || IsDashLocked();

            if (IsPressed(profile.GetPrimaryKey(GameplayInputAction.Cancel)))
            {
                OnCancelPressed?.Invoke();
            }

            if (!IsSwordArtLocked() &&
                IsPressed(profile.GetPrimaryKey(GameplayInputAction.SwordArtRelease)))
            {
                OnSwordArtReleasePressed?.Invoke();
            }

            if (jumpLocked || basicSkillLocked || dashLocked)
            {
                if (IsBlockedGameplayActionPressed(
                    profile,
                    jumpLocked,
                    basicSkillLocked,
                    dashLocked))
                {
                    OnBlockedGameplayInputPressed?.Invoke();
                }

                if (jumpLocked)
                {
                    JumpPressed = false;
                    JumpHeld = false;
                }

                if (dashLocked)
                {
                    DodgePressed = false;
                    SkillSlot4Pressed = false;
                    SkillSlot4Held = false;
                    SkillSlot4Released = false;
                }

                if (basicSkillLocked)
                {
                    ClearSkillInputs();
                }

                if (jumpLocked && basicSkillLocked && dashLocked)
                {
                    return;
                }
            }

            KeyCode jumpKey = profile.GetPrimaryKey(GameplayInputAction.Jump);

            if (!jumpLocked)
            {
                JumpPressed = IsPressed(jumpKey);
                JumpHeld = IsHeld(jumpKey);
            }

            if (!dashLocked)
            {
                DodgePressed = IsPressed(profile.GetPrimaryKey(GameplayInputAction.Dodge));
            }

            KeyCode skillSlot1Key = profile.GetPrimaryKey(GameplayInputAction.SkillSlot1);
            KeyCode skillSlot2Key = profile.GetPrimaryKey(GameplayInputAction.SkillSlot2);
            KeyCode skillSlot3Key = profile.GetPrimaryKey(GameplayInputAction.SkillSlot3);
            KeyCode skillSlot4Key = profile.GetPrimaryKey(GameplayInputAction.SkillSlot4);

            if (!basicSkillLocked)
            {
                SkillSlot1Pressed = IsPressed(skillSlot1Key);
                SkillSlot2Pressed = IsPressed(skillSlot2Key);
                SkillSlot3Pressed = IsPressed(skillSlot3Key);
                SkillSlot1Held = IsHeld(skillSlot1Key);
                SkillSlot2Held = IsHeld(skillSlot2Key);
                SkillSlot3Held = IsHeld(skillSlot3Key);
                SkillSlot1Released = IsReleased(skillSlot1Key);
                SkillSlot2Released = IsReleased(skillSlot2Key);
                SkillSlot3Released = IsReleased(skillSlot3Key);
            }

            if (!basicSkillLocked && !dashLocked)
            {
                SkillSlot4Pressed = IsPressed(skillSlot4Key);
                SkillSlot4Held = IsHeld(skillSlot4Key);
                SkillSlot4Released = IsReleased(skillSlot4Key);
            }

        }

        private void ClearGameplayInputs()
        {
            MoveInput = Vector2.zero;
            JumpPressed = false;
            JumpHeld = false;
            DodgePressed = false;
            SkillSlot1Pressed = false;
            SkillSlot2Pressed = false;
            SkillSlot3Pressed = false;
            SkillSlot4Pressed = false;
            SkillSlot1Held = false;
            SkillSlot2Held = false;
            SkillSlot3Held = false;
            SkillSlot4Held = false;
            SkillSlot1Released = false;
            SkillSlot2Released = false;
            SkillSlot3Released = false;
            SkillSlot4Released = false;
        }

        private void ClearSkillInputs()
        {
            SkillSlot1Pressed = false;
            SkillSlot2Pressed = false;
            SkillSlot3Pressed = false;
            SkillSlot4Pressed = false;
            SkillSlot1Held = false;
            SkillSlot2Held = false;
            SkillSlot3Held = false;
            SkillSlot4Held = false;
            SkillSlot1Released = false;
            SkillSlot2Released = false;
            SkillSlot3Released = false;
            SkillSlot4Released = false;
        }

        private static bool IsBlockedGameplayActionPressed(
            InputBindingProfile profile,
            bool jumpLocked,
            bool basicSkillLocked,
            bool dashLocked)
        {
            return (jumpLocked && IsPressed(profile.GetPrimaryKey(GameplayInputAction.Jump)))
                || (dashLocked && IsPressed(profile.GetPrimaryKey(GameplayInputAction.Dodge)))
                || (basicSkillLocked && IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot1)))
                || (basicSkillLocked && IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot2)))
                || (basicSkillLocked && IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot3)))
                || ((basicSkillLocked || dashLocked) &&
                    IsPressed(profile.GetPrimaryKey(GameplayInputAction.SkillSlot4)));
        }

        private bool IsMoveInputLocked()
        {
            return gameplayInputBlocked || (controlLock != null && controlLock.IsMoveLocked);
        }

        private bool IsLegacyGameplayBlocked()
        {
            return gameplayInputBlocked;
        }

        private bool IsJumpLocked()
        {
            return controlLock != null && controlLock.IsJumpLocked;
        }

        private bool IsBasicSkillLocked()
        {
            return controlLock != null && controlLock.IsBasicSkillLocked;
        }

        private bool IsSwordArtLocked()
        {
            return controlLock != null && controlLock.IsSwordArtLocked;
        }

        private bool IsDashLocked()
        {
            return controlLock != null && controlLock.IsDashLocked;
        }

        private static bool IsPressed(KeyCode key)
        {
            return key != KeyCode.None && global::UnityEngine.Input.GetKeyDown(key);
        }

        private static bool IsHeld(KeyCode key)
        {
            return key != KeyCode.None && global::UnityEngine.Input.GetKey(key);
        }

        private static bool IsReleased(KeyCode key)
        {
            return key != KeyCode.None && global::UnityEngine.Input.GetKeyUp(key);
        }
    }
}
