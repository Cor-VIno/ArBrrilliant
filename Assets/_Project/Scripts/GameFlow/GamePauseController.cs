using JingHongLu.Input;
using JingHongLu.Player;
using JingHongLu.SwordArts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JingHongLu.GameFlow
{
    public sealed class GamePauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuRoot;
        [SerializeField] private CanvasGroup pauseCanvasGroup;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerControlLockController controlLockController;
        [SerializeField] private InsightSelectionController insightSelection;
        [SerializeField] private string bootSceneName = "00_Boot";
        [SerializeField] private bool pauseOnEscape = true;
        [SerializeField] private bool logPause;

        private readonly object pauseLockSource = new object();
        private bool isPaused;
        private float previousTimeScale = 1f;
        private float previousFixedDeltaTime;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            ResolveReferences();
            previousFixedDeltaTime = Time.fixedDeltaTime;
            SetPauseMenuVisible(false);
        }

        private void Update()
        {
            if (!pauseOnEscape || !UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (!isPaused && insightSelection != null && insightSelection.IsActive)
            {
                return;
            }

            TogglePause();
        }

        private void OnDisable()
        {
            if (isPaused)
            {
                ResumeInternal(forceRestoreTime: true);
            }
        }

        public void Pause()
        {
            if (isPaused)
            {
                return;
            }

            ResolveReferences();
            isPaused = true;
            previousTimeScale = Time.timeScale;
            previousFixedDeltaTime = Time.fixedDeltaTime;

            SetPauseMenuVisible(true);

            Time.timeScale = 0f;

            if (controlLockController != null)
            {
                controlLockController.AddLock(pauseLockSource, PlayerControlLockFlags.All);
            }
            else if (inputReader != null)
            {
                inputReader.SetGameplayInputBlocked(true);
            }

            if (logPause)
            {
                Debug.Log("[Pause] Game paused.", this);
            }
        }

        public void Resume()
        {
            ResumeInternal(forceRestoreTime: false);
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void RestartScene()
        {
            ClearPauseStateForSceneLoad();
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        public void ReturnToBoot()
        {
            ClearPauseStateForSceneLoad();
            SceneManager.LoadScene(bootSceneName);
        }

        public void QuitGame()
        {
            ClearPauseStateForSceneLoad();

#if UNITY_EDITOR
            Debug.Log("[Pause] Quit requested. Ignored in editor.", this);
#else
            Application.Quit();
#endif
        }

        private void ResumeInternal(bool forceRestoreTime)
        {
            if (!isPaused && !forceRestoreTime)
            {
                return;
            }

            isPaused = false;

            SetPauseMenuVisible(false);

            RemoveInputLock();
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            Time.fixedDeltaTime = previousFixedDeltaTime > 0f
                ? previousFixedDeltaTime
                : Time.fixedDeltaTime;

            if (logPause)
            {
                Debug.Log("[Pause] Game resumed.", this);
            }
        }

        private void ClearPauseStateForSceneLoad()
        {
            isPaused = false;
            RemoveInputLock();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = previousFixedDeltaTime > 0f
                ? previousFixedDeltaTime
                : Time.fixedDeltaTime;
        }

        private void RemoveInputLock()
        {
            if (controlLockController != null)
            {
                controlLockController.RemoveLock(pauseLockSource);
            }
            else if (inputReader != null)
            {
                inputReader.SetGameplayInputBlocked(false);
            }
        }

        private void ResolveReferences()
        {
            if (pauseCanvasGroup == null && pauseMenuRoot != null)
            {
                pauseCanvasGroup = pauseMenuRoot.GetComponent<CanvasGroup>();
            }

            if (inputReader == null)
            {
                inputReader = FindAnyObjectByType<PlayerInputReader>();
            }

            if (controlLockController == null)
            {
                controlLockController = FindAnyObjectByType<PlayerControlLockController>();
            }

            if (insightSelection == null)
            {
                insightSelection = FindAnyObjectByType<InsightSelectionController>();
            }
        }

        private void SetPauseMenuVisible(bool visible)
        {
            if (pauseMenuRoot == null)
            {
                return;
            }

            if (!pauseMenuRoot.activeSelf)
            {
                pauseMenuRoot.SetActive(true);
            }

            ResolveReferences();

            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = visible ? 1f : 0f;
                pauseCanvasGroup.interactable = visible;
                pauseCanvasGroup.blocksRaycasts = visible;
                return;
            }

            pauseMenuRoot.SetActive(visible);
        }
    }
}
