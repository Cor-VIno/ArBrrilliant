using UnityEngine;

namespace JingHongLu.Combat
{
    public readonly struct PerfectDodgeEventData
    {
        public PerfectDodgeEventData(
            GameObject player,
            GameObject enemy,
            Hitbox2D dodgedHitbox,
            Vector2 dodgeDirection,
            Vector2 contactPoint,
            float time)
        {
            Player = player;
            Enemy = enemy;
            DodgedHitbox = dodgedHitbox;
            DodgeDirection = dodgeDirection;
            ContactPoint = contactPoint;
            Time = time;
        }

        public GameObject Player { get; }
        public GameObject Enemy { get; }
        public Hitbox2D DodgedHitbox { get; }
        public Vector2 DodgeDirection { get; }
        public Vector2 ContactPoint { get; }
        public float Time { get; }
    }
}
