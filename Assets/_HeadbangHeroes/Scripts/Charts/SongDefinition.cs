using UnityEngine;

namespace HeadbangHeroes.Charts
{
    [CreateAssetMenu(menuName = "Headbang Heroes/Song", fileName = "Song_")]
    public sealed class SongDefinition : ScriptableObject
    {
        public string songId = "lab-001";
        public string title = "HH Lab Track #001";
        public string artist = "Asidie";
        public AudioClip audio;
        // Chart authoring lives in the chart JSON (compiled to an immutable RuntimeChart at load).
    }
}
