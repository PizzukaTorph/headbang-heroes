#if UNITY_EDITOR
using System.IO;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeadbangHeroes.Editor
{
    /// <summary>Editor-only setup for the committed 2D avatar source sets and reusable prefab.</summary>
    public static class AvatarPuppetBuilder
    {
        const string PrefabPath = "Assets/_HeadbangHeroes/Prefabs/Avatar/Avatar_Puppet.prefab";
        const string ScenePath = "Assets/_HeadbangHeroes/Scenes/Prototype_Headbang.unity";
        const float PixelsPerUnit = 100f;

        [MenuItem("Headbang Heroes/Avatar/Build Clean Puppet + Prototype")]
        public static void BuildAndInstall()
        {
            ConfigureAvatarTextures();
            AssetDatabase.Refresh();
            BuildPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            InstallIntoPrototype(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Headbang Heroes avatar puppet built: {PrefabPath} (default set: Clean; Modular serialized for later switching).");
        }

        [MenuItem("Headbang Heroes/Avatar/Configure Sprite Import Settings")]
        public static void ConfigureAvatarTextures()
        {
            foreach (var set in new[] { "clean", "modular" })
            foreach (var part in Parts)
            {
                var path = $"Assets/sprites/{set}/{part}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        static readonly string[] Parts = { "Torso", "Arm_L", "Arm_R", "Neck", "Head", "Hair_Back", "Hair_Front" };

        static GameObject BuildPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("Avatar");
            var torso = Sprite(root, "Torso", "Torso", Vector3.zero, 0);
            var armLeft = Sprite(root, "Arm_L", "Arm_L", new Vector3(-2.05f, 0.55f, 0f), 1);
            var armRight = Sprite(root, "Arm_R", "Arm_R", new Vector3(2.05f, 0.55f, 0f), 1);

            var hairBackPivot = Child(root.transform, "HairBackPivot", new Vector3(0f, 2.7f, 0f));
            var hairBack = Sprite(hairBackPivot.gameObject, "Hair_Back", "Hair_Back", new Vector3(0f, 1.45f, 0f), 1);
            var neckPivot = Child(root.transform, "NeckPivot", new Vector3(0f, 2f, 0f));
            var neck = Sprite(neckPivot.gameObject, "Neck", "Neck", new Vector3(0f, 0f, 0f), 2);
            var headPivot = Child(neckPivot, "HeadPivot", new Vector3(0f, 0.55f, 0f));
            var head = Sprite(headPivot.gameObject, "Head", "Head", new Vector3(0f, 1.55f, 0f), 3);
            var hairFront = Sprite(headPivot.gameObject, "Hair_Front", "Hair_Front", new Vector3(0f, 1f, 0f), 4);

            var controller = root.AddComponent<AvatarPuppetController>();
            controller.Configure(null, LoadSet("clean"), LoadSet("modular"), torso, armLeft, armRight, neck, head, hairBack, hairFront, neckPivot, headPivot, hairBackPivot);
            controller.SelectArtSet(AvatarArtSet.Clean);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static AvatarSpriteSet LoadSet(string set)
        {
            return new AvatarSpriteSet
            {
                torso = Load(set, "Torso"), armLeft = Load(set, "Arm_L"), armRight = Load(set, "Arm_R"),
                neck = Load(set, "Neck"), head = Load(set, "Head"),
                hairBack = Load(set, "Hair_Back"), hairFront = Load(set, "Hair_Front")
            };
        }

        static Sprite Load(string set, string part) => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/sprites/{set}/{part}.png");

        static SpriteRenderer Sprite(GameObject parent, string name, string pathWithoutExtension, Vector3 position, int order)
        {
            var go = Child(parent.transform, name, position).gameObject;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Load("clean", Path.GetFileName(pathWithoutExtension));
            renderer.sortingOrder = order;
            return renderer;
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

        static void InstallIntoPrototype(GameObject prefab)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new System.InvalidOperationException($"Prototype scene was not found at {ScenePath}.");
            if (prefab == null) throw new System.InvalidOperationException($"Avatar prefab was not imported at {PrefabPath}.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            var old = GameObject.Find("HH_Avatar_Puppet");
            if (old != null) Object.DestroyImmediate(old);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null) throw new System.InvalidOperationException("Avatar prefab could not be instantiated into the prototype scene.");
            SceneManager.MoveGameObjectToScene(instance, scene);
            EditorUtility.SetDirty(instance);
            instance.name = "HH_Avatar_Puppet";
            instance.transform.position = new Vector3(0f, -1.25f, 0f);
            instance.transform.localScale = Vector3.one * 0.62f;

            var placeholder = GameObject.Find("Prototype_Avatar");
            if (placeholder != null)
            {
                foreach (Transform child in placeholder.transform)
                    if (child.name == "Torso" || child.name == "Head") child.gameObject.SetActive(false);
                var source = placeholder.GetComponent<NeckMotionModel>();
                var puppet = instance.GetComponent<AvatarPuppetController>();
                puppet.SetSource(source);
            }

            // The prototype UI is Screen Space Overlay. Its opaque full-screen Background
            // would cover world-space SpriteRenderers, while the camera clear color already
            // provides the same dark backdrop for this presentation PoC.
            var background = GameObject.Find("Background");
            if (background != null) background.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new System.InvalidOperationException($"Avatar prototype scene could not be saved: {ScenePath}.");
            Debug.Log($"Avatar puppet instance persisted in scene '{scene.path}' as '{instance.name}'.");
        }
    }
}
#endif
