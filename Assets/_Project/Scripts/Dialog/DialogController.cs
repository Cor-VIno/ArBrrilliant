using JingHongLu.Input;
using JingHongLu.GameFlow;
using JingHongLu.Player;
using TMPro;
using UnityEngine;

namespace JingHongLu.Dialog
{
    public sealed class DialogController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private TextMeshProUGUI continueHintText;

        [Header("Input")]
        [SerializeField] private PlayerControlLockController playerControlLock;
        [SerializeField] private PlayerInputReader playerInputReader;
        [SerializeField] private CombatPauseController combatPauseController;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;
        [SerializeField] private bool lockPlayerDuringDialog = true;
        [SerializeField] private bool pauseCombatDuringDialog = true;
        [SerializeField] private bool logDialog;

        [Header("Debug")]
        [SerializeField] private DialogData debugDialogData;
        [SerializeField] private bool enableDebugKeys = true;
        [SerializeField] private KeyCode toggleDebugViewKey = KeyCode.F6;
        [SerializeField] private KeyCode debugStartDialogKey = KeyCode.F7;
        [SerializeField] private string debugSpeakerName = "调试";
        [SerializeField] private string debugText = "这是一条对话 UI 调试文本。";

        private DialogData currentDialogData;
        private int currentLineIndex = -1;
        private bool inputLockedByDialog;
        private bool inputReaderBlockedByDialog;
        private bool combatPausedByDialog;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            ResolveReferences();
            SetRootVisible(false);
        }

        private void Update()
        {
            if (enableDebugKeys)
            {
                if (UnityEngine.Input.GetKeyDown(toggleDebugViewKey))
                {
                    ToggleDebugView();
                }

                if (UnityEngine.Input.GetKeyDown(debugStartDialogKey))
                {
                    DebugStartDialog();
                }
            }

            if (IsPlaying && UnityEngine.Input.GetKeyDown(advanceKey))
            {
                Advance();
            }
        }

        public void StartDialog(DialogData dialogData)
        {
            if (dialogData == null || dialogData.Lines == null || dialogData.Lines.Count == 0)
            {
                if (logDialog)
                {
                    Debug.LogWarning("[Dialog] StartDialog ignored because dialog data is empty.", this);
                }

                return;
            }

            ResolveReferences();
            currentDialogData = dialogData;
            currentLineIndex = 0;
            IsPlaying = true;
            SetRootVisible(true);
            ApplyInputLock();
            ApplyCombatPause();
            ShowLine(currentLineIndex);

            if (logDialog)
            {
                Debug.Log($"[Dialog] Started: {dialogData.name}", this);
            }
        }

        public void Advance()
        {
            if (!IsPlaying || currentDialogData == null)
            {
                return;
            }

            int nextIndex = currentLineIndex + 1;
            if (nextIndex >= currentDialogData.Lines.Count)
            {
                StopDialog();
                return;
            }

            currentLineIndex = nextIndex;
            ShowLine(currentLineIndex);
        }

        public void StopDialog()
        {
            if (!IsPlaying && currentDialogData == null)
            {
                SetRootVisible(false);
                return;
            }

            ReleaseInputLock();
            ReleaseCombatPause();
            IsPlaying = false;
            currentDialogData = null;
            currentLineIndex = -1;
            SetRootVisible(false);

            if (logDialog)
            {
                Debug.Log("[Dialog] Stopped.", this);
            }
        }

        public void ShowDebugView()
        {
            if (IsPlaying)
            {
                return;
            }

            SetRootVisible(true);
            SetText(debugSpeakerName, debugText, "Space");
        }

        public void HideDebugView()
        {
            if (IsPlaying)
            {
                return;
            }

            SetRootVisible(false);
        }

        public void ToggleDebugView()
        {
            if (root != null && root.activeSelf)
            {
                HideDebugView();
                return;
            }

            ShowDebugView();
        }

        public void DebugStartDialog()
        {
            if (debugDialogData == null)
            {
                Debug.LogWarning("[Dialog] Debug dialog data is not assigned.", this);
                return;
            }

            StartDialog(debugDialogData);
        }

        [ContextMenu("Debug/Show Dialog View")]
        private void ContextShowDebugView()
        {
            ShowDebugView();
        }

        [ContextMenu("Debug/Hide Dialog View")]
        private void ContextHideDebugView()
        {
            HideDebugView();
        }

        [ContextMenu("Debug/Start Debug Dialog")]
        private void ContextStartDebugDialog()
        {
            DebugStartDialog();
        }

        private void ShowLine(int index)
        {
            if (currentDialogData == null || index < 0 || index >= currentDialogData.Lines.Count)
            {
                return;
            }

            DialogLine line = currentDialogData.Lines[index];
            SetText(line.SpeakerName, line.Text, "Space");
        }

        private void SetText(string speakerName, string bodyText, string hintText)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName;
            }

            if (dialogText != null)
            {
                dialogText.text = bodyText;
            }

            if (continueHintText != null)
            {
                continueHintText.text = hintText;
            }
        }

        private void ApplyInputLock()
        {
            if (!lockPlayerDuringDialog)
            {
                return;
            }

            if (playerControlLock != null)
            {
                playerControlLock.AddLock(this, PlayerControlLockFlags.All);
                inputLockedByDialog = true;
                return;
            }

            if (playerInputReader != null)
            {
                playerInputReader.SetGameplayInputBlocked(true);
                inputReaderBlockedByDialog = true;
            }
        }

        private void ReleaseInputLock()
        {
            if (inputLockedByDialog && playerControlLock != null)
            {
                playerControlLock.RemoveLock(this);
            }

            if (inputReaderBlockedByDialog && playerInputReader != null)
            {
                playerInputReader.SetGameplayInputBlocked(false);
            }

            inputLockedByDialog = false;
            inputReaderBlockedByDialog = false;
        }

        private void ApplyCombatPause()
        {
            if (!pauseCombatDuringDialog || combatPausedByDialog)
            {
                return;
            }

            ResolveReferences();

            if (combatPauseController == null)
            {
                return;
            }

            combatPauseController.AddPause(this);
            combatPausedByDialog = true;
        }

        private void ReleaseCombatPause()
        {
            if (!combatPausedByDialog)
            {
                return;
            }

            if (combatPauseController != null)
            {
                combatPauseController.RemovePause(this);
            }

            combatPausedByDialog = false;
        }

        private void SetRootVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private void ResolveReferences()
        {
            if (playerInputReader == null)
            {
                playerInputReader = FindAnyObjectByType<PlayerInputReader>();
            }

            if (playerControlLock == null && playerInputReader != null)
            {
                playerControlLock = playerInputReader.GetComponentInParent<PlayerControlLockController>();
            }

            if (playerControlLock == null)
            {
                playerControlLock = FindAnyObjectByType<PlayerControlLockController>();
            }

            if (combatPauseController == null)
            {
                combatPauseController = FindAnyObjectByType<CombatPauseController>();
            }
        }

        private void OnDisable()
        {
            ReleaseInputLock();
            ReleaseCombatPause();
        }
    }
}
