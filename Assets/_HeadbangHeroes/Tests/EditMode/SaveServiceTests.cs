using HeadbangHeroes.Meta;
using NUnit.Framework;
using UnityEngine;

namespace HeadbangHeroes.Tests
{
    public sealed class SaveServiceTests
    {
        sealed class MemoryStore : ISaveStore
        {
            public string primary;
            public string backup;
            public bool TryReadPrimary(out string json) { json = primary; return primary != null; }
            public bool TryReadBackup(out string json) { json = backup; return backup != null; }
            public void WritePrimary(string json) => primary = json;
            public void PromotePrimaryToBackup() { if (primary != null) backup = primary; }
        }

        [Test]
        public void NoSave_LoadReturnsValidDefaultProfile()
        {
            var svc = new SaveService(new MemoryStore());
            var p = svc.Load();
            Assert.IsNotNull(p);
            Assert.AreEqual(UserProfile.CurrentSchemaVersion, p.saveSchemaVersion);
            Assert.AreEqual(1, p.level);
            Assert.IsNotNull(p.records);
        }

        [Test]
        public void SaveThenLoad_RoundTripsProgressionSettingsCalibrationRecords()
        {
            var store = new MemoryStore();
            var svc = new SaveService(store);

            var p = UserProfile.CreateDefault();
            p.level = 4; p.xp = 1234; p.hhCurrency = 567;
            p.settings.musicVolume = 0.6f; p.settings.reducedFlash = true;
            p.calibration.audioOffsetMs = -30;
            p.avatar.hairId = "hair-mohawk";
            p.records.Add(new ChartRecord { songId="s", chartId="c", chartVersion=2, rulesVersion=1, bestScore=99999, longestCombo=42, bestGrade=(int)Grade.A });
            svc.Save(p);

            var loaded = svc.Load();
            Assert.AreEqual(4, loaded.level);
            Assert.AreEqual(1234, loaded.xp);
            Assert.AreEqual(567, loaded.hhCurrency);
            Assert.AreEqual(0.6f, loaded.settings.musicVolume, 1e-4f);
            Assert.IsTrue(loaded.settings.reducedFlash);
            Assert.AreEqual(-30, loaded.calibration.audioOffsetMs, 1e-6);
            Assert.AreEqual("hair-mohawk", loaded.avatar.hairId);
            Assert.AreEqual(1, loaded.records.Count);
            Assert.AreEqual(99999, loaded.records[0].bestScore);
            Assert.AreEqual(42, loaded.records[0].longestCombo);
        }

        [Test]
        public void MalformedPrimary_RecoversFromBackup()
        {
            var store = new MemoryStore();
            var good = UserProfile.CreateDefault();
            good.level = 7;
            store.backup = JsonUtility.ToJson(good);
            store.primary = "{ this is not valid json ]";

            var svc = new SaveService(store);
            var loaded = svc.Load();
            Assert.AreEqual(7, loaded.level, "recovered the known-good backup");
        }

        [Test]
        public void MalformedPrimaryAndBackup_ReturnsFreshDefault()
        {
            var store = new MemoryStore { primary = "garbage", backup = "also garbage {" };
            var svc = new SaveService(store);
            var loaded = svc.Load();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.level, "falls back to a fresh default rather than crashing");
        }

        [Test]
        public void Save_PromotesPreviousToBackup()
        {
            var store = new MemoryStore();
            var svc = new SaveService(store);
            var p = UserProfile.CreateDefault(); p.level = 2;
            svc.Save(p);
            var firstBlob = store.primary;
            p.level = 3;
            svc.Save(p);
            Assert.AreEqual(firstBlob, store.backup, "prior primary becomes the recoverable backup");
        }

        [Test]
        public void Migrate_NewerSchema_IsRejected()
        {
            var p = UserProfile.CreateDefault();
            p.saveSchemaVersion = UserProfile.CurrentSchemaVersion + 5;
            Assert.IsFalse(ProfileMigrator.TryMigrate(p, out _), "a save from a newer client is not blindly downgraded");
        }

        [Test]
        public void Migrate_FillsMissingDefaults()
        {
            var p = new UserProfile { saveSchemaVersion = 1, level = 0, xp = -5, records = null };
            Assert.IsTrue(ProfileMigrator.TryMigrate(p, out var m));
            Assert.AreEqual(1, m.level);
            Assert.AreEqual(0, m.xp);
            Assert.IsNotNull(m.records);
            Assert.IsFalse(string.IsNullOrEmpty(m.playerId));
            Assert.AreEqual("hair-default", m.avatar.hairId, "retired/empty cosmetic falls back gracefully");
        }
    }
}
