using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeadbangHeroes.Editor
{
    /// <summary>
    /// Deterministically creates the placeholder 3D avatar prefab and a parallel gameplay scene.
    /// The existing 2D scene and prefab are never edited by this builder.
    /// </summary>
    public static class Avatar3DPrototypeBuilder
    {
        const string Root = "Assets/_HeadbangHeroes";
        const string PrefabPath = Root + "/Prefabs/Avatar/Avatar3D_Prototype.prefab";
        const string ScenePath = Root + "/Scenes/Prototype_Headbang_3D.unity";
        const string SourceScenePath = Root + "/Scenes/Prototype_Headbang.unity";
        const string MaterialFolder = Root + "/Content/Art/Generated3D";

        [MenuItem("Headbang Heroes/Avatar 3D/Build Placeholder Prefab + Scene")]
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
            var rootNode = Child(root.transform, "Root", Vector3.zero);
            var bodyRoot = Child(rootNode, "BodyRoot", new Vector3(0f, 0.65f, 0f));
            var spine = Child(bodyRoot, "Spine", new Vector3(0f, 0.35f, 0f));
            var chest = Child(spine, "Chest", new Vector3(0f, 0.35f, 0f));

            var bodyMaterial = Material("Avatar3D_Body", new Color(0.08f, 0.09f, 0.11f));
            var skinMaterial = Material("Avatar3D_Skin", new Color(0.62f, 0.38f, 0.25f));
            var darkMaterial = Material("Avatar3D_Dark", new Color(0.025f, 0.025f, 0.035f));

            Primitive("ChestMesh", PrimitiveType.Capsule, chest, new Vector3(0f, 0f, 0f),
                new Vector3(1.55f, 1.35f, 0.8f), bodyMaterial);

            var neck = Child(chest, "Neck", new Vector3(0f, 1.05f, 0f));
            Primitive("NeckMesh", PrimitiveType.Cylinder, neck, Vector3.zero,
                new Vector3(0.42f, 0.5f, 0.42f), skinMaterial);

            var head = Child(chest, "Head", new Vector3(0f, 1.9f, 0f));
            Primitive("HeadMesh", PrimitiveType.Sphere, head, Vector3.zero,
                new Vector3(1.05f, 1.15f, 0.9f), skinMaterial);
            Primitive("HairCap", PrimitiveType.Sphere, head, new Vector3(0f, 0.2f, -0.08f),
                new Vector3(1.1f, 0.52f, 0.94f), darkMaterial);

            var shoulderLeft = Child(chest, "Shoulder_L", new Vector3(-0.95f, 0.1f, 0f));
            Primitive("ShoulderMesh_L", PrimitiveType.Sphere, shoulderLeft, Vector3.zero,
                new Vector3(0.55f, 0.55f, 0.65f), bodyMaterial);
            var armLeft = Child(shoulderLeft, "Arm_L", new Vector3(-0.35f, -0.55f, 0f));
            Primitive("ArmMesh_L", PrimitiveType.Capsule, armLeft, Vector3.zero,
                new Vector3(0.42f, 1.1f, 0.42f), skinMaterial);

            var shoulderRight = Child(chest, "Shoulder_R", new Vector3(0.95f, 0.1f, 0f));
            Primitive("ShoulderMesh_R", PrimitiveType.Sphere, shoulderRight, Vector3.zero,
                new Vector3(0.55f, 0.55f, 0.65f), bodyMaterial);
            var armRight = Child(shoulderRight, "Arm_R", new Vector3(0.35f, -0.55f, 0f));
            Primitive("ArmMesh_R", PrimitiveType.Capsule, armRight, Vector3.zero,
                new Vector3(0.42f, 1.1f, 0.42f), skinMaterial);

            var presenter = root.AddComponent<Avatar3DPresenter>();
            var serialized = new SerializedObject(presenter);
            serialized.FindProperty("neckBone").objectReferenceValue = neck;
            serialized.FindProperty("headBone").objectReferenceValue = head;
            serialized.FindProperty("chestBone").objectReferenceValue = chest;
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
            avatar.transform.position = new Vector3(0f, -1.25f, 0f);
            avatar.transform.localScale = Vector3.one * 0.62f;

            var source = FindSourceNeck();
            var presenter = avatar.GetComponent<Avatar3DPresenter>();
            presenter.SetSource(source);

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

        static void EnsureKeyLight()
        {
            if (Object.FindAnyObjectByType<Light>() != null) return;
            var go = new GameObject("3D_KeyLight");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            go.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
        }

        static Transform Child(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        static void Primitive(string name, PrimitiveType type, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        static Material Material(string name, Color color)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void EnsureFolder(string parent, string name)
        {
            var path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
