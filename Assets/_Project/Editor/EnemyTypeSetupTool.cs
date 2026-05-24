using System.IO;
using JingHongLu.Combat;
using JingHongLu.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JingHongLu.EditorTools
{
    public static class EnemyTypeSetupTool
    {
        private const string EnemyDataFolder = "Assets/_Project/ScriptableObjects/Enemies";
        private const string EnemyPrefabFolder = "Assets/_Project/Prefabs/Enemies";
        private const string CombatScenePath = "Assets/_Project/Scenes/02_Combat_Test.unity";

        private const string BasicMeleeDataPath =
            EnemyDataFolder + "/Enemy_BasicMelee.asset";
        private const string ShieldDataPath =
            EnemyDataFolder + "/Enemy_ShieldGuard.asset";
        private const string RangedDataPath =
            EnemyDataFolder + "/Enemy_RangedHarpoon.asset";
        private const string HarpoonDataPath =
            EnemyDataFolder + "/Projectiles/Projectile_WaterBandit_Harpoon.asset";

        private const string DummyPrefabPath =
            EnemyPrefabFolder + "/Dummy_Enemy.prefab";
        private const string ShieldPrefabPath =
            EnemyPrefabFolder + "/Enemy_ShieldDummy.prefab";
        private const string RangedPrefabPath =
            EnemyPrefabFolder + "/Enemy_RangedDummy.prefab";

        [InitializeOnLoadMethod]
        private static void SyncEnemyTypesAfterReload()
        {
            EditorApplication.delayCall += SyncIfProjectIsReady;
        }

        [MenuItem("JingHongLu/Enemies/Sync Phase 10A Enemy Types")]
        public static void SyncPhase10AEnemyTypes()
        {
            EnsureFolder(EnemyDataFolder);
            EnsureFolder(EnemyPrefabFolder);

            ProjectileData harpoonData =
                AssetDatabase.LoadAssetAtPath<ProjectileData>(HarpoonDataPath);

            EnemyData basicMeleeData = LoadOrCreateEnemyData(BasicMeleeDataPath);
            ConfigureEnemyData(
                basicMeleeData,
                aggroRange: 7f,
                loseTargetRange: 11f,
                moveSpeed: 2f,
                stopDistance: 1.1f,
                attackDamage: 8,
                attackRange: 1.4f,
                attackCooldown: 1.25f,
                attackWindup: 0.25f,
                attackRecovery: 0.35f,
                canUseRangedAttack: false,
                rangedAttackDamage: 0,
                rangedAttackMinDistance: 3f,
                rangedAttackMaxDistance: 8f,
                rangedAttackCooldown: 2.2f,
                rangedAttackWindup: 0.35f,
                harpoonData: null,
                enableCombatSpacing: false,
                preferredMinDistance: 1.4f,
                preferredMaxDistance: 3f,
                postAttackIdleTime: 0.2f,
                enableBackstep: false,
                backstepSpeed: 3f,
                backstepDuration: 0.2f,
                backstepCooldown: 2f,
                backstepChanceAfterAttack: 0f,
                reactToDangerousPlayerSkill: false,
                dangerousSkillReactionChance: 0f,
                dangerousSkillReactionCooldown: 1.5f,
                hitboxSize: new Vector2(1.2f, 0.8f),
                hitboxOffset: new Vector2(0.8f, 0f));

            EnemyData shieldData = LoadOrCreateEnemyData(ShieldDataPath);
            ConfigureEnemyData(
                shieldData,
                aggroRange: 7f,
                loseTargetRange: 11f,
                moveSpeed: 1.35f,
                stopDistance: 1.1f,
                attackDamage: 14,
                attackRange: 1.45f,
                attackCooldown: 1.45f,
                attackWindup: 0.35f,
                attackRecovery: 0.45f,
                canUseRangedAttack: false,
                rangedAttackDamage: 0,
                rangedAttackMinDistance: 3f,
                rangedAttackMaxDistance: 8f,
                rangedAttackCooldown: 2.4f,
                rangedAttackWindup: 0.4f,
                harpoonData: null,
                enableCombatSpacing: false,
                preferredMinDistance: 1.2f,
                preferredMaxDistance: 2.5f,
                postAttackIdleTime: 0.25f,
                enableBackstep: false,
                backstepSpeed: 2.5f,
                backstepDuration: 0.2f,
                backstepCooldown: 3f,
                backstepChanceAfterAttack: 0f,
                reactToDangerousPlayerSkill: false,
                dangerousSkillReactionChance: 0f,
                dangerousSkillReactionCooldown: 1.5f,
                hitboxSize: new Vector2(1.3f, 0.9f),
                hitboxOffset: new Vector2(0.85f, 0f));

            EnemyData rangedData = LoadOrCreateEnemyData(RangedDataPath);
            ConfigureEnemyData(
                rangedData,
                aggroRange: 12f,
                loseTargetRange: 15f,
                moveSpeed: 1.2f,
                stopDistance: 4.2f,
                attackDamage: 4,
                attackRange: 1.1f,
                attackCooldown: 1.8f,
                attackWindup: 0.25f,
                attackRecovery: 0.35f,
                canUseRangedAttack: true,
                rangedAttackDamage: 10,
                rangedAttackMinDistance: 2.5f,
                rangedAttackMaxDistance: 10f,
                rangedAttackCooldown: 2.1f,
                rangedAttackWindup: 0.35f,
                harpoonData: harpoonData,
                enableCombatSpacing: true,
                preferredMinDistance: 4f,
                preferredMaxDistance: 7f,
                postAttackIdleTime: 0.3f,
                enableBackstep: true,
                backstepSpeed: 2.5f,
                backstepDuration: 0.2f,
                backstepCooldown: 2.5f,
                backstepChanceAfterAttack: 0.2f,
                reactToDangerousPlayerSkill: true,
                dangerousSkillReactionChance: 0.25f,
                dangerousSkillReactionCooldown: 1.5f,
                hitboxSize: new Vector2(0.7f, 0.7f),
                hitboxOffset: new Vector2(0.7f, 0f));

            ConfigureEnemyPrefab(
                DummyPrefabPath,
                basicMeleeData,
                health: 60f,
                shield: null);

            ConfigureEnemyPrefab(
                ShieldPrefabPath,
                shieldData,
                health: 120f,
                shield: new ShieldSettings(30f));

            EnsureRangedPrefab();
            ConfigureEnemyPrefab(
                RangedPrefabPath,
                rangedData,
                health: 45f,
                shield: null);

            SyncCombatScene(
                basicMeleeData,
                shieldData,
                rangedData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase10A] Enemy type configuration synced.");
        }

        private static void SyncIfProjectIsReady()
        {
            if (!AssetDatabase.IsValidFolder(EnemyDataFolder) ||
                !File.Exists(DummyPrefabPath) ||
                !File.Exists(ShieldPrefabPath))
            {
                return;
            }

            if (File.Exists(RangedPrefabPath) &&
                File.Exists(ShieldDataPath) &&
                File.Exists(RangedDataPath))
            {
                return;
            }

            SyncPhase10AEnemyTypes();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folder = Path.GetFileName(folderPath);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folder))
            {
                return;
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }

        private static EnemyData LoadOrCreateEnemyData(string path)
        {
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (data != null)
            {
                return data;
            }

            data = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static void ConfigureEnemyData(
            EnemyData data,
            float aggroRange,
            float loseTargetRange,
            float moveSpeed,
            float stopDistance,
            int attackDamage,
            float attackRange,
            float attackCooldown,
            float attackWindup,
            float attackRecovery,
            bool canUseRangedAttack,
            int rangedAttackDamage,
            float rangedAttackMinDistance,
            float rangedAttackMaxDistance,
            float rangedAttackCooldown,
            float rangedAttackWindup,
            ProjectileData harpoonData,
            bool enableCombatSpacing,
            float preferredMinDistance,
            float preferredMaxDistance,
            float postAttackIdleTime,
            bool enableBackstep,
            float backstepSpeed,
            float backstepDuration,
            float backstepCooldown,
            float backstepChanceAfterAttack,
            bool reactToDangerousPlayerSkill,
            float dangerousSkillReactionChance,
            float dangerousSkillReactionCooldown,
            Vector2 hitboxSize,
            Vector2 hitboxOffset)
        {
            SerializedObject serializedObject = new SerializedObject(data);
            Set(serializedObject, "aggroRange", aggroRange);
            Set(serializedObject, "loseTargetRange", loseTargetRange);
            Set(serializedObject, "moveSpeed", moveSpeed);
            Set(serializedObject, "stopDistance", stopDistance);
            Set(serializedObject, "attackDamage", attackDamage);
            Set(serializedObject, "attackRange", attackRange);
            Set(serializedObject, "attackCooldown", attackCooldown);
            Set(serializedObject, "attackWindup", attackWindup);
            Set(serializedObject, "attackRecovery", attackRecovery);
            Set(serializedObject, "canUseRangedAttack", canUseRangedAttack);
            Set(serializedObject, "rangedAttackDamage", rangedAttackDamage);
            Set(serializedObject, "rangedAttackMinDistance", rangedAttackMinDistance);
            Set(serializedObject, "rangedAttackMaxDistance", rangedAttackMaxDistance);
            Set(serializedObject, "rangedAttackCooldown", rangedAttackCooldown);
            Set(serializedObject, "rangedAttackWindup", rangedAttackWindup);
            Set(serializedObject, "harpoonProjectileData", harpoonData);
            Set(serializedObject, "enableCombatSpacing", enableCombatSpacing);
            Set(serializedObject, "preferredMinDistance", preferredMinDistance);
            Set(serializedObject, "preferredMaxDistance", preferredMaxDistance);
            Set(serializedObject, "postAttackIdleTime", postAttackIdleTime);
            Set(serializedObject, "enableBackstep", enableBackstep);
            Set(serializedObject, "backstepSpeed", backstepSpeed);
            Set(serializedObject, "backstepDuration", backstepDuration);
            Set(serializedObject, "backstepCooldown", backstepCooldown);
            Set(serializedObject, "backstepChanceAfterAttack", backstepChanceAfterAttack);
            Set(serializedObject, "reactToDangerousPlayerSkill", reactToDangerousPlayerSkill);
            Set(serializedObject, "dangerousSkillReactionChance", dangerousSkillReactionChance);
            Set(serializedObject, "dangerousSkillReactionCooldown", dangerousSkillReactionCooldown);
            Set(serializedObject, "hitboxShape", (int)HitboxShape.Box);
            Set(serializedObject, "hitboxSize", hitboxSize);
            Set(serializedObject, "hitboxOffset", hitboxOffset);
            Set(serializedObject, "hitboxDuration", 0.12f);
            Set(serializedObject, "targetLayerMask", 1 << 10);
            Set(serializedObject, "gizmoColor", Color.red);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        private static void EnsureRangedPrefab()
        {
            if (File.Exists(RangedPrefabPath))
            {
                return;
            }

            bool copied = AssetDatabase.CopyAsset(DummyPrefabPath, RangedPrefabPath);

            if (!copied)
            {
                Debug.LogError($"[Phase10A] Failed to create {RangedPrefabPath}");
                return;
            }

            AssetDatabase.ImportAsset(RangedPrefabPath);
        }

        private static void ConfigureEnemyPrefab(
            string prefabPath,
            EnemyData data,
            float health,
            ShieldSettings? shield)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"[Phase10A] Prefab not found: {prefabPath}");
                return;
            }

            prefab.name = Path.GetFileNameWithoutExtension(prefabPath);

            Health healthComponent = prefab.GetComponent<Health>();

            if (healthComponent != null)
            {
                SerializedObject healthObject = new SerializedObject(healthComponent);
                Set(healthObject, "maxHealth", health);
                Set(healthObject, "currentHealth", health);
                healthObject.ApplyModifiedPropertiesWithoutUndo();
            }

            EnemyBrain2D brain = prefab.GetComponent<EnemyBrain2D>();

            if (brain != null)
            {
                SerializedObject brainObject = new SerializedObject(brain);
                Set(brainObject, "data", data);
                brainObject.ApplyModifiedPropertiesWithoutUndo();
            }

            ShieldComponent shieldComponent = prefab.GetComponent<ShieldComponent>();

            if (shield.HasValue)
            {
                if (shieldComponent == null)
                {
                    shieldComponent = prefab.AddComponent<ShieldComponent>();
                }

                SerializedObject shieldObject = new SerializedObject(shieldComponent);
                Set(shieldObject, "maxShield", shield.Value.MaxShield);
                Set(shieldObject, "currentShield", shield.Value.MaxShield);
                Set(shieldObject, "startFull", true);
                Set(shieldObject, "blockHealthDamageWhileShielded", true);
                Set(shieldObject, "healthDamageMultiplierWhileShielded", 0f);
                Set(shieldObject, "logShield", true);
                shieldObject.ApplyModifiedPropertiesWithoutUndo();
            }
            else if (shieldComponent != null)
            {
                Object.DestroyImmediate(shieldComponent, true);
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        private static void SyncCombatScene(
            EnemyData basicMeleeData,
            EnemyData shieldData,
            EnemyData rangedData)
        {
            Scene scene = EditorSceneManager.OpenScene(CombatScenePath, OpenSceneMode.Single);

            GameObject dummy = EnsureSceneEnemy(
                DummyPrefabPath,
                "Dummy_Enemy",
                new Vector3(3f, 0f, 0f));
            ConfigureSceneEnemy(dummy, basicMeleeData, 60f, null);

            GameObject shield = EnsureSceneEnemy(
                ShieldPrefabPath,
                "Enemy_ShieldDummy",
                new Vector3(6f, 0f, 0f));
            ConfigureSceneEnemy(shield, shieldData, 120f, new ShieldSettings(30f));

            GameObject ranged = EnsureSceneEnemy(
                RangedPrefabPath,
                "Enemy_RangedDummy",
                new Vector3(9f, 0f, 0f));
            ConfigureSceneEnemy(ranged, rangedData, 45f, null);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject EnsureSceneEnemy(
            string prefabPath,
            string objectName,
            Vector3 position)
        {
            GameObject existing = GameObject.Find(objectName);

            if (existing == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    return null;
                }

                existing = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                existing.name = objectName;
            }

            existing.transform.position = position;
            existing.SetActive(true);
            return existing;
        }

        private static void ConfigureSceneEnemy(
            GameObject enemy,
            EnemyData data,
            float health,
            ShieldSettings? shield)
        {
            if (enemy == null)
            {
                return;
            }

            EnemyBrain2D brain = enemy.GetComponent<EnemyBrain2D>();

            if (brain != null)
            {
                SerializedObject brainObject = new SerializedObject(brain);
                Set(brainObject, "data", data);
                brainObject.ApplyModifiedPropertiesWithoutUndo();
            }

            Health healthComponent = enemy.GetComponent<Health>();

            if (healthComponent != null)
            {
                SerializedObject healthObject = new SerializedObject(healthComponent);
                Set(healthObject, "maxHealth", health);
                Set(healthObject, "currentHealth", health);
                healthObject.ApplyModifiedPropertiesWithoutUndo();
            }

            ShieldComponent shieldComponent = enemy.GetComponent<ShieldComponent>();

            if (shield.HasValue)
            {
                if (shieldComponent == null)
                {
                    shieldComponent = enemy.AddComponent<ShieldComponent>();
                }

                SerializedObject shieldObject = new SerializedObject(shieldComponent);
                Set(shieldObject, "maxShield", shield.Value.MaxShield);
                Set(shieldObject, "currentShield", shield.Value.MaxShield);
                Set(shieldObject, "startFull", true);
                Set(shieldObject, "blockHealthDamageWhileShielded", true);
                Set(shieldObject, "healthDamageMultiplierWhileShielded", 0f);
                Set(shieldObject, "logShield", true);
                shieldObject.ApplyModifiedPropertiesWithoutUndo();
            }
            else if (shieldComponent != null)
            {
                Object.DestroyImmediate(shieldComponent);
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, Vector2 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector2Value = value;
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void Set(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private readonly struct ShieldSettings
        {
            public ShieldSettings(float maxShield)
            {
                MaxShield = maxShield;
            }

            public float MaxShield { get; }
        }
    }
}
