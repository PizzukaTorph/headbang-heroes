using System.Collections.Generic;

namespace HeadbangHeroes.Meta
{
    /// <summary>
    /// A data-driven result-commentary rule (RESULTS_SCREEN_V1). A comment applies when the grade is
    /// within [MinGrade, MaxGrade], ALL RequiredTags are present, and NONE of ExcludedTags are. The
    /// highest <see cref="Priority"/> matching rule wins (ties broken by declaration order). Copy is a
    /// <see cref="TextKey"/> resolved separately, so text stays content and never couples to scoring.
    /// Pure data — no Unity refs.
    /// </summary>
    public readonly struct ResultComment
    {
        public readonly string Id;
        public readonly Grade MinGrade;
        public readonly Grade MaxGrade;
        public readonly ResultTag[] RequiredTags;
        public readonly ResultTag[] ExcludedTags;
        public readonly int Priority;
        public readonly string TextKey;

        public ResultComment(string id, Grade minGrade, Grade maxGrade, int priority, string textKey,
            ResultTag[] requiredTags = null, ResultTag[] excludedTags = null)
        {
            Id = id;
            MinGrade = minGrade;
            MaxGrade = maxGrade;
            Priority = priority;
            TextKey = textKey;
            RequiredTags = requiredTags ?? System.Array.Empty<ResultTag>();
            ExcludedTags = excludedTags ?? System.Array.Empty<ResultTag>();
        }
    }

    /// <summary>
    /// Deterministic selection of the best <see cref="ResultComment"/> for a grade + tag set, and a
    /// simple textKey->string dictionary (English placeholder copy for the prototype; a real
    /// localization layer is future work). Pure and unit-testable.
    /// </summary>
    public static class ResultCommentSelector
    {
        // Grade rank for range checks (D lowest .. S highest), independent of enum underlying value.
        static int Rank(Grade g) => g switch
        {
            Grade.D => 0, Grade.C => 1, Grade.B => 2, Grade.A => 3, Grade.S => 4, _ => 0
        };

        /// <summary>
        /// Picks the highest-priority rule whose grade range + required/excluded tags match. Returns
        /// false if none match (caller should fall back). Deterministic: stable by (priority desc,
        /// declaration order).
        /// </summary>
        public static bool Select(IReadOnlyList<ResultComment> rules, Grade grade,
            IReadOnlyCollection<ResultTag> tags, out ResultComment chosen)
        {
            chosen = default;
            var have = false;
            var gr = Rank(grade);
            for (var i = 0; i < rules.Count; i++)
            {
                var r = rules[i];
                if (gr < Rank(r.MinGrade) || gr > Rank(r.MaxGrade)) continue;
                if (!HasAll(tags, r.RequiredTags)) continue;
                if (HasAny(tags, r.ExcludedTags)) continue;
                if (!have || r.Priority > chosen.Priority) { chosen = r; have = true; }
            }
            return have;
        }

        static bool HasAll(IReadOnlyCollection<ResultTag> tags, ResultTag[] required)
        {
            foreach (var t in required) if (!Contains(tags, t)) return false;
            return true;
        }

        static bool HasAny(IReadOnlyCollection<ResultTag> tags, ResultTag[] excluded)
        {
            foreach (var t in excluded) if (Contains(tags, t)) return true;
            return false;
        }

        static bool Contains(IReadOnlyCollection<ResultTag> tags, ResultTag t)
        {
            foreach (var x in tags) if (x == t) return true;
            return false;
        }

        // ---- Default prototype rule set (English placeholder copy; personality per grade + tags). ----
        // Higher priority = more specific / more flavourful. A per-grade fallback (priority 0) always
        // matches so the screen never shows an empty line.
        public static readonly IReadOnlyList<ResultComment> DefaultRules = new List<ResultComment>
        {
            // Grade fallbacks (always match their grade; lowest priority).
            new("s_base", Grade.S, Grade.S, 0, "s_base"),
            new("a_base", Grade.A, Grade.A, 0, "a_base"),
            new("b_base", Grade.B, Grade.B, 0, "b_base"),
            new("c_base", Grade.C, Grade.C, 0, "c_base"),
            new("d_base", Grade.D, Grade.D, 0, "d_base"),

            // Tag-conditional flavour (higher priority overrides the base line).
            new("zero_miss_high", Grade.A, Grade.S, 30, "zero_miss",
                requiredTags: new[]{ ResultTag.ZeroMiss }),
            new("many_perfects", Grade.B, Grade.S, 25, "many_perfects",
                requiredTags: new[]{ ResultTag.ManyPerfects }),
            new("multi_finisher", Grade.B, Grade.S, 28, "multi_finisher",
                requiredTags: new[]{ ResultTag.MultipleFinishers }),
            new("huge_combo", Grade.B, Grade.S, 22, "huge_combo",
                requiredTags: new[]{ ResultTag.HugeCombo }),
            new("high_hype", Grade.C, Grade.S, 18, "high_hype",
                requiredTags: new[]{ ResultTag.HighHype }),
            // Low grade but one heroic finisher — reward the moment (RESULTS_SCREEN_V1 example).
            new("heroic_in_chaos", Grade.D, Grade.C, 26, "heroic_in_chaos",
                requiredTags: new[]{ ResultTag.MultipleFinishers }),
            new("chaotic", Grade.D, Grade.C, 20, "chaotic",
                requiredTags: new[]{ ResultTag.ChaoticRun }),
            new("no_thebang_lowmid", Grade.C, Grade.B, 15, "no_thebang",
                requiredTags: new[]{ ResultTag.NoTheBang }),
        };

        static readonly Dictionary<string, string> Copy = new()
        {
            ["s_base"] = "The venue may never structurally recover.",
            ["a_base"] = "Almost enough violence. Almost.",
            ["b_base"] = "Respectable. The pit remains unconvinced.",
            ["c_base"] = "Technically metal.",
            ["d_base"] = "Your neck has filed for resignation.",
            ["zero_miss"] = "Not a single miss. Neck integrity: questionable.",
            ["many_perfects"] = "Surgical. Every strike landed clean.",
            ["multi_finisher"] = "Multiple finishers. The crowd has lost its mind.",
            ["huge_combo"] = "That combo bordered on a felony.",
            ["high_hype"] = "The energy nearly tore the roof off.",
            ["heroic_in_chaos"] = "A mess — but that one finisher was legend.",
            ["chaotic"] = "Chaos incarnate. Barely a rhythm survived.",
            ["no_thebang"] = "You never even unleashed THE BANG. Coward.",
        };

        /// <summary>Resolve a text key to placeholder English copy. Missing keys return the key itself.</summary>
        public static string Text(string key) => Copy.TryGetValue(key, out var s) ? s : key;

        /// <summary>Convenience: select + resolve in one call, with a safe fallback string.</summary>
        public static string SelectLine(Grade grade, IReadOnlyCollection<ResultTag> tags)
            => Select(DefaultRules, grade, tags, out var c) ? Text(c.TextKey) : "";
    }
}
