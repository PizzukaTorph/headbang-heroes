#if UNITY_EDITOR
using System.IO;
using System.Linq;
using HeadbangHeroes.Charts.Authoring;
using UnityEditor;
using UnityEngine;

namespace HeadbangHeroes.Editor
{
    public static class HhMidiImportMenu
    {
        [MenuItem("Headbang Heroes/Charts/Import HH-MIDI...")]
        public static void Import()
        {
            var source = EditorUtility.OpenFilePanel("Import Headbang Heroes MIDI", Application.dataPath, "mid");
            if (string.IsNullOrEmpty(source)) return;
            try
            {
                var midi = MidiFileParser.Parse(File.ReadAllBytes(source));
                var fileName = Path.GetFileNameWithoutExtension(source);
                var chartId = fileName.EndsWith(".hh", System.StringComparison.OrdinalIgnoreCase)
                    ? fileName.Substring(0, fileName.Length - 3).TrimEnd('.')
                    : fileName;
                var result = HhMidiImporter.Import(midi, chartId, chartId);
                foreach (var diagnostic in result.Diagnostics)
                    Debug.Log((diagnostic.IsError ? "[HH-MIDI] " : "[HH-MIDI] ") + diagnostic);
                if (result.HasErrors || result.Chart == null)
                {
                    EditorUtility.DisplayDialog("HH-MIDI import failed", string.Join("\n", result.Diagnostics.Select(d => d.ToString())), "OK");
                    return;
                }
                var target = Path.ChangeExtension(source, ".chart.json");
                File.WriteAllText(target, JsonUtility.ToJson(result.Chart, true));
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("HH-MIDI imported", $"Chart written to:\n{target}", "OK");
            }
            catch (MidiFileParser.MidiParseException exception)
            { EditorUtility.DisplayDialog("HH-MIDI parse failed", exception.Message, "OK"); }
            catch (IOException exception)
            { EditorUtility.DisplayDialog("HH-MIDI read failed", exception.Message, "OK"); }
        }

        [MenuItem("Headbang Heroes/Charts/Validate Golden HH-MIDI")]
        public static void ValidateGoldenSample()
        {
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "examples/midi/headbang-heroes-example.hh.mid");
            if (!File.Exists(path)) throw new FileNotFoundException("Golden HH-MIDI sample not found.", path);
            var midi = MidiFileParser.Parse(File.ReadAllBytes(path));
            var result = HhMidiImporter.Import(midi, "golden-song", "golden-chart");
            foreach (var diagnostic in result.Diagnostics) Debug.Log("[HH-MIDI GOLDEN] " + diagnostic);
            if (result.HasErrors || result.Chart == null)
                throw new System.InvalidOperationException("Golden HH-MIDI sample failed validation.");
            Debug.Log($"[HH-MIDI GOLDEN] PASS events={result.Chart.events.Count} rests={result.Chart.rests.Count}");
        }
    }
}
#endif
