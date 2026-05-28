using UnityEngine;

namespace JingHongLu.Feedback
{
    [System.Serializable]
    public sealed class FeedbackCue
    {
        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private AudioClip audioClip;
        [Tooltip("If this list contains valid clips, one will be randomly selected. Otherwise the single AudioClip is used.")]
        [SerializeField] private AudioClip[] randomAudioClips;
        [SerializeField] private string animatorTrigger = string.Empty;
        [SerializeField] private FeedbackSpawnPoint spawnPoint =
            FeedbackSpawnPoint.CasterCenter;
        [SerializeField] private Vector2 localOffset;
        [SerializeField] private bool rotateToDirection;
        [SerializeField] private bool parentToCaster;
        [SerializeField] private float destroyDelay = 2f;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;

        [System.NonSerialized] private AudioClip lastRandomAudioClip;

        public GameObject VfxPrefab => vfxPrefab;
        public AudioClip AudioClip => audioClip;
        public string AnimatorTrigger => animatorTrigger;
        public FeedbackSpawnPoint SpawnPoint => spawnPoint;
        public Vector2 LocalOffset => localOffset;
        public bool RotateToDirection => rotateToDirection;
        public bool ParentToCaster => parentToCaster;
        public float DestroyDelay => destroyDelay;
        public float Volume => volume;
        public float Pitch => pitch;

        public AudioClip GetAudioClip()
        {
            AudioClip selectedRandomClip = GetRandomAudioClipWithoutImmediateRepeat();
            return selectedRandomClip != null ? selectedRandomClip : audioClip;
        }

        private AudioClip GetRandomAudioClipWithoutImmediateRepeat()
        {
            if (randomAudioClips == null || randomAudioClips.Length == 0)
            {
                return null;
            }

            int validCount = 0;
            AudioClip onlyValidClip = null;

            for (int i = 0; i < randomAudioClips.Length; i++)
            {
                AudioClip clip = randomAudioClips[i];

                if (clip == null)
                {
                    continue;
                }

                validCount++;
                onlyValidClip = clip;
            }

            if (validCount == 0)
            {
                return null;
            }

            if (validCount == 1)
            {
                lastRandomAudioClip = onlyValidClip;
                return onlyValidClip;
            }

            for (int attempt = 0; attempt < 8; attempt++)
            {
                AudioClip candidate =
                    randomAudioClips[Random.Range(0, randomAudioClips.Length)];

                if (candidate != null && candidate != lastRandomAudioClip)
                {
                    lastRandomAudioClip = candidate;
                    return candidate;
                }
            }

            for (int i = 0; i < randomAudioClips.Length; i++)
            {
                AudioClip candidate = randomAudioClips[i];

                if (candidate != null && candidate != lastRandomAudioClip)
                {
                    lastRandomAudioClip = candidate;
                    return candidate;
                }
            }

            return null;
        }
    }
}
