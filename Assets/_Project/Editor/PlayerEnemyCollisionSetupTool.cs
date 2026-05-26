using UnityEditor;
using UnityEngine;

namespace JingHongLu.EditorTools
{
    public static class PlayerEnemyCollisionSetupTool
    {
        [MenuItem("JingHongLu/Combat/Setup Player Enemy Collision")]
        public static void SetupPlayerEnemyCollision()
        {
            Debug.Log(
                "[CollisionSetup] Player/enemy body collision is configured at runtime by PlayerEnemyCollisionController2D.");
        }
    }
}
