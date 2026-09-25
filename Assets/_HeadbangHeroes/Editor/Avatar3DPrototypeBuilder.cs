using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeadbangHeroes.Editor
{
    /// <summary>
    /// Deterministically creates the HH-owned 3D presentation host and a parallel gameplay scene.
    /// The existing 2D scene and prefab are never edited by this builder.
    /// </summary>
    public static class Avatar3DPrototypeBuilder
    {
        const string Root = "Assets/_HeadbangHeroes";
        const string PrefabPath = Root + "/Prefabs/Avatar/Avatar3D_Prototype.prefab";
        const string ScenePath = Root + "/Scenes/Prototype_Headbang_3D.unity";
        const string SourceScenePath = Root + "/Scenes/Prototype_Headbang.unity";
        [MenuItem("Headbang Heroes/Avatar 3D/Build Sidekick Host + Scene")]
        public static void Build()
        {
            EnsureFolder(Root + "/Prefabs", "Avatar");
            EnsureFolder(Root + "/Content/Art", "Generated3D");

            var prefab = BuildPrefab();
            BuildScene(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Headbang Heroes 3D spike built: {PrefabPath} and {ScenePath}");
        }

        static GameObject BuildPrefab()
        {
            var root = new GameObject("Avatar3D");
            var presenter = root.AddComponent<Avatar3DPresenter>();
            var serialized = new SerializedObject(presenter);
            var mapping = serialized.FindProperty("mapping");
            mapping.FindPropertyRelative("neckShare").floatValue = 0.35f;
            mapping.FindPropertyRelative("headShare").floatValue = 0.65f;
            mapping.FindPropertyRelative("chestCompensation").floatValue = 0.08f;
            mapping.FindPropertyRelative("maxVisualAngle").floatValue = 42f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildScene(GameObject prefab)
        {
            if (!AssetDatabase.CopyAsset(SourceScenePath, ScenePath))
                throw new System.InvalidOperationException($"Could not copy {SourceScenePath} to {ScenePath}.");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var oldAvatar = GameObject.Find("HH_Avatar_Puppet");
            if (oldAvatar != null) oldAvatar.SetActive(false);

            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            avatar.name = "HH_Avatar_3D";
            avatar.transform.position = new Vector3(0f, -0.85f, 0f);
            avatar.transform.localScale = Vector3.one;

            var sidekickPrefab = FindHumanoidCharacterPrefab();
            var character = (GameObject)PrefabUtility.InstantiatePrefab(sidekickPrefab, scene);
            character.name = "SidekickCharacter_LocalDependency";
            character.transform.SetParent(avatar.transform, false);
            character.transform.localPosition = Vector3.zero;
            character.transform.localRotation = Quaternion.identity;
            character.transform.localScale = Vector3.one;

            var source = FindSourceNeck();
            var presenter = avatar.GetComponent<Avatar3DPresenter>();
            presenter.SetSource(source);
            var animator = character.GetComponentInChildren<Animator>(true);
            if (animator == null || !animator.isHuman)
                throw new System.InvalidOperationException($"Selected Sidekick prefab has no Humanoid Animator: {AssetDatabase.GetAssetPath(sidekickPrefab)}");
            presenter.SetAnimator(animator);

            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.transform.rotation = Quaternion.identity;
            }

            EnsureKeyLight();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            Selection.activeGameObject = avatar;
        }

        static void RegisterSceneInBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var index = scenes.FindIndex(scene => scene.path == ScenePath);
            var entry = new EditorBuildSettingsScene(ScenePath, true);
            if (index >= 0) scenes[index] = entry;
            else scenes.Add(entry);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static NeckMotionModel FindSourceNeck()
        {
            var sourceObject = GameObject.Find("Prototype_Avatar");
            if (sourceObject == null)
                throw new System.InvalidOperationException("Prototype_Avatar was not found in the copied gameplay scene.");
            var source = sourceObject.GetComponent<NeckMotionModel>();
            if (source == null)
                throw new System.InvalidOperationException("Prototype_Avatar has no NeckMotionModel.");
            return source;
        }

        static GameObject FindHumanoidCharacterPrefab()
        {
            const string searchRoot = "Assets/Synty/SidekickCharacters/Characters";
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { searchRoot });
            System.Array.Sort(guids, (left, right) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(left), AssetDatabase.GUIDToAssetPath(right),
                    System.StringComparison.Ordinal));

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var animator = prefab == null ? null : prefab.GetComponentInChildren<Animator>(true);
                if (animator != null && animator.avatar != null)
                {
                    Debug.Log($"Using local Humanoid avatar dependency: {path}");
                    return prefab;
                }
            }

            throw new System.InvalidOperationException(
                $"No local Humanoid Sidekick prefab was found below {searchRoot}. Install/refresh the local dependency, then rebuild.");
        }

        static void EnsureKeyLight()
        {
            if (Object.FindAnyObjectByType<Light>() != null) return;
            var go = new GameObject("3D_KeyLight");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            go.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
        }

        static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
