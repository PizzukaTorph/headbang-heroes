using HeadbangHeroes.Audio;
using HeadbangHeroes.Charts;
using HeadbangHeroes.Core;
using HeadbangHeroes.Gameplay;
using HeadbangHeroes.Input;
using HeadbangHeroes.Presentation;
using HeadbangHeroes.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace HeadbangHeroes.Editor
{
    /// <summary>
    /// One-click builder for the M0 "One Head, One Song, One Circle" prototype scene.
    /// Idempotent: each run generates a fresh scene from scratch (no leftover objects),
    /// re-uses/updates the generated SongDefinition asset, wires every serialized field,
    /// assigns the chart JSON, and registers the scene in Build Settings.
    /// </summary>
    public static class M0PrototypeBuilder
    {
        const string Root = "Assets/_HeadbangHeroes";
        const string ChartJsonPath = Root + "/Content/Lab/lab-001-beyond-the-pain-classic-m0.json";
        const string LocalAudioPath = Root + "/Content/Lab/LocalAudio/BeyondThePain.mp3";
        const string TempoRampChartPath = Root + "/Content/Lab/lab-002-tempo-ramp.json";
        const string TempoRampAudioPath = Root + "/Content/Lab/TempoRamp.wav";
        const string GeneratedFolder = Root + "/Content/Lab/Generated";
        const string SongAssetPath = GeneratedFolder + "/Song_Lab001.asset";
        const string SceneFolder = Root + "/Scenes";
        const string ScenePath = SceneFolder + "/Prototype_Headbang.unity";

        // Where we look for the (gitignored) real audio. First match wins. Both the canonical
        // LocalAudio/ subfolder and the Content/Lab/ root are accepted, in a few common formats.
        static readonly string[] AudioCandidatePaths =
        {
            Root + "/Content/Lab/LocalAudio/BeyondThePain.mp3",
            Root + "/Content/Lab/LocalAudio/BeyondThePain.wav",
            Root + "/Content/Lab/LocalAudio/BeyondThePain.ogg",
            Root + "/Content/Lab/BeyondThePain.mp3",
            Root + "/Content/Lab/BeyondThePain.wav",
            Root + "/Content/Lab/BeyondThePain.ogg",
        };

        static readonly Color BackgroundColor = new(0.055f, 0.055f, 0.07f, 1f);

        [MenuItem("Tools/Headbang Heroes/Build M0 Prototype")]
        public static void Build() => BuildInternal(ChartJsonPath, tempoRamp: false);

        [MenuItem("Tools/Headbang Heroes/Build M0 Prototype (Tempo Ramp)")]
        public static void BuildTempoRamp() => BuildInternal(TempoRampChartPath, tempoRamp: true);

        static void BuildInternal(string chartPath, bool tempoRamp)
        {
            EnsureFolder(Root + "/Content/Lab", "Generated");
            EnsureFolder(Root, "Scenes");

            var chartJson = AssetDatabase.LoadAssetAtPath<TextAsset>(chartPath);
            if (chartJson == null)
            {
                Debug.LogError($"HH M0 build failed: missing chart JSON at {chartPath}. Cannot build the prototype.");
                return;
            }

            ChartJsonData chartData;
            try
            {
                chartData = ChartJsonLoader.Parse(chartJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HH M0 build failed: could not parse chart JSON at {chartPath}. {e.Message}");
                return;
            }

            AudioClip audio;
            bool usingClickTrack;
            double startSongTime;

            if (tempoRamp)
            {
                // Synchronized tuning track: WAV clicks match the chart exactly; start at 0.
                if (System.IO.File.Exists(AbsoluteFromProject(TempoRampAudioPath)))
                    AssetDatabase.ImportAsset(TempoRampAudioPath, ImportAssetOptions.ForceSynchronousImport);
                audio = AssetDatabase.LoadAssetAtPath<AudioClip>(TempoRampAudioPath);
                usingClickTrack = false;
                if (audio == null) { audio = GetOrCreateClickTrack(chartData); usingClickTrack = true; }
                startSongTime = 0.0;
            }
            else
            {
                var realAudio = FindRealAudio();
                // Testability without the licensed MP3: if missing, generate a synthetic click-track.
                usingClickTrack = realAudio == null;
                audio = realAudio != null ? realAudio : GetOrCreateClickTrack(chartData);
                // The MIDI-derived chart's first hit is ~12.8s; start ~1s before.
                startSongTime = 11.8;
            }

            var song = GetOrCreateSong(audio);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Prototype_Headbang";

            // --- Camera (fixes "No Cameras Rendering") ---
            CreateCamera();

            // --- Systems ---
            var systems = new GameObject("HH_M0_Systems");
            var source = systems.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            var clock = systems.AddComponent<AudioClock>();
            var scheduler = systems.AddComponent<ChartScheduler>();
            var input = systems.AddComponent<HeadbangInput>();
            var controller = systems.AddComponent<PrototypeController>();

            Assign(clock, "source", source);
            Assign(scheduler, "clock", clock);
            Assign(scheduler, "cueLead", 1.0);

            // --- UI ---
            var canvas = CreateCanvas();
            var backgroundImage = CreateBackground(canvas.transform);
            var avatar = CreateAvatar(canvas.transform, out var headMotion, out var head, out var torso, out var hair);
            var hud = CreateHud(canvas.transform, clock, scheduler, headMotion);

            // --- Presentation (downstream only) ---
            var presentation = new GameObject("HH_M0_Presentation");
            var neckPresenter = presentation.AddComponent<NeckPresenter>();
            Assign(neckPresenter, "neck", headMotion);
            Assign(neckPresenter, "head", head);

            var bodyPresenter = presentation.AddComponent<BodyReactionPresenter>();
            Assign(bodyPresenter, "neck", headMotion);
            Assign(bodyPresenter, "body", torso);

            var hairPresenter = presentation.AddComponent<HairReactionPresenter>();
            Assign(hairPresenter, "neck", headMotion);
            Assign(hairPresenter, "strand", hair);

            var venuePresenter = presentation.AddComponent<VenueReactionPresenter>();
            Assign(venuePresenter, "background", backgroundImage);

            var haptics = presentation.AddComponent<HapticsService>();

            // --- Controller wiring ---
            Assign(controller, "song", song);
            Assign(controller, "chartJsonOverride", chartJson);
            Assign(controller, "clock", clock);
            Assign(controller, "scheduler", scheduler);
            Assign(controller, "input", input);
            Assign(controller, "head", headMotion);
            Assign(controller, "hud", hud);
            Assign(controller, "neckPresenter", neckPresenter);
            Assign(controller, "bodyPresenter", bodyPresenter);
            Assign(controller, "hairPresenter", hairPresenter);
            Assign(controller, "venuePresenter", venuePresenter);
            Assign(controller, "haptics", haptics);
            Assign(controller, "startOnPlay", false);   // the GameFlowController starts the run
            Assign(controller, "startSongTime", startSongTime);

            // --- POC product loop: Home / Song Select / Pre-song / Results panels + flow ---
            var flowGo = new GameObject("HH_M0_Flow");
            var flow = flowGo.AddComponent<GameFlowController>();

            var homeText = CreatePanel(canvas.transform, "HomePanel", out var homePanel);
            var songText = CreatePanel(canvas.transform, "SongSelectPanel", out var songPanel);
            var preText = CreatePanel(canvas.transform, "PreSongPanel", out var prePanel);
            var resultsText = CreatePanel(canvas.transform, "ResultsPanel", out var resultsPanel);

            Assign(flow, "gameplay", controller);
            Assign(flow, "homePanel", homePanel);
            Assign(flow, "songSelectPanel", songPanel);
            Assign(flow, "preSongPanel", prePanel);
            Assign(flow, "resultsPanel", resultsPanel);
            Assign(flow, "homeText", homeText);
            Assign(flow, "songSelectText", songText);
            Assign(flow, "preSongText", preText);
            Assign(flow, "resultsText", resultsText);

            // --- P08: on-screen touch controls (Option A UX) ---
            CreateTouchControls(canvas.transform, flow, controller, resultsPanel);

            // --- P08 affordance: brief bang-zone hint at run start ---
            var zoneHint = CreateBangZoneHint(canvas.transform);
            Assign(controller, "zoneHint", zoneHint);

            // --- P08 affordance: per-tap section flash ---
            var zoneFlash = CreateZoneTapFlash(canvas.transform);
            Assign(controller, "zoneFlash", zoneFlash);

            // --- ADR-0001: pulse-in-sector timing cue ---
            var sectorPulse = CreateSectorPulseCue(canvas.transform, clock);
            Assign(controller, "sectorPulse", sectorPulse);

            // --- Input event system ---
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterSceneInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = avatar;

            if (usingClickTrack)
                Debug.LogWarning($"HH M0 prototype ready (using a generated CLICK-TRACK). " +
                    $"To play with the real song, put the MP3 at:\n{LocalAudioPath}\nthen run 'Tools > Headbang Heroes > Build M0 Prototype' again.");
            else
                Debug.Log("HH M0 prototype ready.");
        }

        static void CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera));
            go.tag = "MainCamera";

            var cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;             // world units; UI is ScreenSpaceOverlay so this is for future 2D world content
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BackgroundColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            go.transform.position = new Vector3(0f, 0f, -10f);

            // The scene needs exactly one AudioListener or no audio is heard at all.
            go.AddComponent<AudioListener>();

            // URP requires additional per-camera data. Assembly-CSharp references the URP
            // runtime, so this compiles even without an asmdef.
            var urpData = go.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderType = CameraRenderType.Base;
            urpData.renderPostProcessing = false;
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

        const string ClickTrackPath = GeneratedFolder + "/M0_ClickTrack.wav";
        const int ClickSampleRate = 44100;

        /// <summary>
        /// Generates (once) a mono WAV click-track that plays a short tick at each chart
        /// event time, so the M0 loop is fully playable without the licensed MP3.
        /// </summary>
        static AudioClip GetOrCreateClickTrack(ChartJsonData chart)
        {
            // Compile to the immutable runtime chart so the click-track uses the same validated,
            // normalized events (and precomputed times/directions) that gameplay will consume.
            var runtime = HeadbangHeroes.Charts.Runtime.ChartCompiler.Compile(chart);

            var lastEvent = 0.0;
            foreach (var e in runtime.MotionEvents)
                if (e.Time > lastEvent) lastEvent = e.Time;

            var totalSeconds = (float)(lastEvent + 2.0);
            var totalSamples = Mathf.CeilToInt(totalSeconds * ClickSampleRate);
            var samples = new float[totalSamples];

            // Short decaying sine "tick" per event.
            const float clickSeconds = 0.05f;
            var clickSamples = Mathf.CeilToInt(clickSeconds * ClickSampleRate);
            foreach (var e in runtime.MotionEvents)
            {
                var start = Mathf.RoundToInt((float)e.Time * ClickSampleRate);
                var freq = e.Direction.Axis() == BangAxis.Horizontal ? 660f : 880f; // axis-differentiated tick
                for (var i = 0; i < clickSamples && start + i < totalSamples; i++)
                {
                    var t = (float)i / ClickSampleRate;
                    var env = Mathf.Exp(-t * 45f);
                    samples[start + i] += Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.8f;
                }
            }

            WriteWav(ClickTrackPath, samples, ClickSampleRate, 1);
            AssetDatabase.ImportAsset(ClickTrackPath, ImportAssetOptions.ForceSynchronousImport);

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickTrackPath);
            if (clip == null)
                Debug.LogError($"HH M0 build: failed to generate click-track at {ClickTrackPath}.");
            return clip;
        }

        static void WriteWav(string projectRelativePath, float[] samples, int sampleRate, int channels)
        {
            var absolute = AbsoluteFromProject(projectRelativePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolute));

            using var stream = new System.IO.FileStream(absolute, System.IO.FileMode.Create);
            using var w = new System.IO.BinaryWriter(stream);

            var byteRate = sampleRate * channels * 2;
            var dataSize = samples.Length * 2;

            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + dataSize);
            w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);               // PCM
            w.Write((short)channels);
            w.Write(sampleRate);
            w.Write(byteRate);
            w.Write((short)(channels * 2));  // block align
            w.Write((short)16);              // bits per sample
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            w.Write(dataSize);

            foreach (var s in samples)
            {
                var v = (short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue);
                w.Write(v);
            }
        }

        static string AbsoluteFromProject(string projectRelativePath)
            => System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName, projectRelativePath);

        /// <summary>
        /// Finds the real (gitignored) song audio across the accepted candidate paths/formats.
        /// If a file exists on disk but is not yet imported (just copied in), it force-imports it.
        /// Returns null when no real audio is present (caller falls back to the click-track).
        /// </summary>
        static AudioClip FindRealAudio()
        {
            foreach (var path in AudioCandidatePaths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null && System.IO.File.Exists(AbsoluteFromProject(path)))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
                if (clip != null)
                {
                    Debug.Log($"HH M0: using real audio at {path}");
                    return clip;
                }
            }
            return null;
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

        static Image CreateBackground(Transform parent)
        {
            var bg = CreateRect("Background", parent, Vector2.zero, new Vector2(1080, 1920));
            var image = bg.gameObject.AddComponent<Image>();
            image.color = BackgroundColor;
            image.raycastTarget = false;
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            return image;
        }

        static GameObject CreateAvatar(Transform parent, out NeckMotionModel motion,
            out RectTransform head, out RectTransform torso, out RectTransform hair)
        {
            var avatar = new GameObject("Prototype_Avatar", typeof(RectTransform));
            var root = avatar.GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.42f);
            root.sizeDelta = new Vector2(420, 620);

            torso = CreateRect("Torso", root, new Vector2(0, -105), new Vector2(310, 390));
            var torsoImage = torso.gameObject.AddComponent<Image>();
            torsoImage.color = new Color(0.18f, 0.18f, 0.2f, 1f);
            torsoImage.raycastTarget = false;

            head = CreateRect("Head", root, new Vector2(0, 170), new Vector2(185, 185));
            var headImage = head.gameObject.AddComponent<Image>();
            headImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            headImage.color = new Color(0.62f, 0.52f, 0.43f, 1f);
            headImage.raycastTarget = false;

            // Placeholder hair strand as a child of the head (secondary motion surface).
            hair = CreateRect("HairStrand", head, new Vector2(0, 95), new Vector2(70, 150));
            var hairImage = hair.gameObject.AddComponent<Image>();
            hairImage.color = new Color(0.12f, 0.10f, 0.14f, 1f);
            hairImage.raycastTarget = false;
            hair.pivot = new Vector2(0.5f, 0f); // pivot at the roots so it swings from the head

            // The neck model is domain-only now: it does NOT hold the head transform.
            motion = avatar.AddComponent<NeckMotionModel>();
            return avatar;
        }

        static PrototypeHud CreateHud(Transform parent, AudioClock clock, ChartScheduler scheduler, NeckMotionModel head)
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

            // Compact always-visible top bar (pause/score/progress/multiplier/HYPE), ~top of screen.
            var topBar = CreateText("TopBar", rect, new Vector2(0, 900), new Vector2(1040, 70), 34, TextAnchor.MiddleCenter);

            // Debug telemetry block, anchored to the bottom-left corner.
            var debug = CreateCornerText("Debug", rect);

            var hud = root.AddComponent<PrototypeHud>();
            Assign(hud, "scoreText", score);
            Assign(hud, "comboText", combo);
            Assign(hud, "judgmentText", judgment);
            Assign(hud, "topBarText", topBar);
            Assign(hud, "debugText", debug);
            Assign(hud, "clock", clock);
            Assign(hud, "scheduler", scheduler);
            Assign(hud, "head", head);
            return hud;
        }

        static Text CreatePanel(Transform parent, string name, out CanvasGroup group)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var dim = go.AddComponent<Image>();
            dim.color = new Color(0.03f, 0.03f, 0.05f, 0.96f);
            dim.raycastTarget = false;

            var text = CreateText("Text", rect, new Vector2(0, 0), new Vector2(1000, 1400), 42, TextAnchor.MiddleCenter);

            group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return text;
        }

        static Text CreateCornerText(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(24, 24);
            rect.sizeDelta = new Vector2(520, 560);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.LowerLeft;
            text.color = new Color(0.65f, 0.95f, 0.7f, 0.95f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "";
            text.raycastTarget = false;
            return text;
        }

        static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var rect = CreateRect(name, parent, position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "";
            text.raycastTarget = false;
            return text;
        }

        /// <summary>
        /// Builds the P08 on-screen touch controls (Option A). Buttons are edge/overlay only and never
        /// cover the protected region (head/neck/CURRENT around screen centre, anchor y ~0.42).
        /// </summary>
        /// <summary>
        /// Builds the bang-zone affordance hint: 4 large directional arrows in the real ResolveZone
        /// quadrants (screen split from centre into L/R/U/D). Non-raycast (never eats a bang), inside
        /// a CanvasGroup that BangZoneHint fades out shortly after run start.
        /// </summary>
        /// <summary>
        /// Builds the per-tap section flash: 4 half-screen overlays (Left/Right/Up/Down) each in its
        /// own CanvasGroup, faint, non-raycast. ZoneTapFlash briefly lights the tapped section so the
        /// player sees the whole half was a valid tap area.
        /// </summary>
        /// <summary>
        /// Builds the ADR-0001 pulse-in-sector timing cue: 4 half-screen sector overlays that pulse
        /// with a build-up culminating on the event. Non-raycast; behind gameplay UI.
        /// </summary>
        static SectorPulseCue CreateSectorPulseCue(Transform canvas, AudioClock clock)
        {
            var go = new GameObject("HH_SectorPulseCue", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();   // behind avatar/cue, above background

            var col = new Color(0.95f, 0.55f, 0.2f, 1f);   // alpha via CanvasGroup; recolored at runtime
            var left = MakeFlagHalf(rect, "PulseLeft", new Vector2(0f, 0f), new Vector2(0.5f, 1f), col);
            var right = MakeFlagHalf(rect, "PulseRight", new Vector2(0.5f, 0f), new Vector2(1f, 1f), col);
            var up = MakeFlagHalf(rect, "PulseUp", new Vector2(0f, 0.5f), new Vector2(1f, 1f), col);
            var down = MakeFlagHalf(rect, "PulseDown", new Vector2(0f, 0f), new Vector2(1f, 0.5f), col);

            var pulse = go.AddComponent<SectorPulseCue>();
            Assign(pulse, "clock", clock);
            Assign(pulse, "left", left);
            Assign(pulse, "right", right);
            Assign(pulse, "up", up);
            Assign(pulse, "down", down);
            return pulse;
        }

        static ZoneTapFlash CreateZoneTapFlash(Transform canvas)
        {
            var go = new GameObject("HH_ZoneTapFlash", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            // Sit behind the avatar/cue but above the background; created early in the hierarchy.
            rect.SetAsFirstSibling();

            var col = new Color(0.9f, 0.9f, 1f, 1f);   // alpha driven by the CanvasGroup
            var left = MakeFlagHalf(rect, "FlashLeft", new Vector2(0f, 0f), new Vector2(0.5f, 1f), col);
            var right = MakeFlagHalf(rect, "FlashRight", new Vector2(0.5f, 0f), new Vector2(1f, 1f), col);
            var up = MakeFlagHalf(rect, "FlashUp", new Vector2(0f, 0.5f), new Vector2(1f, 1f), col);
            var down = MakeFlagHalf(rect, "FlashDown", new Vector2(0f, 0f), new Vector2(1f, 0.5f), col);

            var flash = go.AddComponent<ZoneTapFlash>();
            Assign(flash, "left", left);
            Assign(flash, "right", right);
            Assign(flash, "up", up);
            Assign(flash, "down", down);
            return flash;
        }

        static CanvasGroup MakeFlagHalf(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color col)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = col;
            img.raycastTarget = false;             // must never intercept a tap
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            return group;
        }

        static BangZoneHint CreateBangZoneHint(Transform canvas)
        {
            var go = new GameObject("HH_BangZoneHint", typeof(RectTransform), typeof(CanvasGroup));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;      // must NOT intercept taps — zones stay tappable

            var hintCol = new Color(0.85f, 0.85f, 0.95f, 0.55f);
            // Arrows near each edge centre, sized so it's clear the whole quadrant is tappable.
            MakeArrow(rect, "HintUp", "\u25B2\nUP", new Vector2(0.5f, 1f), new Vector2(0, -230), hintCol);
            MakeArrow(rect, "HintDown", "DOWN\n\u25BC", new Vector2(0.5f, 0f), new Vector2(0, 470), hintCol);
            MakeArrow(rect, "HintLeft", "\u25C0 LEFT", new Vector2(0f, 0.5f), new Vector2(230, 0), hintCol);
            MakeArrow(rect, "HintRight", "RIGHT \u25B6", new Vector2(1f, 0.5f), new Vector2(-230, 0), hintCol);

            var hint = go.AddComponent<BangZoneHint>();
            Assign(hint, "group", group);
            return hint;
        }

        static void MakeArrow(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Color col)
        {
            var t = CreateText(name, parent, Vector2.zero, new Vector2(360, 200), 56, TextAnchor.MiddleCenter);
            t.text = label;
            t.color = col;
            t.fontStyle = FontStyle.Bold;
            var r = t.rectTransform;
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
        }

        static void CreateTouchControls(Transform canvas, GameFlowController flow,
            PrototypeController gameplay, CanvasGroup resultsPanel)
        {
            var accent = new Color(0.75f, 0.18f, 0.20f, 0.92f);   // THE BANG / primary
            var neutral = new Color(0.16f, 0.16f, 0.20f, 0.92f);  // secondary
            var quitCol = new Color(0.30f, 0.10f, 0.12f, 0.95f);

            var tcGo = new GameObject("HH_TouchControls", typeof(RectTransform));
            var tcRect = tcGo.GetComponent<RectTransform>();
            tcRect.SetParent(canvas, false);
            tcRect.anchorMin = Vector2.zero; tcRect.anchorMax = Vector2.one;
            tcRect.offsetMin = tcRect.offsetMax = Vector2.zero;
            var tc = tcGo.AddComponent<TouchControls>();

            // PAUSE — top-left corner (top bar area), well away from the thumb bang zones.
            var pause = CreateButton(tcRect, "PauseButton", "II",
                anchor: new Vector2(0f, 1f), anchoredPos: new Vector2(90, -90),
                size: new Vector2(120, 120), bg: neutral, fontSize: 48);

            // THE BANG — bottom-centre, reachable by either thumb; shown only on HYPE READY.
            var bangGroupGo = new GameObject("TheBangGroup", typeof(RectTransform), typeof(CanvasGroup));
            var bangRect = bangGroupGo.GetComponent<RectTransform>();
            bangRect.SetParent(tcRect, false);
            bangRect.anchorMin = bangRect.anchorMax = new Vector2(0.5f, 0f);
            bangRect.pivot = new Vector2(0.5f, 0f);
            bangRect.anchoredPosition = new Vector2(0, 150);
            bangRect.sizeDelta = new Vector2(560, 200);
            var bangGroup = bangGroupGo.GetComponent<CanvasGroup>();
            bangGroup.alpha = 0f; bangGroup.interactable = false; bangGroup.blocksRaycasts = false;
            var theBang = CreateButton(bangRect, "TheBangButton", "THE BANG",
                anchor: new Vector2(0.5f, 0.5f), anchoredPos: Vector2.zero,
                size: new Vector2(560, 200), bg: accent, fontSize: 64);

            // PAUSE overlay: dim full-screen CanvasGroup with RESUME/RETRY/QUIT + calibration +/-.
            var overlayGo = new GameObject("PauseOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var overlayRect = overlayGo.GetComponent<RectTransform>();
            overlayRect.SetParent(tcRect, false);
            overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            overlayGo.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.04f, 0.85f);
            var overlay = overlayGo.GetComponent<CanvasGroup>();
            overlay.alpha = 0f; overlay.interactable = false; overlay.blocksRaycasts = false;

            var title = CreateText("PauseTitle", overlayRect, new Vector2(0, 560), new Vector2(900, 140), 72, TextAnchor.MiddleCenter);
            title.text = "PAUSED";
            var resume = CreateButton(overlayRect, "ResumeButton", "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(560, 150), accent, 52);
            var pRetry = CreateButton(overlayRect, "PauseRetryButton", "RETRY", new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(560, 150), neutral, 52);
            var quit = CreateButton(overlayRect, "QuitButton", "QUIT", new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(560, 150), quitCol, 52);
            var calText = CreateText("CalibrationText", overlayRect, new Vector2(0, -360), new Vector2(700, 180), 40, TextAnchor.MiddleCenter);
            calText.text = "CALIBRATION";
            var calMinus = CreateButton(overlayRect, "CalMinusButton", "-", new Vector2(0.5f, 0.5f), new Vector2(-200, -520), new Vector2(150, 150), neutral, 64);
            var calPlus = CreateButton(overlayRect, "CalPlusButton", "+", new Vector2(0.5f, 0.5f), new Vector2(200, -520), new Vector2(150, 150), neutral, 64);

            // RESULTS actions — large one-tap RETRY / CONTINUE, parented to the results panel so they
            // show/hide with it.
            var rRetry = CreateButton(resultsPanel.transform, "ResultsRetryButton", "RETRY", new Vector2(0.5f, 0f), new Vector2(-300, 220), new Vector2(500, 160), accent, 56);
            var rCont = CreateButton(resultsPanel.transform, "ResultsContinueButton", "CONTINUE", new Vector2(0.5f, 0f), new Vector2(300, 220), new Vector2(500, 160), neutral, 52);

            // --- Wire TouchControls ---
            Assign(tc, "flow", flow);
            Assign(tc, "gameplay", gameplay);
            Assign(tc, "pauseButton", pause);
            Assign(tc, "theBangButton", theBang);
            Assign(tc, "theBangGroup", bangGroup);
            Assign(tc, "pauseOverlay", overlay);
            Assign(tc, "resumeButton", resume);
            Assign(tc, "pauseRetryButton", pRetry);
            Assign(tc, "quitButton", quit);
            Assign(tc, "calMinusButton", calMinus);
            Assign(tc, "calPlusButton", calPlus);
            Assign(tc, "calibrationText", calText);
            Assign(tc, "resultsRetryButton", rRetry);
            Assign(tc, "resultsContinueButton", rCont);
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

        /// <summary>
        /// A touch button: an Image (raycast target, so HeadbangInput suppresses a bang on it) with a
        /// centered label. Anchored to the given normalized anchor so portrait layout is stable.
        /// </summary>
        static Button CreateButton(Transform parent, string name, string label, Vector2 anchor,
            Vector2 anchoredPos, Vector2 size, Color bg, int fontSize = 40)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.color = bg;                 // raycastTarget defaults true -> eats the tap

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText(name + "Label", rect, Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
            text.text = label;
            var t = text.rectTransform;
            t.anchorMin = Vector2.zero; t.anchorMax = Vector2.one; t.offsetMin = t.offsetMax = Vector2.zero;

            return button;
        }

        static void RegisterSceneInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == ScenePath)
                {
                    // Already present: make sure it is enabled and keep the list intact.
                    if (!s.enabled)
                    {
                        s.enabled = true;
                        EditorBuildSettings.scenes = scenes;
                    }
                    return;
                }
            }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void EnsureFolder(string parent, string child)
        {
            var full = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
        }

        static void Assign(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null)
            {
                Debug.LogError($"HH M0 builder: '{target.GetType().Name}' has no serialized field '{property}'. Wiring skipped.");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(Object target, string property, double value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null)
            {
                Debug.LogError($"HH M0 builder: '{target.GetType().Name}' has no serialized field '{property}'. Wiring skipped.");
                return;
            }
            prop.doubleValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(property);
            if (prop == null)
            {
                Debug.LogError($"HH M0 builder: '{target.GetType().Name}' has no serialized field '{property}'. Wiring skipped.");
                return;
            }
            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
