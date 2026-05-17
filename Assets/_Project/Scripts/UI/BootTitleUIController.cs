using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JingHongLu.UI
{
    public sealed class BootTitleUIController : MonoBehaviour
    {
        [SerializeField] private GameObject titleRoot;
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private UIFadeGraphic clickPromptFade;
        [SerializeField] private UILoadingDotsAnimator loadingDotsAnimator;
        [SerializeField] private bool clickAnywhereToStart = true;
        [SerializeField] private bool logStartRequest = true;

        [Header("Scene Loading")]
        [SerializeField] private string targetSceneName = "02_Combat_Test";
        [SerializeField] private float minimumLoadingTime = 1f;
        [SerializeField] private bool loadSceneAfterStart = true;

        [Header("Transition")]
        [SerializeField] private UIFadeOverlay fadeOverlay;
        [SerializeField] private float fadeInDuration = 0.25f;
        [SerializeField] private float fadeOutDuration = 0.35f;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource bootAudioSource;

        private bool hasStarted;

        private void Start()
        {
            SetTitleVisible(true);
            SetLoadingVisible(false);

            if (clickPromptFade != null)
            {
                clickPromptFade.Play();
            }

            if (loadingDotsAnimator != null)
            {
                loadingDotsAnimator.ResetDots();
                loadingDotsAnimator.Stop();
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.SetAlpha(0f);
            }
        }

        private void Update()
        {
            if (!clickAnywhereToStart || hasStarted)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.anyKeyDown)
            {
                RequestStart();
            }
        }

        public void RequestStart()
        {
            if (hasStarted)
            {
                return;
            }

            hasStarted = true;

            // 音频播放逻辑
            if (bootAudioSource != null && bootAudioSource.clip != null)
            {
                DontDestroyOnLoad(bootAudioSource.gameObject);
                bootAudioSource.Play();
                Destroy(bootAudioSource.gameObject, bootAudioSource.clip.length);
            }

            StartCoroutine(StartFlowRoutine());
        }

        private IEnumerator StartFlowRoutine()
        {
            if (fadeOverlay != null)
            {
                yield return fadeOverlay.FadeIn(fadeInDuration);
            }

            SetTitleVisible(false);
            SetLoadingVisible(true);

            if (loadingDotsAnimator != null)
            {
                loadingDotsAnimator.ResetDots();
                loadingDotsAnimator.Play();
            }

            if (logStartRequest)
            {
                Debug.Log("Boot title start requested.", this);
            }

            if (fadeOverlay != null)
            {
                yield return fadeOverlay.FadeOut(fadeOutDuration);
            }

            if (loadSceneAfterStart)
            {
                yield return LoadTargetSceneRoutine();
            }
        }

        private IEnumerator LoadTargetSceneRoutine()
        {
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.LogWarning("Target scene name is empty.", this);
                yield break;
            }

            float startTime = Time.unscaledTime;
            AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

            if (operation == null)
            {
                Debug.LogError($"Failed to load scene: {targetSceneName}", this);
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                yield return null;
            }

            float elapsed = Time.unscaledTime - startTime;
            float remaining = minimumLoadingTime - elapsed;

            if (remaining > 0f)
            {
                yield return new WaitForSecondsRealtime(remaining);
            }

            operation.allowSceneActivation = true;
        }

        private void SetTitleVisible(bool visible)
        {
            if (titleRoot != null)
            {
                titleRoot.SetActive(visible);
            }
        }

        private void SetLoadingVisible(bool visible)
        {
            if (loadingRoot != null)
            {
                loadingRoot.SetActive(visible);
            }
        }
    }
}
