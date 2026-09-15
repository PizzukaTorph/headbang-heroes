using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Core;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Input;
using HeadbangHeroes.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HeadbangHeroes.Editor
{
    public static class M0PrototypeBuilder
    {
        const string Root = "Assets/_HeadbangHeroes";
        const string ChartJsonPath = Root + "/Content/Lab/lab-001-beyond-the-pain-classic-m0.json";
        const string LocalAudioPath = Root + "/Content/Lab/LocalAudio/BeyondThePain.mp3";
        const string GeneratedFolder = Root + "/Content/Lab/Generated";
        const string SongAssetPath = GeneratedFolder + "/Song_Lab001.asset";
        const string SceneFolder = Root + "/Scenes";
        const string ScenePath = SceneFolder + "/Prototype_Headbang.unity";

        [MenuItem("Tools/Headbang Heroes/Build M0 Prototype")]
        public static void Build()
        {
            EnsureFolder(Root + "/Content/Lab", "Generated");
            EnsureFolder(Root, "Scenes");

            var chartJson = AssetDatabase.LoadAssetAtPath<TextAsset>(ChartJsonPath);
            if (chartJson == null)
            {
                Debug.LogError($"Missing chart JSON: {ChartJsonPath}");
                return;
            }

            var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(LocalAudioPath);
            var song = GetOrCreateSong(audio);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Prototype_Headbang";

            var systems = new GameObject("HH_M0_Systems");
            var source = systems.AddComponent<AudioSource>();
            source.playOnAwake = false;
            var clock = systems.AddComponent<AudioClock>();
            var scheduler = systems.AddComponent<ChartScheduler>();
            var input = systems.AddComponent<HeadbangInput>();
            var controller = systems.AddComponent<PrototypeController>();

            Assign(clock, "source", source);
            Assign(scheduler, "clock", clock);
            Assign(scheduler, "approachTime", 1.0);

            var canvas = CreateCanvas();
            CreateBackground(canvas.transform);
            var avatar = CreateAvatar(canvas.transform, out var headMotion);
            var cue = CreateTimingCue(canvas.transform, clock);
            var hud = CreateHud(canvas.transform);

            Assign(controller, "song", song);
            Assign(controller, "chartJsonOverride", chartJson);
            Assign(controller, "clock", clock);
            Assign(controller, "scheduler", scheduler);
            Assign(controller, "input", input);
            Assign(controller, "head", headMotion);
            Assign(controller, "cue", cue);
            Assign(controller, "hud", hud);
            Assign(controller, "startOnPlay", true);
            Assign(controller, "startSongTime", 22.0);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = avatar;

            if (audio == null)
                Debug.LogWarning($"HH M0 scene created, but audio is missing. Put the supplied MP3 at: {LocalAudioPath} and run this menu command again.");
            else
                Debug.Log("HH M0 prototype scene created and wired. Open Prototype_Headbang and press Play.");
        }

        static SongDefinition GetOrCreateSong(AudioClip audio)
        {
            var song = AssetDatabase.LoadAssetAtPath<SongDefinition>(SongAssetPath);
            if (song == null)
            {
                song = ScriptableObject.CreateInstance<SongDefinition>();
                AssetDatabase.CreateAsset(song, SongAssetPath);
            }
            song.songId = "lab-001-beyond-the-pain";
            song.title = "Beyond the Pain";
            song.artist = "Asidie";
            song.audio = audio;
            EditorUtility.SetDirty(song);
            return song;
        }

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        static void CreateBackground(Transform parent)
        {
            var bg = CreateRect("Background", parent, Vector2.zero, new Vector2(1080, 1920));
            var image = bg.gameObject.AddComponent<Image>();
            image.color = new Color(0.055f, 0.055f, 0.07f, 1f);
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
        }

        static GameObject CreateAvatar(Transform parent, out HeadMotionModel motion)
        {
            var avatar = new GameObject("Prototype_Avatar", typeof(RectTransform));
            var root = avatar.GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.42f);
            root.sizeDelta = new Vector2(420, 620);

            var torso = CreateRect("Torso", root, new Vector2(0, -105), new Vector2(310, 390));
            torso.gameObject.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.2f, 1f);

            var head = CreateRect("Head", root, new Vector2(0, 170), new Vector2(185, 185));
            var headImage = head.gameObject.AddComponent<Image>();
            headImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            headImage.color = new Color(0.62f, 0.52f, 0.43f, 1f);

            motion = avatar.AddComponent<HeadMotionModel>();
            Assign(motion, "head", head);
            return avatar;
        }

        static ClosingCircleCue CreateTimingCue(Transform parent, AudioClock clock)
        {
            var root = new GameObject("TimingCue", typeof(RectTransform), typeof(CanvasGroup));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.42f);
            rect.anchoredPosition = new Vector2(0, 170);
            rect.sizeDelta = new Vector2(260, 260);

            var group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var target = CreateRect("TargetRing", rect, Vector2.zero, new Vector2(215, 215));
            target.gameObject.AddComponent<RingGraphic>().color = new Color(1f, 1f, 1f, 0.9f);

            var approach = CreateRect("ApproachRing", rect, Vector2.zero, new Vector2(215, 215));
            approach.gameObject.AddComponent<RingGraphic>().color = new Color(0.9f, 0.2f, 0.2f, 1f);

            var cue = root.AddComponent<ClosingCircleCue>();
            Assign(cue, "clock", clock);
            Assign(cue, "approachRing", approach);
            Assign(cue, "canvasGroup", group);
            return cue;
        }

        static PrototypeHud CreateHud(Transform parent)
        {
            var root = new GameObject("PrototypeHUD", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var score = CreateText("Score", rect, new Vector2(-270, 790), new Vector2(420, 100), 54, TextAnchor.MiddleLeft);
            var combo = CreateText("Combo", rect, new Vector2(300, 790), new Vector2(300, 100), 54, TextAnchor.MiddleRight);
            var judgment = CreateText("Judgment", rect, new Vector2(0, 360), new Vector2(700, 180), 62, TextAnchor.MiddleCenter);

            var hud = root.AddComponent<PrototypeHud>();
            Assign(hud, "scoreText", score);
            Assign(hud, "comboText", combo);
            Assign(hud, "judgmentText", judgment);
            return hud;
        }

        static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var rect = CreateRect(name, parent, position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = "";
            text.raycastTarget = false;
            return text;
        }

        static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        static void EnsureFolder(string parent, string child)
        {
            var full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
        }

        static void Assign(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(Object target, string property, double value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).doubleValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(property).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
