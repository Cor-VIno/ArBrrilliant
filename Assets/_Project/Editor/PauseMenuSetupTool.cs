#if UNITY_EDITOR
using JingHongLu.GameFlow;
using JingHongLu.Input;
using JingHongLu.Player;
using JingHongLu.SwordArts;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JingHongLu.EditorTools
{
    public static class PauseMenuSetupTool
    {
        private const string CombatScenePath =
            "Assets/_Project/Scenes/02_Combat_Test.unity";
        private const string FontPath =
            "Assets/_Project/Art/Fonts/simkai SDF.asset";

        [MenuItem("JingHongLu/Game Flow/Setup Pause Menu")]
        public static void SetupPauseMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(CombatScenePath);
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("[Pause] Canvas not found in 02_Combat_Test.");
                return;
            }

            EnsureGraphicRaycaster(canvas);
            DestroyIfExists("PauseMenuRoot");
            DestroyIfExists("GamePauseController");

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            GameObject root = CreatePauseMenuRoot(canvas.transform);
            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            GamePauseController controller = CreateController(root, canvasGroup);

            CreateDimBackground(root.transform);
            GameObject panel = CreatePausePanel(root.transform);
            CreateText(
                "TitleText",
                panel.transform,
                "\u6682\u505C",
                font,
                60f,
                new Vector2(0f, 210f),
                new Vector2(460f, 90f));

            GameObject buttonRoot = CreateButtonRoot(panel.transform);
            CreateButton(
                "ResumeButton",
                buttonRoot.transform,
                "\u7EE7\u7EED",
                font,
                controller.Resume);
            CreateButton(
                "RestartButton",
                buttonRoot.transform,
                "\u91CD\u65B0\u5F00\u59CB",
                font,
                controller.RestartScene);
            CreateButton(
                "ReturnToBootButton",
                buttonRoot.transform,
                "\u8FD4\u56DE\u6807\u9898",
                font,
                controller.ReturnToBoot);
            CreateButton(
                "QuitButton",
                buttonRoot.transform,
                "\u9000\u51FA",
                font,
                controller.QuitGame);

            SetCanvasGroupVisible(canvasGroup, false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Pause] Pause menu setup complete.");
        }

        private static void DestroyIfExists(string objectName)
        {
            GameObject[] objects = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include);

            foreach (GameObject existing in objects)
            {
                if (existing.name == objectName)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static void EnsureGraphicRaycaster(Canvas canvas)
        {
            if (!canvas.TryGetComponent(out GraphicRaycaster _))
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static GameObject CreatePauseMenuRoot(Transform canvasTransform)
        {
            GameObject root = CreateRectObject(
                "PauseMenuRoot",
                canvasTransform,
                typeof(CanvasGroup));
            RectTransform rect = root.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.zero);
            root.SetActive(true);
            return root;
        }

        private static void CreateDimBackground(Transform root)
        {
            GameObject dim = CreateRectObject(
                "DimBackground",
                root,
                typeof(Image));
            RectTransform rect = dim.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.zero);

            Image image = dim.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.52f);
            image.raycastTarget = true;
        }

        private static GameObject CreatePausePanel(Transform root)
        {
            GameObject panel = CreateRectObject(
                "PausePanel",
                root,
                typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(520f, 600f);

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.04f, 0.06f, 0.1f, 0.9f);
            image.raycastTarget = true;
            return panel;
        }

        private static GameObject CreateButtonRoot(Transform panel)
        {
            GameObject root = CreateRectObject(
                "ButtonRoot",
                panel,
                typeof(VerticalLayoutGroup));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -70f);
            rect.sizeDelta = new Vector2(380f, 360f);

            VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return root;
        }

        private static GamePauseController CreateController(
            GameObject pauseRoot,
            CanvasGroup canvasGroup)
        {
            GameObject controllerObject = new GameObject("GamePauseController");
            GamePauseController controller =
                controllerObject.AddComponent<GamePauseController>();

            SerializedObject serializedObject = new SerializedObject(controller);
            Set(serializedObject, "pauseMenuRoot", pauseRoot);
            Set(serializedObject, "pauseCanvasGroup", canvasGroup);
            Set(serializedObject, "inputReader", Object.FindAnyObjectByType<PlayerInputReader>());
            Set(
                serializedObject,
                "controlLockController",
                Object.FindAnyObjectByType<PlayerControlLockController>());
            Set(
                serializedObject,
                "insightSelection",
                Object.FindAnyObjectByType<InsightSelectionController>());
            Set(serializedObject, "bootSceneName", "00_Boot");
            Set(serializedObject, "pauseOnEscape", true);
            Set(serializedObject, "logPause", false);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return controller;
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

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string text,
            TMP_FontAsset font,
            float fontSize,
            Vector2 position,
            Vector2 size)
        {
            GameObject textObject = CreateRectObject(
                name,
                parent,
                typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            if (font != null)
            {
                label.font = font;
            }

            return label;
        }

        private static void CreateButton(
            string name,
            Transform parent,
            string text,
            TMP_FontAsset font,
            UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonObject = CreateRectObject(
                name,
                parent,
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 72f);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 360f;
            layout.preferredHeight = 72f;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.23f, 0.32f, 0.96f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = true;
            UnityEventTools.AddPersistentListener(button.onClick, callback);

            CreateText(
                "Label",
                buttonObject.transform,
                text,
                font,
                38f,
                Vector2.zero,
                new Vector2(340f, 64f));
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

        private static void SetCanvasGroupVisible(CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
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
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
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
