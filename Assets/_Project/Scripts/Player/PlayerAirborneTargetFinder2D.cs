using JingHongLu.Combat;
using UnityEngine;

namespace JingHongLu.Player
{
    public sealed class PlayerAirborneTargetFinder2D : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private float defaultSearchRadius = 5f;

        public AirborneTarget2D FindNearestAirborneTarget(
            Vector2 origin,
            float radius,
            LayerMask mask)
        {
            float searchRadius = radius > 0f ? radius : defaultSearchRadius;
            LayerMask searchMask = mask.value != 0 ? mask : targetLayerMask;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                origin,
                searchRadius,
                searchMask);

            AirborneTarget2D nearestTarget = null;
            float nearestSqrDistance = float.PositiveInfinity;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];

                if (hit == null)
                {
                    continue;
                }

                AirborneTarget2D airborneTarget =
                    hit.GetComponentInParent<AirborneTarget2D>();

                if (airborneTarget == null || !airborneTarget.IsAirborne)
                {
                    continue;
                }

                float sqrDistance =
                    ((Vector2)airborneTarget.transform.position - origin).sqrMagnitude;

                if (sqrDistance >= nearestSqrDistance)
                {
                    continue;
                }

                nearestSqrDistance = sqrDistance;
                nearestTarget = airborneTarget;
            }

            return nearestTarget;
        }
    }
}
