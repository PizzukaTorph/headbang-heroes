using System.Collections.Generic;

namespace HeadbangHeroes.Meta
{
    /// <summary>
    /// Deterministic, monotonic, narrow profile migration by <c>saveSchemaVersion</c>. Load flow is
    /// read -> migrate forward if supported -> validate. Never assumes the save was written by the
    /// current build. Unit-tested. When new schema versions arrive, add a numbered step and bump
    /// <see cref="UserProfile.CurrentSchemaVersion"/>.
    /// </summary>
    public static class ProfileMigrator
    {
        /// <summary>
        /// Migrates <paramref name="profile"/> forward to the current schema, filling any missing
        /// defaults, and returns a validated profile. A profile newer than this client is rejected
        /// (returns false) rather than silently downgraded.
        /// </summary>
        public static bool TryMigrate(UserProfile profile, out UserProfile migrated)
        {
            migrated = null;
            if (profile == null) return false;

            if (profile.saveSchemaVersion > UserProfile.CurrentSchemaVersion)
                return false; // save is from a newer client; do not blindly downgrade

            // Forward migration steps would go here, e.g.:
            //   if (profile.saveSchemaVersion < 2) { /* v1 -> v2 */ profile.saveSchemaVersion = 2; }
            // For v1 there are no prior versions; just normalize.

            profile.saveSchemaVersion = UserProfile.CurrentSchemaVersion;
            Validate(profile);
            migrated = profile;
            return true;
        }

        /// <summary>Fills missing/invalid fields with safe defaults so a partial save stays usable.</summary>
        public static void Validate(UserProfile p)
        {
            if (string.IsNullOrEmpty(p.playerId)) p.playerId = System.Guid.NewGuid().ToString("N");
            if (string.IsNullOrEmpty(p.displayName)) p.displayName = "Headbanger";
            if (p.level < 1) p.level = 1;
            if (p.xp < 0) p.xp = 0;
            if (p.hhCurrency < 0) p.hhCurrency = 0;
            if (p.records == null) p.records = new List<ChartRecord>();

            // Settings sanity (accessibility flags never affect scoring; clamp presentation ranges).
            if (p.settings.musicVolume < 0f) p.settings.musicVolume = 0f;
            if (p.settings.musicVolume > 1f) p.settings.musicVolume = 1f;
            if (p.settings.hapticIntensity < 0f) p.settings.hapticIntensity = 0f;
            if (p.settings.hapticIntensity > 1f) p.settings.hapticIntensity = 1f;

            // Avatar cosmetic fallback: retired/empty ids fall back gracefully (never invalidate profile).
            if (string.IsNullOrEmpty(p.avatar.presentation)) p.avatar.presentation = "Other";
            if (string.IsNullOrEmpty(p.avatar.faceId)) p.avatar.faceId = "face-default";
            if (string.IsNullOrEmpty(p.avatar.hairId)) p.avatar.hairId = "hair-default";
            if (string.IsNullOrEmpty(p.avatar.performanceStyleId)) p.avatar.performanceStyleId = "style-default";
            if (string.IsNullOrEmpty(p.avatar.idleStyleId)) p.avatar.idleStyleId = "idle-default";
        }
    }
}
