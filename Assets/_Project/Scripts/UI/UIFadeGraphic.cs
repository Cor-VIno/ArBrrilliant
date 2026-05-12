using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class UIFadeGraphic : MonoBehaviour
    {
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private float minAlpha = 0.15f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float duration = 3f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool loop = true;

        private bool isPlaying;
        private float elapsed;

        private void Awake()
        {
            if (targetGraphic == null)
            {
                TryGetComponent(out targetGraphic);
            }
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!isPlaying || targetGraphic == null)
            {
                return;
            }

            float safeDuration = Mathf.Max(0.01f, duration);
            elapsed += Time.unscaledDeltaTime;

            if (loop)
            {
                float t = Mathf.PingPong(elapsed / safeDuration, 1f);
                SetAlpha(Mathf.Lerp(minAlpha, maxAlpha, t));
                return;
            }

            float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);
            SetAlpha(Mathf.Lerp(minAlpha, maxAlpha, normalizedTime));

            if (normalizedTime >= 1f)
            {
                isPlaying = false;
            }
        }

        public void Play()
        {
            elapsed = 0f;
            isPlaying = true;
            SetAlpha(minAlpha);
        }

        public void Stop(bool snapToMaxAlpha = false)
        {
            isPlaying = false;

            if (snapToMaxAlpha)
            {
                SetAlpha(maxAlpha);
            }
        }

        public void SetAlpha(float alpha)
        {
            if (targetGraphic == null)
            {
                return;
            }

            Color color = targetGraphic.color;
            color.a = Mathf.Clamp01(alpha);
            targetGraphic.color = color;
        }

        public void SetAlphaRange(float minAlpha, float maxAlpha)
        {
            this.minAlpha = Mathf.Clamp01(minAlpha);
            this.maxAlpha = Mathf.Clamp01(maxAlpha);
        }

        public void SetDuration(float duration)
        {
            this.duration = Mathf.Max(0.01f, duration);
        }
    }
}
