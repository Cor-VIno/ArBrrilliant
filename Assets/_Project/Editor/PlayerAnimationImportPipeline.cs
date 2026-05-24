using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JingHongLu.Feedback;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JingHongLu.EditorTools
{
    public static class PlayerAnimationImportPipeline
    {
        private const string RawRoot = "Assets/_Project/Art/Characters/Player/Raw";
        private const string IdleFolder = RawRoot + "/Idle";
        private const string AnimationFolder = "Assets/_Project/Art/Animations/Player";
        private const string PlayerControllerPath = AnimationFolder + "/Player.controller";
        private const string PlayerIdleClipPath = AnimationFolder + "/Player_Idle.anim";
        private const string CombatScenePath = "Assets/_Project/Scenes/02_Combat_Test.unity";
        private const float FrameRate = 12f;
        private static readonly Regex StopFramePattern =
            new Regex("^stop_(\\d{4})\\.png$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [InitializeOnLoadMethod]
        private static void SyncAnimatorParametersAfterReload()
        {
            EditorApplication.delayCall += SyncAnimatorParametersIfNeeded;
        }

        public static void RunPhase6C()
        {
            EnsureFolder("Assets/_Project/Editor");
            EnsureFolder(AnimationFolder);

            RenameStopFramesToIdle();
            AssetDatabase.Refresh();

            ConfigurePlayerSpriteImports();
            AssetDatabase.Refresh();

            AnimationClip idleClip = CreateIdleClip();
            AnimatorController controller = CreateOrUpdateAnimatorController(idleClip);
            BindSceneAnimator(controller, idleClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase6C] Player animation import pipeline completed.");
        }

        public static void RunPhase6CFix1()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);

            if (controller == null)
            {
                Debug.LogError($"[Phase6C-Fix1] Player controller not found: {PlayerControllerPath}");
                return;
            }

            EnsurePlayerAnimatorParameters(controller);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase6C-Fix1] Player animator parameters synced.");
        }

        private static void SyncAnimatorParametersIfNeeded()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);

            if (controller == null || HasRequiredPlayerAnimatorParameters(controller))
            {
                return;
            }

            EnsurePlayerAnimatorParameters(controller);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log("[Phase6C-Fix1] Missing player animator parameters were added.");
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

        private static void RenameStopFramesToIdle()
        {
            if (!AssetDatabase.IsValidFolder(IdleFolder))
            {
                Debug.LogWarning($"[Phase6C] Idle folder not found: {IdleFolder}");
                return;
            }

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { IdleFolder });

            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(path);
                Match match = StopFramePattern.Match(fileName);

                if (!match.Success)
                {
                    continue;
                }

                string newName = $"Idle_{match.Groups[1].Value}";
                string error = AssetDatabase.RenameAsset(path, newName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"[Phase6C] Failed to rename {path}: {error}");
                }
            }
        }

        private static void ConfigurePlayerSpriteImports()
        {
            if (!AssetDatabase.IsValidFolder(RawRoot))
            {
                Debug.LogWarning($"[Phase6C] Player raw folder not found: {RawRoot}");
                return;
            }

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { RawRoot });

            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }

        private static AnimationClip CreateIdleClip()
        {
            Sprite[] sprites = LoadSprites(IdleFolder, "Idle_*.png");

            if (sprites.Length == 0)
            {
                Debug.LogWarning("[Phase6C] No Idle sprites found. Idle clip was not created.");
                return null;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerIdleClipPath);

            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, PlayerIdleClipPath);
            }

            clip.frameRate = FrameRate;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / FrameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static Sprite[] LoadSprites(string folder, string pattern)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return Array.Empty<Sprite>();
            }

            string absoluteFolder = Path.GetFullPath(folder);

            return Directory.GetFiles(absoluteFolder, pattern)
                .Select(ToAssetPath)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(sprite => sprite != null)
                .ToArray();
        }

        private static string ToAssetPath(string absolutePath)
        {
            string normalized = absolutePath.Replace('\\', '/');
            string projectRoot = Path.GetFullPath(".").Replace('\\', '/').TrimEnd('/');
            return normalized.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring(projectRoot.Length + 1)
                : normalized;
        }

        private static AnimatorController CreateOrUpdateAnimatorController(AnimationClip idleClip)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);

            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(PlayerControllerPath);
            }

            EnsurePlayerAnimatorParameters(controller);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            if (idleClip != null)
            {
                AnimatorState idleState = EnsureState(stateMachine, "Idle", idleClip);
                stateMachine.defaultState = idleState;
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsurePlayerAnimatorParameters(AnimatorController controller)
        {
            EnsureBool(controller, "IsMoving");
            EnsureBool(controller, "IsCharging");
            EnsureBool(controller, "IsGrounded");

            EnsureFloat(controller, "MoveSpeed");
            EnsureFloat(controller, "VerticalSpeed");

            string[] triggers =
            {
                "LightAttack",
                "HeavyStage1",
                "HeavyChargeStart",
                "HeavyRelease",
                "RangedAttack",
                "DashStart",
                "DashEnd",
                "SwordArtStart",
                "PerfectDodge",
                "Hurt",
                "Death",
                "QingFengJueStart",
                "CangLangJueStart",
                "ChangShengJueStart",
                "Attack_Horizontal",
                "Attack_Vertical",
                "Attack_LeftFalling",
                "Attack_RightFalling"
            };

            foreach (string trigger in triggers)
            {
                EnsureTrigger(controller, trigger);
            }
        }

        private static bool HasRequiredPlayerAnimatorParameters(AnimatorController controller)
        {
            string[] required =
            {
                "IsMoving",
                "IsCharging",
                "IsGrounded",
                "MoveSpeed",
                "VerticalSpeed",
                "LightAttack",
                "HeavyStage1",
                "HeavyChargeStart",
                "HeavyRelease",
                "RangedAttack",
                "DashStart",
                "DashEnd",
                "SwordArtStart",
                "PerfectDodge",
                "Hurt",
                "Death",
                "QingFengJueStart",
                "CangLangJueStart",
                "ChangShengJueStart",
                "Attack_Horizontal",
                "Attack_Vertical",
                "Attack_LeftFalling",
                "Attack_RightFalling"
            };

            return required.All(name =>
                controller.parameters.Any(parameter => parameter.name == name));
        }

        private static void EnsureBool(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
            {
                return;
            }

            controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        }

        private static void EnsureFloat(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
            {
                return;
            }

            controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        private static void EnsureTrigger(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
            {
                return;
            }

            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
        }

        private static AnimatorState EnsureState(
            AnimatorStateMachine stateMachine,
            string stateName,
            Motion motion)
        {
            ChildAnimatorState childState =
                stateMachine.states.FirstOrDefault(state => state.state.name == stateName);

            AnimatorState animatorState = childState.state;

            if (animatorState == null)
            {
                animatorState = stateMachine.AddState(stateName);
            }

            animatorState.motion = motion;
            return animatorState;
        }

        private static void BindSceneAnimator(
            AnimatorController controller,
            AnimationClip idleClip)
        {
            Scene scene = EditorSceneManager.OpenScene(CombatScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.FindWithTag("Player");

            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player == null)
            {
                Debug.LogWarning("[Phase6C] Player not found in combat scene.");
                return;
            }

            GameObject visual = FindOrCreateChild(player.transform, "PlayerVisual");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * 0.1f;

            SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>();

            if (visualRenderer == null)
            {
                visualRenderer = visual.AddComponent<SpriteRenderer>();
            }

            SpriteRenderer rootRenderer = player.GetComponent<SpriteRenderer>();

            if (rootRenderer != null)
            {
                visualRenderer.sortingLayerID = rootRenderer.sortingLayerID;
                visualRenderer.sortingOrder = rootRenderer.sortingOrder;
                visualRenderer.flipX = rootRenderer.flipX;
                visualRenderer.color = rootRenderer.color;
                rootRenderer.enabled = false;
            }

            Sprite firstIdleSprite = LoadSprites(IdleFolder, "Idle_*.png").FirstOrDefault();

            if (firstIdleSprite != null)
            {
                visualRenderer.sprite = firstIdleSprite;
            }

            Animator animator = visual.GetComponent<Animator>();

            if (animator == null)
            {
                animator = visual.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            BindFeedbackAnimator<PlayerSkillFeedbackBinder>(player, animator);
            BindFeedbackAnimator<SwordArtFeedbackBinder>(player, animator);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject FindOrCreateChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);

            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void BindFeedbackAnimator<T>(GameObject player, Animator animator)
            where T : Component
        {
            T binder = player.GetComponent<T>();

            if (binder == null)
            {
                binder = player.GetComponentInChildren<T>(true);
            }

            if (binder == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(binder);
            SerializedProperty animatorProperty =
                serializedObject.FindProperty("animator");

            if (animatorProperty == null)
            {
                return;
            }

            animatorProperty.objectReferenceValue = animator;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(binder);
        }
    }
}
