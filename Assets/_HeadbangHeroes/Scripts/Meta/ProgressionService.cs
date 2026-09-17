using System.Collections.Generic;
using HeadbangHeroes.Gameplay.Scoring;

namespace HeadbangHeroes.Meta
{
    /// <summary>
    /// Data-driven progression tuning (PROGRESSION_V1). XP = BaseSongXP × grade modifier; HH from
    /// base + milestone bonuses. Level thresholds are cumulative XP required to reach level N+1.
    /// All values are tuning; progression grants EXPRESSION/CONTENT, never gameplay power.
    /// </summary>
    public readonly struct ProgressionConfig
    {
        public readonly long BaseSongXp;
        public readonly float DModifier, CModifier, BModifier, AModifier, SModifier;
        public readonly long BaseHh;
        public readonly long FirstClearBonusHh;
        public readonly long FirstSBonusHh;
        public readonly long NewBestBonusHh;
        public readonly long XpPerLevel;   // simple linear threshold for POC

        public ProgressionConfig(
            long baseSongXp,
            float dMod, float cMod, float bMod, float aMod, float sMod,
            long baseHh, long firstClearBonusHh, long firstSBonusHh, long newBestBonusHh,
            long xpPerLevel)
        {
            BaseSongXp = baseSongXp;
            DModifier = dMod; CModifier = cMod; BModifier = bMod; AModifier = aMod; SModifier = sMod;
            BaseHh = baseHh;
            FirstClearBonusHh = firstClearBonusHh;
            FirstSBonusHh = firstSBonusHh;
            NewBestBonusHh = newBestBonusHh;
            XpPerLevel = xpPerLevel < 1 ? 1 : xpPerLevel;
        }

        public static ProgressionConfig Default => new ProgressionConfig(
            baseSongXp: 100,
            dMod: 0.70f, cMod: 0.85f, bMod: 1.00f, aMod: 1.20f, sMod: 1.50f,
            baseHh: 50,
            firstClearBonusHh: 100,
            firstSBonusHh: 150,
            newBestBonusHh: 40,
            xpPerLevel: 500);

        public float GradeXpModifier(Grade grade)
        {
            switch (grade)
            {
                case Grade.S: return SModifier;
                case Grade.A: return AModifier;
                case Grade.B: return BModifier;
                case Grade.C: return CModifier;
                default: return DModifier;
            }
        }
    }

    /// <summary>Deterministic result of applying one run's progression (for the Results reveal).</summary>
    public readonly struct ProgressionOutcome
    {
        public readonly long XpEarned;
        public readonly long HhEarned;
        public readonly int LevelBefore;
        public readonly int LevelAfter;
        public readonly bool NewRecord;
        public readonly bool FirstClear;
        public readonly bool FirstS;

        public bool LeveledUp => LevelAfter > LevelBefore;

        public ProgressionOutcome(long xp, long hh, int levelBefore, int levelAfter, bool newRecord, bool firstClear, bool firstS)
        {
            XpEarned = xp;
            HhEarned = hh;
            LevelBefore = levelBefore;
            LevelAfter = levelAfter;
            NewRecord = newRecord;
            FirstClear = firstClear;
            FirstS = firstS;
        }
    }

    /// <summary>
    /// Pure progression application. Consumes an authoritative <see cref="RunResult"/> + its
    /// <see cref="GradeResult"/> and MUTATES the given profile in place (xp/level/hh + version-aware
    /// record update), returning a deterministic <see cref="ProgressionOutcome"/> for the Results
    /// reveal. It never participates in gameplay and grants no gameplay power.
    /// </summary>
    public static class ProgressionService
    {
        public static ProgressionOutcome Apply(UserProfile profile, in RunResult run, in GradeResult grade, in ProgressionConfig config)
        {
            var levelBefore = profile.level;

            // XP.
            var xp = (long)(config.BaseSongXp * config.GradeXpModifier(grade.Grade));

            // Record update (version-aware) + first-clear / first-S / new-best detection.
            var idx = profile.FindRecordIndex(run.SongId, run.ChartId, run.ChartVersion, run.RulesVersion);
            var isFirstClear = idx < 0;
            var newRecord = false;
            var firstS = false;

            var incoming = new ChartRecord
            {
                songId = run.SongId,
                chartId = run.ChartId,
                chartVersion = run.ChartVersion,
                rulesVersion = run.RulesVersion,
                bestScore = run.Score,
                bestGrade = (int)grade.Grade,
                longestCombo = run.LongestCombo,
                bestFinisherCount = run.FinishersExecuted,
                firstClear = true,
                firstS = grade.Grade == Grade.S
            };

            if (idx < 0)
            {
                profile.records ??= new List<ChartRecord>();
                profile.records.Add(incoming);
                newRecord = true;
                firstS = grade.Grade == Grade.S;
            }
            else
            {
                var existing = profile.records[idx];
                newRecord = run.Score > existing.bestScore;
                firstS = grade.Grade == Grade.S && !existing.firstS;
                profile.records[idx] = ChartRecord.MergeBest(existing, incoming);
            }

            // HH: base + milestone bonuses.
            var hh = config.BaseHh;
            if (isFirstClear) hh += config.FirstClearBonusHh;
            if (firstS) hh += config.FirstSBonusHh;
            if (newRecord && !isFirstClear) hh += config.NewBestBonusHh;

            profile.xp += xp;
            profile.hhCurrency += hh;

            // Simple linear leveling for POC: level = 1 + floor(xp / xpPerLevel).
            var levelAfter = 1 + (int)(profile.xp / config.XpPerLevel);
            profile.level = levelAfter;

            return new ProgressionOutcome(xp, hh, levelBefore, levelAfter, newRecord, isFirstClear, firstS);
        }
    }
}
