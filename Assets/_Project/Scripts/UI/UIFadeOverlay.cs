using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class UIFadeOverlay : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private bool startTransparent = true;

        public bool IsFading { get; private set; }

        private void Awake()
        {
            if (fadeImage == null)
            {
                TryGetComponent(out fadeImage);
            }

            if (startTransparent)
            {
                SetAlpha(0f);
            }
        }

        public void SetAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }

        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (fadeImage == null)
            {
                yield break;
            }

            targetAlpha = Mathf.Clamp01(targetAlpha);

            if (duration <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            IsFading = true;
            float startAlpha = fadeImage.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
                yield return null;
            }

            SetAlpha(targetAlpha);
            IsFading = false;
        }

        public IEnumerator FadeIn(float duration)
        {
            yield return FadeTo(1f, duration);
        }

        public IEnumerator FadeOut(float duration)
        {
            yield return FadeTo(0f, duration);
        }
    }
}
