using System;
using System.Collections.Generic;

namespace HeadbangHeroes.Meta
{
    /// <summary>Persisted presentation/accessibility settings. Never change scoring potential.</summary>
    [Serializable]
    public struct ProfileSettings
    {
        public float musicVolume;
        public bool hapticsEnabled;
        public float hapticIntensity;
        public bool reducedFlash;
        public bool reducedShake;

        public static ProfileSettings Default => new ProfileSettings
        {
            musicVolume = 1f,
            hapticsEnabled = true,
            hapticIntensity = 1f,
            reducedFlash = false,
            reducedShake = false
        };
    }

    /// <summary>
    /// Persisted timing calibration. Aligns perception/input timing only; it is explicit and
    /// auditable and MUST NOT widen judgment windows (SAVE_PROFILE_V1). Values in milliseconds.
    /// </summary>
    [Serializable]
    public struct Calibration
    {
        public double audioOffsetMs;
        public double inputOffsetMs;
        public double visualOffsetMs;

        public static Calibration Default => new Calibration
        {
            audioOffsetMs = 0,
            inputOffsetMs = 0,
            visualOffsetMs = 0
        };

        /// <summary>Combined offset applied to the authoritative clock (seconds). Not a window widener.</summary>
        public double CombinedSeconds => (audioOffsetMs + inputOffsetMs) / 1000.0;
    }

    /// <summary>Semantic (presentation-only) avatar selections. Cosmetic; never affect gameplay.</summary>
    [Serializable]
    public struct AvatarSelections
    {
        public string presentation;   // Male/Female/Other
        public string faceId;
        public string hairId;
        public string performanceStyleId;
        public string idleStyleId;

        public static AvatarSelections Default => new AvatarSelections
        {
            presentation = "Other",
            faceId = "face-default",
            hairId = "hair-default",
            performanceStyleId = "style-default",
            idleStyleId = "idle-default"
        };
    }

    /// <summary>
    /// Local player profile — the persisted description of the PLAYER (not the song catalog).
    /// Serializable via JsonUtility. <see cref="saveSchemaVersion"/> drives deterministic migration.
    /// Progression stores XP/level/HH; nothing here grants gameplay power.
    /// </summary>
    [Serializable]
    public sealed class UserProfile
    {
        public int saveSchemaVersion;
        public string playerId;
        public string displayName;

        public int level;
        public long xp;
        public long hhCurrency;

        public AvatarSelections avatar;
        public ProfileSettings settings;
        public Calibration calibration;
        public List<ChartRecord> records;

        public long updatedAt;   // unix seconds; set by SaveService on write

        public const int CurrentSchemaVersion = 1;

        public static UserProfile CreateDefault()
        {
            return new UserProfile
            {
                saveSchemaVersion = CurrentSchemaVersion,
                playerId = Guid.NewGuid().ToString("N"),
                displayName = "Headbanger",
                level = 1,
                xp = 0,
                hhCurrency = 0,
                avatar = AvatarSelections.Default,
                settings = ProfileSettings.Default,
                calibration = Calibration.Default,
                records = new List<ChartRecord>(),
                updatedAt = 0
            };
        }

        /// <summary>Finds the record for a chart identity, or null.</summary>
        public int FindRecordIndex(string songId, string chartId, int chartVersion, int rulesVersion)
        {
            if (records == null) return -1;
            for (var i = 0; i < records.Count; i++)
                if (records[i].Matches(songId, chartId, chartVersion, rulesVersion)) return i;
            return -1;
        }
    }
}
