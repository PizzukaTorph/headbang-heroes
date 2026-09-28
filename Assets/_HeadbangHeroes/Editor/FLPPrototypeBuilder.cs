using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HeadbangHeroes.Input;
using HeadbangHeroes.Presentation;

namespace HeadbangHeroes.Editor
{
    /// <summary>
    /// Builds the isolated Erik frame-animation proof of concept.
    /// It intentionally has no dependency on gameplay, chart, input, or avatar presentation code.
    /// </summary>
    public static class FLPPrototypeBuilder
    {
        const string FrameRoot = "Assets/_HeadbangHeroes/Content/FLP/Erik/Headbang";
        const string AnimationRoot = "Assets/_HeadbangHeroes/Content/FLP/Erik/Animations";
        const string PrefabRoot = "Assets/_HeadbangHeroes/Prefabs/FLP";
        const string ClipPath = AnimationRoot + "/Erik_Headbang_POC.anim";
        const string ControllerPath = AnimationRoot + "/Erik_Headbang_POC.controller";
        const string PrefabPath = PrefabRoot + "/Erik_FLPPoc.prefab";
        const string ScenePath = "Assets/_HeadbangHeroes/Scenes/FLPHeadbangPOC.unity";
        const string GameplayScenePath = "Assets/_HeadbangHeroes/Scenes/Prototype_Headbang.unity";
        const string GameplayPrefabName = "HH_Avatar_FLP";
        const float FrameRate = 12f;
        const int PixelsPerUnit = 100;

        [MenuItem("Headbang Heroes/FLP/Build Erik Headbang POC")]
        public static void Build()
        {
            EnsureFolder("Assets/_HeadbangHeroes/Content/FLP", "Erik");
            EnsureFolder("Assets/_HeadbangHeroes/Content/FLP/Erik", "Animations");
            EnsureFolder("Assets/_HeadbangHeroes/Prefabs", "FLP");

            var sprites = ConfigureAndLoadFrames();
            var clip = BuildClip(sprites);
            var controller = BuildController(clip);
            var prefab = BuildPrefab(sprites, controller);
            BuildScene(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"FLP POC built with {sprites.Length} frames at {FrameRate} FPS: {ScenePath}");
        }

        [MenuItem("Headbang Heroes/FLP/Integrate Erik Into Gameplay Scene")]
        public static void IntegrateIntoGameplayScene()
        {
            var sprites = ConfigureAndLoadFrames();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                var clip = BuildClip(sprites);
                prefab = BuildPrefab(sprites, BuildController(clip));
            }

            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var legacy = FindRoot(scene, "HH_Avatar_Puppet") ?? FindRoot(scene, "Prototype_Avatar");
            var input = UnityEngine.Object.FindFirstObjectByType<HeadbangInput>();
            if (input == null) throw new InvalidOperationException("Gameplay scene has no HeadbangInput.");

            var oldPosition = legacy != null ? legacy.transform.position : Vector3.zero;
            var oldRotation = legacy != null ? legacy.transform.rotation : Quaternion.identity;
            var oldScale = legacy != null ? legacy.transform.lossyScale : Vector3.one;
            if (legacy != null) legacy.SetActive(false);

            var existing = GameObject.Find(GameplayPrefabName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            avatar.name = GameplayPrefabName;
            avatar.transform.position = oldPosition;
            avatar.transform.rotation = oldRotation;
            avatar.transform.localScale = oldScale;

            var animator = avatar.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;

            var presenter = avatar.GetComponent<FlpAvatarPresenter>();
            if (presenter == null) throw new InvalidOperationException("Erik FLP prefab has no FlpAvatarPresenter.");
            var so = new SerializedObject(presenter);
            so.FindProperty("input").objectReferenceValue = input;
            so.FindProperty("listenToInput").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            presenter.enabled = true;

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"FLP gameplay integration built in {GameplayScenePath}. Legacy avatar disabled; {GameplayPrefabName} listens to HeadbangInput.Bang.");
        }

        static GameObject FindRoot(Scene scene, string objectName)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == objectName) return root;
            return null;
        }

        static Sprite[] ConfigureAndLoadFrames()
        {
            var sprites = new Sprite[16];
            for (var i = 0; i < sprites.Length; i++)
            {
                var path = $"{FrameRoot}/headbang_{i:00}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"Missing Erik frame importer: {path}");

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                var textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);
                textureSettings.spriteAlignment = (int)SpriteAlignment.Center;
                textureSettings.spritePivot = new Vector2(0.5f, 0.5f);
                importer.SetTextureSettings(textureSettings);
                importer.spritePixelsPerUnit = PixelsPerUnit;

                foreach (var platform in new[] { "DefaultTexturePlatform", "Standalone", "Android", "iOS" })
                {
                    var settings = importer.GetPlatformTextureSettings(platform);
                    settings.name = platform;
                    settings.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(settings);
                }

                importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null) throw new InvalidOperationException($"Could not load Erik frame: {path}");
            }

            return sprites;
        }

        static AnimationClip BuildClip(IReadOnlyList<Sprite> sprites)
        {
            DeleteAssetIfPresent(ClipPath);
            var clip = new AnimationClip
            {
                name = "Erik_Headbang_POC",
                frameRate = FrameRate,
                wrapMode = WrapMode.Loop
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (var i = 0; i < sprites.Count; i++)
                keyframes[i] = new ObjectReferenceKeyframe { time = i / FrameRate, value = sprites[i] };

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, ClipPath);
            return clip;
        }

        static AnimatorController BuildController(AnimationClip clip)
        {
            DeleteAssetIfPresent(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.AddState("Headbang");
            state.motion = clip;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        static GameObject BuildPrefab(IReadOnlyList<Sprite> sprites, RuntimeAnimatorController controller)
        {
            DeleteAssetIfPresent(PrefabPath);
            var root = new GameObject("Erik_FLPPoc");
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[0];
            renderer.sortingOrder = 0;
            renderer.drawMode = SpriteDrawMode.Simple;
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var presenter = root.AddComponent<FlpAvatarPresenter>();
            var serializedPresenter = new SerializedObject(presenter);
            serializedPresenter.FindProperty("target").objectReferenceValue = renderer;
            var frames = serializedPresenter.FindProperty("headbangFrames");
            frames.arraySize = sprites.Count;
            for (var i = 0; i < sprites.Count; i++)
                frames.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            presenter.enabled = false;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildScene(GameObject prefab)
        {
            DeleteAssetIfPresent(ScenePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.25f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.025f, 0.04f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            avatar.name = "Erik_FLPPoc";
            avatar.transform.position = Vector3.zero;
            avatar.transform.localScale = Vector3.one;

            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
        }

        static void RegisterSceneInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var index = scenes.FindIndex(scene => scene.path == ScenePath);
            var entry = new EditorBuildSettingsScene(ScenePath, true);
            if (index >= 0) scenes[index] = entry;
            else scenes.Add(entry);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        static void DeleteAssetIfPresent(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
        }
    }
}
