using UnityEngine;

namespace JingHongLu.Feedback
{
    [System.Serializable]
    public sealed class FeedbackCue
    {
        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private string animatorTrigger = string.Empty;
        [SerializeField] private FeedbackSpawnPoint spawnPoint =
            FeedbackSpawnPoint.CasterCenter;
        [SerializeField] private Vector2 localOffset;
        [SerializeField] private bool rotateToDirection;
        [SerializeField] private bool parentToCaster;
        [SerializeField] private float destroyDelay = 2f;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;

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
    }
}
