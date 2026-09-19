using System.Collections.Generic;
using HeadbangHeroes.Meta;
using NUnit.Framework;

namespace HeadbangHeroes.Tests
{
    public sealed class ResultCommentSelectorTests
    {
        static ResultComment[] Rules => new[]
        {
            new ResultComment("base_any", Grade.D, Grade.S, 0, "base"),
            new ResultComment("s_flavor", Grade.S, Grade.S, 10, "s_flavor"),
            new ResultComment("zeromiss", Grade.A, Grade.S, 30, "zeromiss",
                requiredTags: new[] { ResultTag.ZeroMiss }),
            new ResultComment("clean_no_chaos", Grade.B, Grade.S, 20, "clean",
                excludedTags: new[] { ResultTag.ChaoticRun }),
        };

        static HashSet<ResultTag> Tags(params ResultTag[] t) => new HashSet<ResultTag>(t);

        [Test]
        public void PicksHighestPriorityMatch()
        {
            var ok = ResultCommentSelector.Select(Rules, Grade.S, Tags(ResultTag.ZeroMiss), out var c);
            Assert.IsTrue(ok);
            Assert.AreEqual("zeromiss", c.Id, "priority 30 required-tag rule beats base/flavor");
        }

        [Test]
        public void GradeOutOfRange_RuleIgnored()
        {
            // zeromiss requires grade >= A; at B with ZeroMiss it must NOT be chosen.
            var ok = ResultCommentSelector.Select(Rules, Grade.B, Tags(ResultTag.ZeroMiss), out var c);
            Assert.IsTrue(ok);
            Assert.AreNotEqual("zeromiss", c.Id);
            Assert.AreEqual("clean_no_chaos", c.Id, "at B without chaos, the priority-20 clean rule wins over base");
        }

        [Test]
        public void ExcludedTag_BlocksRule()
        {
            var ok = ResultCommentSelector.Select(Rules, Grade.B, Tags(ResultTag.ChaoticRun), out var c);
            Assert.IsTrue(ok);
            Assert.AreEqual("base", c.TextKey, "clean rule excluded by ChaoticRun -> only base matches");
        }

        [Test]
        public void RequiredTagMissing_RuleIgnored()
        {
            // zeromiss requires ZeroMiss (absent) -> ignored. Among the rest at S with no tags,
            // clean_no_chaos (prio 20, excludes ChaoticRun which is absent) beats s_flavor (prio 10).
            var ok = ResultCommentSelector.Select(Rules, Grade.S, Tags(), out var c);
            Assert.IsTrue(ok);
            Assert.AreNotEqual("zeromiss", c.Id, "the required-tag rule must be ignored when its tag is absent");
            Assert.AreEqual("clean_no_chaos", c.Id, "highest-priority still-matching rule wins");
        }

        [Test]
        public void NoMatch_ReturnsFalse()
        {
            var only = new[] { new ResultComment("s_only", Grade.S, Grade.S, 5, "s") };
            var ok = ResultCommentSelector.Select(only, Grade.D, Tags(), out _);
            Assert.IsFalse(ok, "no rule covers grade D");
        }

        [Test]
        public void Deterministic_SameInputsSameChoice()
        {
            ResultCommentSelector.Select(Rules, Grade.S, Tags(ResultTag.ZeroMiss), out var a);
            ResultCommentSelector.Select(Rules, Grade.S, Tags(ResultTag.ZeroMiss), out var b);
            Assert.AreEqual(a.Id, b.Id);
        }

        // ---- Default rule set + copy dictionary ----

        [Test]
        public void DefaultRules_EveryGradeHasAFallbackLine()
        {
            foreach (var g in new[] { Grade.D, Grade.C, Grade.B, Grade.A, Grade.S })
            {
                var line = ResultCommentSelector.SelectLine(g, Tags());
                Assert.IsNotEmpty(line, $"grade {g} must always yield a non-empty line");
            }
        }

        [Test]
        public void DefaultRules_ZeroMissOnS_GivesFlavourNotBase()
        {
            var line = ResultCommentSelector.SelectLine(Grade.S, Tags(ResultTag.ZeroMiss));
            Assert.AreEqual(ResultCommentSelector.Text("zero_miss"), line);
        }

        [Test]
        public void Text_MissingKeyReturnsKey()
        {
            Assert.AreEqual("no_such_key", ResultCommentSelector.Text("no_such_key"));
        }
    }
}
