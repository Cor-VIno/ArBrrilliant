using UnityEngine;

namespace JingHongLu.Core
{
    public sealed class TimeScaleController : MonoBehaviour
    {
        [SerializeField] private bool resetOnDisable = true;

        private float defaultFixedDeltaTime;
        private int slowMotionRequestCount;

        private void Awake()
        {
            defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnDisable()
        {
            if (resetOnDisable)
            {
                ForceReset();
            }
        }

        public void EnterSlowMotion(float scale)
        {
            float safeScale = Mathf.Clamp(scale, 0.01f, 1f);
            slowMotionRequestCount++;

            Time.timeScale = safeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * safeScale;
        }

        public void EnterTimeStop()
        {
            slowMotionRequestCount++;

            Time.timeScale = 0f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }

        public void ExitSlowMotion()
        {
            slowMotionRequestCount = Mathf.Max(0, slowMotionRequestCount - 1);

            if (slowMotionRequestCount > 0)
            {
                return;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }

        public void ForceReset()
        {
            slowMotionRequestCount = 0;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }
}
