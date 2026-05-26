#if UNITY_EDITOR
using JingHongLu.Combat;
using JingHongLu.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.EditorTools
{
    public static class EnemyHealthBarSetupTool
    {
        private const string DummyEnemyPrefabPath =
            "Assets/_Project/Prefabs/Enemies/Dummy_Enemy.prefab";
        private const string ShieldEnemyPrefabPath =
            "Assets/_Project/Prefabs/Enemies/Enemy_ShieldDummy.prefab";
        private const string RangedEnemyPrefabPath =
            "Assets/_Project/Prefabs/Enemies/Enemy_RangedDummy.prefab";
        private const string FontPath =
            "Assets/_Project/Art/Fonts/simkai SDF.asset";

        [MenuItem("JingHongLu/Enemies/Setup Enemy Health Bars")]
        public static void SetupEnemyHealthBars()
        {
            SetupHealthBar(DummyEnemyPrefabPath, new Vector3(0f, 1.45f, 0f));
            SetupHealthBar(ShieldEnemyPrefabPath, new Vector3(0f, 1.55f, 0f));
            SetupHealthBar(RangedEnemyPrefabPath, new Vector3(0f, 1.45f, 0f));
            AssetDatabase.SaveAssets();
            Debug.Log("[HealthUI] Enemy health bars setup complete.");
        }

        private static void SetupHealthBar(string prefabPath, Vector3 worldOffset)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Enemy prefab not found: {prefabPath}");
                return;
            }

            EnsureHealthBar(prefab, worldOffset);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        private static void EnsureHealthBar(GameObject enemy, Vector3 worldOffset)
        {
            Transform oldRoot = enemy.transform.Find("HealthBarRoot");
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot.gameObject);
            }

            Health health = enemy.GetComponent<Health>();
            GameObject root = CreateRectObject(
                "HealthBarRoot",
                enemy.transform,
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(EnemyHealthBarView));
            root.layer = enemy.layer;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.localPosition = worldOffset;
            rootRect.localRotation = Quaternion.identity;
            rootRect.localScale = Vector3.one * 0.01f;
            rootRect.sizeDelta = new Vector2(180f, 38f);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 21;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject background = CreateRectObject(
                "Background",
                root.transform,
                typeof(Image));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            Stretch(backgroundRect, Vector2.zero, Vector2.zero);
            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = new Color(0.12f, 0.06f, 0.06f, 0.86f);
            backgroundImage.raycastTarget = false;

            GameObject fill = CreateRectObject(
                "Fill",
                background.transform,
                typeof(Image));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect, new Vector2(6f, 6f), new Vector2(-6f, -16f));
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.78f, 0.18f, 0.18f, 0.96f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
            fillImage.raycastTarget = false;

            GameObject valueText = CreateRectObject(
                "ValueText",
                root.transform,
                typeof(TextMeshProUGUI));
            RectTransform valueTextRect = valueText.GetComponent<RectTransform>();
            Stretch(valueTextRect, new Vector2(0f, 0f), new Vector2(0f, -18f));
            TextMeshProUGUI text = valueText.GetComponent<TextMeshProUGUI>();
            text.text = health != null
                ? $"{Mathf.CeilToInt(health.CurrentHealth)} / {Mathf.CeilToInt(health.MaxHealth)}"
                : "0 / 0";
            text.fontSize = 13f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font != null)
            {
                text.font = font;
            }

            EnemyHealthBarView view = root.GetComponent<EnemyHealthBarView>();
            SerializedObject viewObject = new SerializedObject(view);
            Set(viewObject, "health", health);
            Set(viewObject, "followTarget", enemy.transform);
            Set(viewObject, "worldOffset", worldOffset);
            Set(viewObject, "root", root);
            Set(viewObject, "fillImage", fillImage);
            Set(viewObject, "valueText", text);
            Set(viewObject, "hideWhenDead", true);
            Set(viewObject, "showValueText", true);
            Set(viewObject, "keepReadableWhenParentFlipped", true);
            Set(viewObject, "faceCamera", true);
            Set(viewObject, "baseLocalScale", Vector3.one * 0.01f);
            viewObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateRectObject(
            string name,
            Transform parent,
            params System.Type[] components)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            foreach (System.Type component in components)
            {
                gameObject.AddComponent(component);
            }

            return gameObject;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static void Set(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void Set(
            SerializedObject serializedObject,
            string propertyName,
            Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void Set(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }
    }
}
#endif
