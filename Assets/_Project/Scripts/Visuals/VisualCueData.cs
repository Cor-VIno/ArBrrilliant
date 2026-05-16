using UnityEngine;

namespace JingHongLu.Visuals
{
    [System.Serializable]
    public sealed class VisualCueData
    {
        [SerializeField] private string animatorTrigger = string.Empty;
        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private VisualSpawnPointType spawnPoint =
            VisualSpawnPointType.CasterCenter;
        [SerializeField] private Vector2 localOffset;
        [SerializeField] private bool rotateToAimDirection;
        [SerializeField] private bool parentToSpawnPoint;
        [SerializeField] private float destroyDelay;

        public string AnimatorTrigger => animatorTrigger;
        public GameObject VfxPrefab => vfxPrefab;
        public VisualSpawnPointType SpawnPoint => spawnPoint;
        public Vector2 LocalOffset => localOffset;
        public bool RotateToAimDirection => rotateToAimDirection;
        public bool ParentToSpawnPoint => parentToSpawnPoint;
        public float DestroyDelay => destroyDelay;
    }
}
