using System.Collections.Generic;
using JingHongLu.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JingHongLu.EditorTools
{
    public static class BootTitleUISetupTool
    {
        private const string BootScenePath = "Assets/_Project/Scenes/00_Boot.unity";

        private static readonly string[] SpritePaths =
        {
            "Assets/_Project/Art/UI/TitleScreen/Background/title_bg_pattern.png",
            "Assets/_Project/Art/UI/TitleScreen/Background/title_bg_base.png",
            "Assets/_Project/Art/UI/TitleScreen/Logo/title_logo.png",
            "Assets/_Project/Art/UI/TitleScreen/BottomBar/title_bottom_bar.png",
            "Assets/_Project/Art/UI/TitleScreen/TextSprites/title_version_v1_0_0.png",
            "Assets/_Project/Art/UI/TitleScreen/TextSprites/title_prompt_click_anywhere.png",
            "Assets/_Project/Art/UI/TitleScreen/Background/loading_bg_pattern.png",
            "Assets/_Project/Art/UI/TitleScreen/Background/loading_bg_base.png",
            "Assets/_Project/Art/UI/TitleScreen/Logo/loading_logo.png",
            "Assets/_Project/Art/UI/TitleScreen/BottomBar/loading_bottom_bar.png",
            "Assets/_Project/Art/UI/TitleScreen/TextSprites/loading_tip_placeholder.png",
            "Assets/_Project/Art/UI/TitleScreen/LoadingDots/loading_dot_01.png",
            "Assets/_Project/Art/UI/TitleScreen/LoadingDots/loading_dot_02.png",
            "Assets/_Project/Art/UI/TitleScreen/LoadingDots/loading_dot_03.png"
        };

        [MenuItem("JingHongLu/UI/Build Boot Title UI")]
        public static void BuildBootTitleUI()
        {
            ConfigureSpriteImportSettings();
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(BootScenePath);

            GameObject existingCanvas = GameObject.Find("Canvas");

            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
            }

            GameObject existingEventSystem = GameObject.Find("EventSystem");

            if (existingEventSystem != null)
            {
                Object.DestroyImmediate(existingEventSystem);
            }

            GameObject canvasObject = new GameObject(
                "Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject titleRoot = CreateRoot("TitleRoot", canvasObject.transform, true);
            GameObject loadingRoot = CreateRoot("LoadingRoot", canvasObject.transform, false);

            CreateFullScreenImage(
                "TitleBackgroundBase",
                titleRoot.transform,
                LoadSprite("Background/title_bg_base.png"),
                Image.Type.Simple,
                preserveAspect: false);
            CreateFullScreenImage(
                "TitleBackgroundPattern",
                titleRoot.transform,
                LoadSprite("Background/title_bg_pattern.png"),
                Image.Type.Tiled,
                preserveAspect: false);

            Image titleLogo = CreateAnchoredImage(
                "TitleLogo",
                titleRoot.transform,
                LoadSprite("Logo/title_logo.png"),
                new Vector2(0.5f, 0.56f),
                Vector2.zero,
                1f);

            Image titleLogoOverlay = CreateAnchoredImage(
                "TitleLogoOverlay",
                titleRoot.transform,
                LoadSprite("Logo/title_logo.png"),
                new Vector2(0.5f, 0.56f),
                Vector2.zero,
                1f);
            titleLogoOverlay.color = new Color(1f, 1f, 1f, 0.15f);
            titleLogoOverlay.gameObject.SetActive(false);

            Image titleBottomBar = CreateBottomStretchImage(
                "TitleBottomBar",
                titleRoot.transform,
                LoadSprite("BottomBar/title_bottom_bar.png"),
                0f);
            titleBottomBar.type = Image.Type.Sliced;

            CreateCornerImage(
                "TitleVersionImage",
                titleRoot.transform,
                LoadSprite("TextSprites/title_version_v1_0_0.png"),
                new Vector2(0f, 1f),
                new Vector2(72f, -48f),
                new Vector2(0f, 1f),
                1f);

            Image titlePrompt = CreateAnchoredImage(
                "TitlePromptImage",
                titleRoot.transform,
                LoadSprite("TextSprites/title_prompt_click_anywhere.png"),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 122f),
                1f);
            UIFadeGraphic clickPromptFade = titlePrompt.gameObject.AddComponent<UIFadeGraphic>();
            ConfigureFade(clickPromptFade, titlePrompt, 0.25f, 1f, 1.4f, true, true);

            CreateFullScreenImage(
                "LoadingBackgroundBase",
                loadingRoot.transform,
                LoadSprite("Background/loading_bg_base.png"),
                Image.Type.Simple,
                preserveAspect: false);
            CreateFullScreenImage(
                "LoadingBackgroundPattern",
                loadingRoot.transform,
                LoadSprite("Background/loading_bg_pattern.png"),
                Image.Type.Tiled,
                preserveAspect: false);

            CreateAnchoredImage(
                "LoadingLogo",
                loadingRoot.transform,
                LoadSprite("Logo/loading_logo.png"),
                new Vector2(0.5f, 0.56f),
                Vector2.zero,
                1f);

            Image loadingBottomBar = CreateBottomStretchImage(
                "LoadingBottomBar",
                loadingRoot.transform,
                LoadSprite("BottomBar/loading_bottom_bar.png"),
                0f);
            loadingBottomBar.type = Image.Type.Sliced;

            CreateAnchoredImage(
                "LoadingTipImage",
                loadingRoot.transform,
                LoadSprite("TextSprites/loading_tip_placeholder.png"),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 128f),
                1f);

            GameObject dotsRoot = CreateRectObject("LoadingDotsRoot", loadingRoot.transform);
            RectTransform dotsRootRect = dotsRoot.GetComponent<RectTransform>();
            dotsRootRect.anchorMin = new Vector2(1f, 0f);
            dotsRootRect.anchorMax = new Vector2(1f, 0f);
            dotsRootRect.pivot = new Vector2(1f, 0f);
            dotsRootRect.anchoredPosition = new Vector2(-118f, 76f);
            dotsRootRect.sizeDelta = new Vector2(120f, 32f);

            Image dot01 = CreateDot("Dot01", dotsRoot.transform, LoadSprite("LoadingDots/loading_dot_01.png"), -38f);
            Image dot02 = CreateDot("Dot02", dotsRoot.transform, LoadSprite("LoadingDots/loading_dot_02.png"), 0f);
            Image dot03 = CreateDot("Dot03", dotsRoot.transform, LoadSprite("LoadingDots/loading_dot_03.png"), 38f);
            UILoadingDotsAnimator dotsAnimator = dotsRoot.AddComponent<UILoadingDotsAnimator>();
            ConfigureDotsAnimator(dotsAnimator, new[] { dot01, dot02, dot03 });

            GameObject controllerObject = new GameObject("BootTitleUIController");
            controllerObject.transform.SetParent(canvasObject.transform, false);
            BootTitleUIController controller = controllerObject.AddComponent<BootTitleUIController>();
            UIFadeOverlay fadeOverlay = EnsureFadePanel(canvasObject.transform);
            ConfigureController(
                controller,
                titleRoot,
                loadingRoot,
                clickPromptFade,
                dotsAnimator,
                fadeOverlay);

            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Boot title UI setup complete.");
        }

        [MenuItem("JingHongLu/UI/Adjust Boot Title Layout")]
        public static void AdjustBootTitleLayout()
        {
            Scene scene = EditorSceneManager.OpenScene(BootScenePath);
            GameObject titleRoot = FindSceneObjectByName("TitleRoot");

            if (titleRoot == null)
            {
                Debug.LogError("TitleRoot was not found in 00_Boot.unity.");
                return;
            }

            RectTransform titleRootRect = titleRoot.GetComponent<RectTransform>();

            if (titleRootRect == null)
            {
                Debug.LogError("TitleRoot is missing RectTransform.");
                return;
            }

            GameObject backgroundBase = FindOrWarn("TitleBackgroundBase");
            GameObject backgroundPattern = FindOrWarn("TitleBackgroundPattern");
            GameObject titleLogo = FindOrWarn("TitleLogo");
            GameObject titleLogoOverlay = FindOrWarn("TitleLogoOverlay");
            GameObject titleBottomBar = FindOrWarn("TitleBottomBar");
            GameObject titleVersionImage = FindOrWarn("TitleVersionImage");
            GameObject titleContentArea = GameObject.Find("TitleContentArea");

            if (titleContentArea == null)
            {
                titleContentArea = CreateRectObject("TitleContentArea", titleRoot.transform);
            }

            GameObject titlePromptImage = GameObject.Find("TitlePromptImage");

            if (titlePromptImage == null && titleBottomBar != null)
            {
                titlePromptImage = CreateRectObject("TitlePromptImage", titleBottomBar.transform);
                Image promptImage = titlePromptImage.AddComponent<Image>();
                promptImage.sprite = LoadSprite("TextSprites/title_prompt_click_anywhere.png");
                promptImage.preserveAspect = true;
            }

            SetParentAndReset(titleContentArea, titleRoot.transform);

            if (backgroundBase != null)
            {
                SetParentAndReset(backgroundBase, titleRoot.transform);
                ConfigureFullStretch(backgroundBase.GetComponent<RectTransform>());
            }

            if (backgroundPattern != null)
            {
                SetParentAndReset(backgroundPattern, titleRoot.transform);
                ConfigureFullStretch(backgroundPattern.GetComponent<RectTransform>());
            }

            ConfigureTitleContentArea(titleContentArea.GetComponent<RectTransform>());

            if (titleLogo != null)
            {
                SetParentAndReset(titleLogo, titleContentArea.transform);
                ConfigureCenteredFixed(titleLogo.GetComponent<RectTransform>(), new Vector2(300f, 460f));
                SetPreserveAspect(titleLogo, true);
            }

            if (titleLogoOverlay != null)
            {
                SetParentAndReset(titleLogoOverlay, titleContentArea.transform);
                ConfigureCenteredFixed(titleLogoOverlay.GetComponent<RectTransform>(), new Vector2(300f, 460f));
                SetPreserveAspect(titleLogoOverlay, true);
            }

            if (titleBottomBar != null)
            {
                SetParentAndReset(titleBottomBar, titleRoot.transform);
                ConfigureBottomBar(titleBottomBar.GetComponent<RectTransform>());
                SetPreserveAspect(titleBottomBar, false);
            }

            if (titlePromptImage != null && titleBottomBar != null)
            {
                SetParentAndReset(titlePromptImage, titleBottomBar.transform);
                ConfigureCenteredFixed(titlePromptImage.GetComponent<RectTransform>(), new Vector2(360f, 40f));
                SetPreserveAspect(titlePromptImage, true);
            }

            if (titleVersionImage != null)
            {
                SetParentAndReset(titleVersionImage, titleRoot.transform);
                ConfigureVersionImage(titleVersionImage.GetComponent<RectTransform>());
                SetPreserveAspect(titleVersionImage, true);
            }

            SetSiblingOrder(
                titleRoot.transform,
                backgroundBase,
                backgroundPattern,
                titleContentArea,
                titleBottomBar,
                titleVersionImage);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Boot title layout adjusted.");
        }

        [MenuItem("JingHongLu/UI/Configure Boot Title Fades")]
        public static void ConfigureBootTitleFades()
        {
            Scene scene = EditorSceneManager.OpenScene(BootScenePath);

            GameObject titleLogo = GameObject.Find("TitleLogo");
            GameObject titleLogoOverlay = FindSceneObjectByName("TitleLogoOverlay");
            GameObject titlePromptImage = GameObject.Find("TitlePromptImage");
            GameObject titleBottomBar = GameObject.Find("TitleBottomBar");
            GameObject titleVersionImage = GameObject.Find("TitleVersionImage");
            GameObject controllerObject = GameObject.Find("BootTitleUIController");

            RemoveFadeIfPresent(titleLogo);
            RemoveFadeIfPresent(titleBottomBar);
            RemoveFadeIfPresent(titleVersionImage);

            RemoveFadeIfPresent(titleLogoOverlay);

            if (titleLogoOverlay != null)
            {
                titleLogoOverlay.SetActive(false);
            }

            UIFadeGraphic promptFade = ConfigureFadeOnObject(
                titlePromptImage,
                minAlpha: 0.25f,
                maxAlpha: 1f,
                duration: 1.2f,
                playOnEnable: true,
                loop: true);

            if (controllerObject != null)
            {
                BootTitleUIController controller =
                    controllerObject.GetComponent<BootTitleUIController>();

                if (controller != null)
                {
                    SerializedObject serializedObject = new SerializedObject(controller);
                    serializedObject.FindProperty("clickPromptFade").objectReferenceValue =
                        promptFade;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Boot title fade effects configured.");
        }

        [MenuItem("JingHongLu/UI/Build Boot Loading Flow")]
        public static void BuildBootLoadingFlow()
        {
            ConfigureSpriteImportSettings();
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(BootScenePath);
            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();

            GameObject titleRoot = GameObject.Find("TitleRoot");

            if (titleRoot == null)
            {
                Debug.LogError("TitleRoot was not found in 00_Boot.unity.");
                return;
            }

            titleRoot.SetActive(true);

            GameObject titleLogoOverlay = GameObject.Find("TitleLogoOverlay");

            if (titleLogoOverlay != null)
            {
                RemoveFadeIfPresent(titleLogoOverlay);
                titleLogoOverlay.SetActive(false);
            }

            GameObject loadingRoot = ResolveSingleLoadingRoot();

            if (loadingRoot == null)
            {
                loadingRoot = CreateRoot("LoadingRoot", canvas.transform, false);
            }
            else
            {
                SetParentAndReset(loadingRoot, canvas.transform);
                ConfigureFullStretch(loadingRoot.GetComponent<RectTransform>());
                loadingRoot.SetActive(false);
            }

            Image loadingBackgroundBase = EnsureChildImage(
                loadingRoot.transform,
                "LoadingBackgroundBase",
                LoadSprite("Background/loading_bg_base.png"));
            ConfigureFullStretch(loadingBackgroundBase.rectTransform);
            loadingBackgroundBase.preserveAspect = false;

            Image loadingBackgroundPattern = EnsureChildImage(
                loadingRoot.transform,
                "LoadingBackgroundPattern",
                LoadSprite("Background/loading_bg_pattern.png"));
            ConfigureFullStretch(loadingBackgroundPattern.rectTransform);
            loadingBackgroundPattern.preserveAspect = false;

            Image loadingLogo = EnsureChildImage(
                loadingRoot.transform,
                "LoadingLogo",
                LoadSprite("Logo/loading_logo.png"));
            ConfigureCenteredFixed(loadingLogo.rectTransform, new Vector2(520f, 760f));
            loadingLogo.rectTransform.anchoredPosition = new Vector2(0f, 40f);
            loadingLogo.preserveAspect = true;

            Image loadingBottomBar = EnsureChildImage(
                loadingRoot.transform,
                "LoadingBottomBar",
                LoadSprite("BottomBar/loading_bottom_bar.png"));
            ConfigureBottomBar(loadingBottomBar.rectTransform);
            loadingBottomBar.preserveAspect = false;

            Image loadingTipImage = EnsureChildImage(
                loadingBottomBar.transform,
                "LoadingTipImage",
                LoadSprite("TextSprites/loading_tip_placeholder.png"));
            ConfigureCenteredFixed(loadingTipImage.rectTransform, new Vector2(420f, 46f));
            loadingTipImage.preserveAspect = true;

            GameObject loadingDotsRoot = FindDirectChild(loadingRoot.transform, "LoadingDotsRoot");

            if (loadingDotsRoot == null)
            {
                loadingDotsRoot = CreateRectObject("LoadingDotsRoot", loadingRoot.transform);
            }

            SetParentAndReset(loadingDotsRoot, loadingRoot.transform);
            RectTransform dotsRootRect = loadingDotsRoot.GetComponent<RectTransform>();
            dotsRootRect.anchorMin = new Vector2(1f, 0f);
            dotsRootRect.anchorMax = new Vector2(1f, 0f);
            dotsRootRect.pivot = new Vector2(1f, 0f);
            dotsRootRect.anchoredPosition = new Vector2(-48f, 36f);
            dotsRootRect.sizeDelta = new Vector2(100f, 32f);
            dotsRootRect.localScale = Vector3.one;

            Image dot01 = EnsureChildImage(
                loadingDotsRoot.transform,
                "Dot01",
                LoadSprite("LoadingDots/loading_dot_01.png"));
            ConfigureDot(dot01.rectTransform, -34f);
            dot01.preserveAspect = true;

            Image dot02 = EnsureChildImage(
                loadingDotsRoot.transform,
                "Dot02",
                LoadSprite("LoadingDots/loading_dot_02.png"));
            ConfigureDot(dot02.rectTransform, 0f);
            dot02.preserveAspect = true;

            Image dot03 = EnsureChildImage(
                loadingDotsRoot.transform,
                "Dot03",
                LoadSprite("LoadingDots/loading_dot_03.png"));
            ConfigureDot(dot03.rectTransform, 34f);
            dot03.preserveAspect = true;

            UILoadingDotsAnimator dotsAnimator =
                loadingDotsRoot.GetComponent<UILoadingDotsAnimator>();

            if (dotsAnimator == null)
            {
                dotsAnimator = loadingDotsRoot.AddComponent<UILoadingDotsAnimator>();
            }

            ConfigureDotsAnimator(dotsAnimator, new[] { dot01, dot02, dot03 });

            UIFadeGraphic clickPromptFade = null;
            GameObject titlePromptImage = FindSceneObjectByName("TitlePromptImage");

            if (titlePromptImage != null)
            {
                clickPromptFade = titlePromptImage.GetComponent<UIFadeGraphic>();
            }

            UIFadeOverlay fadeOverlay = EnsureFadePanel(canvas.transform);

            GameObject controllerObject = FindSceneObjectByName("BootTitleUIController");

            if (controllerObject == null)
            {
                controllerObject = new GameObject("BootTitleUIController");
            }

            controllerObject.transform.SetParent(canvas.transform, false);
            BootTitleUIController controller =
                controllerObject.GetComponent<BootTitleUIController>();

            if (controller == null)
            {
                controller = controllerObject.AddComponent<BootTitleUIController>();
            }

            ConfigureController(
                controller,
                titleRoot,
                loadingRoot,
                clickPromptFade,
                dotsAnimator,
                fadeOverlay);

            SetSiblingOrder(
                loadingRoot.transform,
                loadingBackgroundBase.gameObject,
                loadingBackgroundPattern.gameObject,
                loadingLogo.gameObject,
                loadingBottomBar.gameObject,
                loadingDotsRoot);

            fadeOverlay.transform.SetAsLastSibling();

            ConfigureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Boot loading flow setup complete.");
        }

        private static void ConfigureSpriteImportSettings()
        {
            foreach (string path in SpritePaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    Debug.LogWarning($"Missing texture importer: {path}");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        private static GameObject CreateRoot(string name, Transform parent, bool active)
        {
            GameObject root = CreateRectObject(name, parent);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            root.SetActive(active);
            return root;
        }

        private static Image CreateFullScreenImage(
            string name,
            Transform parent,
            Sprite sprite,
            Image.Type imageType,
            bool preserveAspect)
        {
            GameObject imageObject = CreateRectObject(name, parent);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = imageType;
            image.preserveAspect = preserveAspect;
            return image;
        }

        private static Image CreateBottomStretchImage(
            string name,
            Transform parent,
            Sprite sprite,
            float yOffset)
        {
            GameObject imageObject = CreateRectObject(name, parent);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(0f, sprite != null ? sprite.rect.height : 120f);

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            return image;
        }

        private static Image CreateCornerImage(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 pivot,
            float scale)
        {
            Image image = CreateAnchoredImage(name, parent, sprite, anchor, anchoredPosition, scale);
            RectTransform rect = image.rectTransform;
            rect.pivot = pivot;
            return image;
        }

        private static Image CreateAnchoredImage(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 anchor,
            Vector2 anchoredPosition,
            float scale)
        {
            GameObject imageObject = CreateRectObject(name, parent);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.SetNativeSize();
            rect.sizeDelta *= scale;
            return image;
        }

        private static Image CreateDot(string name, Transform parent, Sprite sprite, float x)
        {
            Image image = CreateAnchoredImage(
                name,
                parent,
                sprite,
                new Vector2(0.5f, 0.5f),
                new Vector2(x, 0f),
                1f);
            return image;
        }

        private static GameObject CreateRectObject(string name, Transform parent)
        {
            GameObject rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.layer = parent != null ? parent.gameObject.layer : 5;
            rectObject.transform.SetParent(parent, false);
            return rectObject;
        }

        private static GameObject FindOrWarn(string name)
        {
            GameObject found = GameObject.Find(name);

            if (found == null)
            {
                Debug.LogWarning($"{name} was not found in 00_Boot.unity.");
            }

            return found;
        }

        private static void SetParentAndReset(GameObject child, Transform parent)
        {
            if (child == null || parent == null)
            {
                return;
            }

            child.transform.SetParent(parent, false);
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }
        }

        private static void ConfigureFullStretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureBottomBar(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 120f);
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureTitleContentArea(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(0f, 120f);
            rectTransform.offsetMax = new Vector2(0f, 0f);
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureCenteredFixed(RectTransform rectTransform, Vector2 size)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureVersionImage(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(24f, -20f);
            rectTransform.sizeDelta = new Vector2(150f, 28f);
            rectTransform.localScale = Vector3.one;
        }

        private static void SetPreserveAspect(GameObject target, bool preserveAspect)
        {
            if (target == null)
            {
                return;
            }

            Image image = target.GetComponent<Image>();

            if (image != null)
            {
                image.preserveAspect = preserveAspect;
            }
        }

        private static void SetSiblingOrder(Transform parent, params GameObject[] children)
        {
            int index = 0;

            foreach (GameObject child in children)
            {
                if (child == null || child.transform.parent != parent)
                {
                    continue;
                }

                child.transform.SetSiblingIndex(index);
                index++;
            }
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(
                    "Canvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
        }

        private static Image EnsureChildImage(
            Transform parent,
            string name,
            Sprite sprite)
        {
            GameObject child = FindDirectChild(parent, name);

            if (child == null)
            {
                child = CreateRectObject(name, parent);
            }

            SetParentAndReset(child, parent);

            Image image = child.GetComponent<Image>();

            if (image == null)
            {
                image = child.AddComponent<Image>();
            }

            image.sprite = sprite;
            return image;
        }

        private static UIFadeOverlay EnsureFadePanel(Transform canvasTransform)
        {
            GameObject fadePanel = FindSceneObjectByName("FadePanel");

            if (fadePanel == null)
            {
                fadePanel = CreateRectObject("FadePanel", canvasTransform);
            }

            SetParentAndReset(fadePanel, canvasTransform);
            ConfigureFullStretch(fadePanel.GetComponent<RectTransform>());
            fadePanel.SetActive(true);

            Image image = fadePanel.GetComponent<Image>();

            if (image == null)
            {
                image = fadePanel.AddComponent<Image>();
            }

            image.sprite = null;
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            image.preserveAspect = false;

            UIFadeOverlay overlay = fadePanel.GetComponent<UIFadeOverlay>();

            if (overlay == null)
            {
                overlay = fadePanel.AddComponent<UIFadeOverlay>();
            }

            SerializedObject serializedObject = new SerializedObject(overlay);
            serializedObject.FindProperty("fadeImage").objectReferenceValue = image;
            serializedObject.FindProperty("startTransparent").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return overlay;
        }

        private static GameObject FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static GameObject FindSceneObjectByName(string name)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject sceneObject in objects)
            {
                if (sceneObject.name != name)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(sceneObject))
                {
                    continue;
                }

                return sceneObject;
            }

            return null;
        }

        private static GameObject ResolveSingleLoadingRoot()
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject selected = null;
            List<GameObject> duplicates = new List<GameObject>();

            foreach (GameObject sceneObject in objects)
            {
                if (sceneObject.name != "LoadingRoot" || EditorUtility.IsPersistent(sceneObject))
                {
                    continue;
                }

                duplicates.Add(sceneObject);

                if (selected == null || sceneObject.transform.childCount > selected.transform.childCount)
                {
                    selected = sceneObject;
                }
            }

            foreach (GameObject duplicate in duplicates)
            {
                if (duplicate == selected)
                {
                    continue;
                }

                Object.DestroyImmediate(duplicate);
            }

            return selected;
        }

        private static void ConfigureDot(RectTransform rectTransform, float x)
        {
            ConfigureCenteredFixed(rectTransform, new Vector2(18f, 18f));
            rectTransform.anchoredPosition = new Vector2(x, 0f);
        }

        private static void ConfigureBuildSettings()
        {
            string[] scenePaths =
            {
                "Assets/_Project/Scenes/00_Boot.unity",
                "Assets/_Project/Scenes/01_TeaHouse_Backyard.unity",
                "Assets/_Project/Scenes/02_Combat_Test.unity"
            };

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            foreach (string scenePath in scenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Sprite LoadSprite(string relativePath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/_Project/Art/UI/TitleScreen/{relativePath}");
        }

        private static void ConfigureFade(
            UIFadeGraphic fade,
            Graphic graphic,
            float minAlpha,
            float maxAlpha,
            float duration,
            bool playOnEnable,
            bool loop)
        {
            SerializedObject serializedObject = new SerializedObject(fade);
            serializedObject.FindProperty("targetGraphic").objectReferenceValue = graphic;
            serializedObject.FindProperty("minAlpha").floatValue = minAlpha;
            serializedObject.FindProperty("maxAlpha").floatValue = maxAlpha;
            serializedObject.FindProperty("duration").floatValue = duration;
            serializedObject.FindProperty("playOnEnable").boolValue = playOnEnable;
            serializedObject.FindProperty("loop").boolValue = loop;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIFadeGraphic ConfigureFadeOnObject(
            GameObject target,
            float minAlpha,
            float maxAlpha,
            float duration,
            bool playOnEnable,
            bool loop)
        {
            if (target == null)
            {
                return null;
            }

            Image image = target.GetComponent<Image>();

            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            UIFadeGraphic fade = target.GetComponent<UIFadeGraphic>();

            if (fade == null)
            {
                fade = target.AddComponent<UIFadeGraphic>();
            }

            ConfigureFade(fade, image, minAlpha, maxAlpha, duration, playOnEnable, loop);
            return fade;
        }

        private static void RemoveFadeIfPresent(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            UIFadeGraphic fade = target.GetComponent<UIFadeGraphic>();

            if (fade != null)
            {
                Object.DestroyImmediate(fade);
            }
        }

        private static void ConfigureDotsAnimator(UILoadingDotsAnimator animator, Image[] dots)
        {
            SerializedObject serializedObject = new SerializedObject(animator);
            SerializedProperty dotsProperty = serializedObject.FindProperty("dots");
            dotsProperty.arraySize = dots.Length;

            for (int i = 0; i < dots.Length; i++)
            {
                dotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
            }

            serializedObject.FindProperty("interval").floatValue = 0.35f;
            serializedObject.FindProperty("playOnEnable").boolValue = false;
            serializedObject.FindProperty("useUnscaledTime").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureController(
            BootTitleUIController controller,
            GameObject titleRoot,
            GameObject loadingRoot,
            UIFadeGraphic clickPromptFade,
            UILoadingDotsAnimator loadingDotsAnimator,
            UIFadeOverlay fadeOverlay)
        {
            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("titleRoot").objectReferenceValue = titleRoot;
            serializedObject.FindProperty("loadingRoot").objectReferenceValue = loadingRoot;
            serializedObject.FindProperty("clickPromptFade").objectReferenceValue = clickPromptFade;
            serializedObject.FindProperty("loadingDotsAnimator").objectReferenceValue = loadingDotsAnimator;
            serializedObject.FindProperty("clickAnywhereToStart").boolValue = true;
            serializedObject.FindProperty("logStartRequest").boolValue = true;
            serializedObject.FindProperty("targetSceneName").stringValue = "02_Combat_Test";
            serializedObject.FindProperty("minimumLoadingTime").floatValue = 1f;
            serializedObject.FindProperty("loadSceneAfterStart").boolValue = true;
            serializedObject.FindProperty("fadeOverlay").objectReferenceValue = fadeOverlay;
            serializedObject.FindProperty("fadeInDuration").floatValue = 0.25f;
            serializedObject.FindProperty("fadeOutDuration").floatValue = 0.35f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
