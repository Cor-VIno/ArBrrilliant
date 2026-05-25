#if UNITY_EDITOR
using JingHongLu.Combat;
using JingHongLu.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JingHongLu.EditorTools
{
    public static class EnemyShieldBarSetupTool
    {
        private const string ShieldEnemyPrefabPath =
            "Assets/_Project/Prefabs/Enemies/Enemy_ShieldDummy.prefab";
        private const string FontPath =
            "Assets/_Project/Art/Fonts/simkai SDF.asset";

        [MenuItem("JingHongLu/Enemies/Setup Shield Bar UI")]
        public static void SetupShieldBar()
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(
                ShieldEnemyPrefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Shield enemy prefab not found: {ShieldEnemyPrefabPath}");
                return;
            }

            EnsureShieldBar(prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab, ShieldEnemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[ShieldUI] Enemy_ShieldDummy shield bar setup complete.");
        }

        private static void EnsureShieldBar(GameObject enemy)
        {
            Transform oldRoot = enemy.transform.Find("ShieldBarRoot");
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot.gameObject);
            }

            ShieldComponent shield = enemy.GetComponent<ShieldComponent>();
            GameObject root = CreateRectObject(
                "ShieldBarRoot",
                enemy.transform,
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(EnemyShieldBarView));
            root.layer = enemy.layer;

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.localPosition = new Vector3(0f, 1.2f, 0f);
            rootRect.localRotation = Quaternion.identity;
            rootRect.localScale = Vector3.one * 0.01f;
            rootRect.sizeDelta = new Vector2(180f, 42f);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

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
            backgroundImage.color = new Color(0.08f, 0.1f, 0.16f, 0.85f);
            backgroundImage.raycastTarget = false;

            GameObject fill = CreateRectObject(
                "Fill",
                background.transform,
                typeof(Image));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect, new Vector2(6f, 6f), new Vector2(-6f, -16f));
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.25f, 0.62f, 1f, 0.95f);
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
            Stretch(valueTextRect, new Vector2(0f, 0f), new Vector2(0f, -20f));
            TextMeshProUGUI text = valueText.GetComponent<TextMeshProUGUI>();
            text.text = "30 / 30";
            text.fontSize = 14f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                FontPath);
            if (font != null)
            {
                text.font = font;
            }

            EnemyShieldBarView view = root.GetComponent<EnemyShieldBarView>();
            SerializedObject viewObject = new SerializedObject(view);
            Set(viewObject, "shield", shield);
            Set(viewObject, "followTarget", enemy.transform);
            Set(viewObject, "worldOffset", new Vector3(0f, 1.2f, 0f));
            Set(viewObject, "root", root);
            Set(viewObject, "fillImage", fillImage);
            Set(viewObject, "valueText", text);
            Set(viewObject, "hideWhenBroken", true);
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
