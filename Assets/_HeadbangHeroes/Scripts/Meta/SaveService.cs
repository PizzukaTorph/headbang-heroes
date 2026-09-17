using System;
using UnityEngine;

namespace HeadbangHeroes.Meta
{
    /// <summary>
    /// Raw persistence seam: reads/writes the primary blob and a previous-known-good backup.
    /// Keeping storage behind this interface lets SaveService be unit-tested with an in-memory
    /// store and lets a future ProfileSyncService sit here without gameplay changes.
    /// </summary>
    public interface ISaveStore
    {
        bool TryReadPrimary(out string json);
        bool TryReadBackup(out string json);
        void WritePrimary(string json);
        void PromotePrimaryToBackup(); // called before overwriting, to keep a recoverable copy
    }

    /// <summary>
    /// Local versioned profile persistence. Gameplay systems NEVER call this — only the meta flow.
    /// Load: read primary -> parse -> migrate -> validate; on failure, recover from backup; on total
    /// failure, return a fresh default profile (never crash the loop). Save: promote current primary
    /// to backup, then write validated JSON (previous-known-good recovery + validation-before-replace).
    /// </summary>
    public sealed class SaveService
    {
        readonly ISaveStore store;

        public SaveService(ISaveStore store) => this.store = store;

        /// <summary>Loads a usable profile, migrating/validating/recovering as needed. Never throws.</summary>
        public UserProfile Load()
        {
            if (TryLoadFrom(primary: true, out var p)) return p;
            if (TryLoadFrom(primary: false, out var b))
            {
                Debug.LogWarning("HH save: primary profile unreadable; recovered from backup.");
                return b;
            }
            Debug.LogWarning("HH save: no valid profile found; creating a fresh default.");
            return UserProfile.CreateDefault();
        }

        bool TryLoadFrom(bool primary, out UserProfile profile)
        {
            profile = null;
            var ok = primary ? store.TryReadPrimary(out var json) : store.TryReadBackup(out json);
            if (!ok || string.IsNullOrWhiteSpace(json)) return false;

            UserProfile parsed;
            try { parsed = JsonUtility.FromJson<UserProfile>(json); }
            catch { return false; }
            if (parsed == null) return false;

            if (!ProfileMigrator.TryMigrate(parsed, out var migrated)) return false;
            profile = migrated;
            return true;
        }

        /// <summary>Validates then persists the profile, keeping the prior version as recoverable backup.</summary>
        public void Save(UserProfile profile)
        {
            if (profile == null) return;
            ProfileMigrator.Validate(profile);
            profile.saveSchemaVersion = UserProfile.CurrentSchemaVersion;
            profile.updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var json = JsonUtility.ToJson(profile);
            // Validation-before-replace: only keep a backup if the new blob is non-empty/serializable.
            if (string.IsNullOrWhiteSpace(json)) return;

            store.PromotePrimaryToBackup();
            store.WritePrimary(json);
        }
    }

    /// <summary>File-backed store using the platform persistent data path. Atomic-ish via temp file.</summary>
    public sealed class FileSaveStore : ISaveStore
    {
        readonly string primaryPath;
        readonly string backupPath;

        public FileSaveStore(string fileName = "hh_profile.json")
        {
            var dir = Application.persistentDataPath;
            primaryPath = System.IO.Path.Combine(dir, fileName);
            backupPath = System.IO.Path.Combine(dir, fileName + ".bak");
        }

        public bool TryReadPrimary(out string json) => TryRead(primaryPath, out json);
        public bool TryReadBackup(out string json) => TryRead(backupPath, out json);

        static bool TryRead(string path, out string json)
        {
            json = null;
            try
            {
                if (!System.IO.File.Exists(path)) return false;
                json = System.IO.File.ReadAllText(path);
                return true;
            }
            catch { return false; }
        }

        public void WritePrimary(string json)
        {
            try
            {
                var tmp = primaryPath + ".tmp";
                System.IO.File.WriteAllText(tmp, json);
                if (System.IO.File.Exists(primaryPath)) System.IO.File.Delete(primaryPath);
                System.IO.File.Move(tmp, primaryPath);   // atomic-ish replace
            }
            catch (Exception e) { Debug.LogError($"HH save write failed: {e.Message}"); }
        }

        public void PromotePrimaryToBackup()
        {
            try
            {
                if (!System.IO.File.Exists(primaryPath)) return;
                if (System.IO.File.Exists(backupPath)) System.IO.File.Delete(backupPath);
                System.IO.File.Copy(primaryPath, backupPath);
            }
            catch { /* backup is best-effort; never block a save */ }
        }
    }
}
