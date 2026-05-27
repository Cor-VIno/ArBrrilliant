using UnityEngine;

namespace JingHongLu.Cameras
{
    [DefaultExecutionOrder(1000)]
    public sealed class CameraShakeController2D : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.12f;
        [SerializeField] private float defaultStrength = 0.08f;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private bool logShake;

        private float remainingTime;
        private float currentDuration;
        private float currentStrength;
        private Vector3 appliedOffset;

        public bool IsShaking => remainingTime > 0f;

        public void Shake()
        {
            Shake(defaultDuration, defaultStrength);
        }

        public void Shake(float duration, float strength)
        {
            if (duration <= 0f || strength <= 0f)
            {
                return;
            }

            RemoveAppliedOffset();

            currentDuration = duration;
            remainingTime = duration;
            currentStrength = strength;

            if (logShake)
            {
                Debug.Log($"[CameraShake] Started. Duration={duration}, Strength={strength}", this);
            }
        }

        public void StopShake()
        {
            bool wasShaking = IsShaking || appliedOffset != Vector3.zero;
            remainingTime = 0f;
            currentDuration = 0f;
            currentStrength = 0f;
            RemoveAppliedOffset();

            if (wasShaking && logShake)
            {
                Debug.Log("[CameraShake] Stopped.", this);
            }
        }

        private void Update()
        {
            RemoveAppliedOffset();
        }

        private void LateUpdate()
        {
            if (remainingTime <= 0f)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            remainingTime = Mathf.Max(0f, remainingTime - deltaTime);

            float normalized = currentDuration > 0f
                ? Mathf.Clamp01(remainingTime / currentDuration)
                : 0f;
            Vector2 randomOffset = Random.insideUnitCircle * (currentStrength * normalized);
            appliedOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            transform.position += appliedOffset;

            if (remainingTime <= 0f)
            {
                currentDuration = 0f;
                currentStrength = 0f;

                if (logShake)
                {
                    Debug.Log("[CameraShake] Ended.", this);
                }
            }
        }

        private void OnDisable()
        {
            StopShake();
        }

        private void OnDestroy()
        {
            RemoveAppliedOffset();
        }

        private void RemoveAppliedOffset()
        {
            if (appliedOffset == Vector3.zero)
            {
                return;
            }

            transform.position -= appliedOffset;
            appliedOffset = Vector3.zero;
        }
    }
}
