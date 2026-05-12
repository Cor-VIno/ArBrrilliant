using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.UI
{
    public sealed class UILoadingDotsAnimator : MonoBehaviour
    {
        [SerializeField] private Image[] dots;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private float interval = 0.35f;
        [SerializeField] private bool playOnEnable = false;
        [SerializeField] private bool useUnscaledTime = true;

        private int activeIndex;
        private float timer;
        private bool isPlaying;

        private void OnEnable()
        {
            ResetDots();

            if (playOnEnable)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!isPlaying || dots == null || dots.Length == 0)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer += deltaTime;

            if (timer < Mathf.Max(0.01f, interval))
            {
                return;
            }

            timer = 0f;
            activeIndex = (activeIndex + 1) % dots.Length;
            ApplyDotColors();
        }

        public void Play()
        {
            isPlaying = true;
            timer = 0f;
            ApplyDotColors();
        }

        public void Stop()
        {
            isPlaying = false;
        }

        public void ResetDots()
        {
            activeIndex = 0;
            timer = 0f;
            ApplyDotColors();
        }

        private void ApplyDotColors()
        {
            if (dots == null)
            {
                return;
            }

            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] == null)
                {
                    continue;
                }

                dots[i].color = i == activeIndex ? activeColor : inactiveColor;
            }
        }
    }
}
