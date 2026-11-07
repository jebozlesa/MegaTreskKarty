using System.Collections.Generic;
using System.IO;
using System;
using NUnit.Framework;
using UnityEngine;

public class BattleSharedDamagePlaybackTests
{
    [TestCase(71, 19, true, false)]
    [TestCase(71, 19, false, false)]
    [TestCase(71, 21, true, false)]
    [TestCase(71, 21, false, false)]
    [TestCase(64, 9, true, true)]
    [TestCase(64, 9, false, true)]
    [TestCase(64, 12, true, false)]
    [TestCase(64, 12, false, false)]
    [TestCase(3, 19, true, false)]
    [TestCase(3, 19, false, false)]
    [TestCase(10, 3, true, false)]
    [TestCase(10, 3, false, false)]
    [TestCase(26, 3, true, false)]
    [TestCase(26, 3, false, false)]
    public void CausalCleanupFollowsCastBeforeRollbackAndSecondaryEffect(
        int attackId, int effectType, bool casterIsPlayer, bool confusion)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        var root = new GameObject("CausalCleanup", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            Component Card(string id)
            {
                var child = new GameObject(id, typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
                var card = child.AddComponent(cardType);
                cardType.GetField("cardId").SetValue(card, id);
                cardType.GetField("cardName").SetValue(card, id);
                cardType.GetProperty("health").SetValue(card, 20);
                cardType.GetProperty("maxHealth").SetValue(card, 20);
                return card;
            }
            var caster = Card("caster");
            var target = Card("target");
            var context = Activator.CreateInstance(contextType);
            contextType.GetField("PlayerCard").SetValue(context, casterIsPlayer ? caster : target);
            contextType.GetField("EnemyCard").SetValue(context, casterIsPlayer ? target : caster);
            contextType.GetField("PlayerCardId").SetValue(context, casterIsPlayer ? "caster" : "target");
            contextType.GetField("EnemyCardId").SetValue(context, casterIsPlayer ? "target" : "caster");
            var attack = root.AddComponent(runtime.GetType("Attack", true));
            attack.GetType().GetField("attackAnimations").SetValue(attack,
                root.AddComponent(runtime.GetType("AttackAnimations", true)));
            contextType.GetField("AttackComponent").SetValue(context, attack);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            object Step(string kind, string targetId)
            {
                var step = Activator.CreateInstance(stepType);
                stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, kind));
                stepType.GetField("ActorCardId").SetValue(step, "caster");
                stepType.GetField("TargetCardId").SetValue(step, targetId);
                steps.Add(step);
                return step;
            }
            var cast = Step("Attack", "target");
            stepType.GetField("AttackId").SetValue(cast, attackId);
            stepType.GetField("AttackResult").SetValue(cast, "escaped");
            if (confusion) stepType.GetField("EffectsApplied").SetValue(cast,
                new List<Dictionary<string, object>> { new Dictionary<string, object> { { "type", 17 }, { "duration", 2 } } });
            string cleansedId = attackId == 10 ? "target" : "caster";
            var removal = Step("EffectRemoved", cleansedId);
            stepType.GetField("EffectType").SetValue(removal, effectType);
            stepType.GetField("RemovalReason").SetValue(removal, "cleansed");
            var rollback = Step("StatChange", cleansedId);
            stepType.GetField("StatName").SetValue(rollback, "STR");
            stepType.GetField("Amount").SetValue(rollback, 2);
            if (confusion)
            {
                var applied = Step("EffectApplied", "target");
                stepType.GetField("AttackId").SetValue(applied, attackId);
                stepType.GetField("EffectType").SetValue(applied, 17);
                stepType.GetField("Duration").SetValue(applied, 2);
            }
            string castName = attackId == 71 ? "<PlayBattleCryAnimation>"
                : attackId == 64 ? "<PlayHandcuffEscapeAnimation>"
                : attackId == 3 ? "<PlayHealAnimation>"
                : attackId == 10 ? "<PlayScratchAnimation>" : "<PlayBoostAnimation>";
            var trace = new List<string>();
            void Drain(System.Collections.IEnumerator routine)
            {
                string name = routine.GetType().Name;
                if (name.Contains(castName)) { trace.Add("cast"); return; }
                if (name.Contains("<RemoveEndedEffectIcon>"))
                {
                    CollectionAssert.AreEqual(new[] { "cast" }, trace);
                    trace.Add("cleanup");
                    return;
                }
                if (name.Contains("<PlayTimelineStatChange>"))
                {
                    CollectionAssert.AreEqual(new[] { "cast", "cleanup" }, trace);
                    trace.Add("rollback");
                    return;
                }
                if (name.Contains("<PlayConfusionStartAnimation>"))
                {
                    CollectionAssert.AreEqual(new[] { "cast", "cleanup", "rollback" }, trace);
                    trace.Add("confusion");
                    return;
                }
                if (name.Contains("<AddEffectIconOnly>")) { trace.Add("icon"); return; }
                while (routine.MoveNext())
                    if (routine.Current is System.Collections.IEnumerator nested) Drain(nested);
            }
            Drain((System.Collections.IEnumerator)runtime.GetType("BattleTimelinePlayback", true).GetMethod("Play")
                .Invoke(null, new object[] { context, steps }));
            CollectionAssert.AreEqual(confusion
                ? new[] { "cast", "cleanup", "rollback", "confusion", "icon" }
                : new[] { "cast", "cleanup", "rollback" }, trace);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [TestCase(75, true, 18)]
    [TestCase(75, false, 18)]
    [TestCase(75, true, 20)]
    [TestCase(75, false, 20)]
    [TestCase(115, true, 18)]
    [TestCase(115, false, 18)]
    [TestCase(115, true, 20)]
    [TestCase(115, false, 20)]
    public void TargetHealTimelineHasOneHandlerValueOwner(int attackId, bool casterIsPlayer, int targetHealth)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        var root = new GameObject("TargetHealOwner", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            Component Card(string id, int health)
            {
                var child = new GameObject(id, typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
                var card = child.AddComponent(cardType);
                cardType.GetField("cardId").SetValue(card, id);
                cardType.GetField("cardName").SetValue(card, id);
                cardType.GetProperty("health").SetValue(card, health);
                cardType.GetProperty("maxHealth").SetValue(card, 20);
                return card;
            }
            var caster = Card("poet", 9);
            var target = Card("reader", targetHealth);
            var context = Activator.CreateInstance(contextType);
            contextType.GetField("PlayerCard").SetValue(context, casterIsPlayer ? caster : target);
            contextType.GetField("EnemyCard").SetValue(context, casterIsPlayer ? target : caster);
            contextType.GetField("PlayerCardId").SetValue(context, casterIsPlayer ? "poet" : "reader");
            contextType.GetField("EnemyCardId").SetValue(context, casterIsPlayer ? "reader" : "poet");
            contextType.GetField("CardAnimator").SetValue(context, root.AddComponent(runtime.GetType("MultiplayerCardAnimator", true)));
            Type attackType = runtime.GetType("Attack", true);
            var attack = root.AddComponent(attackType);
            attackType.GetField("attackAnimations").SetValue(attack, root.AddComponent(runtime.GetType("AttackAnimations", true)));
            contextType.GetField("AttackComponent").SetValue(context, attack);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            object Step(string kind)
            {
                var step = Activator.CreateInstance(stepType);
                stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, kind));
                stepType.GetField("ActorCardId").SetValue(step, "poet");
                stepType.GetField("TargetCardId").SetValue(step, "reader");
                stepType.GetField("Source").SetValue(step, "attack");
                steps.Add(step);
                return step;
            }
            var cast = Step("Attack");
            stepType.GetField("AttackId").SetValue(cast, attackId);
            stepType.GetField("AttackResult").SetValue(cast, attackId == 75 ? "liked-it" : "inspired");
            if (targetHealth < 20) stepType.GetField("Amount").SetValue(Step("Heal"), 1);
            int Health(Component card) => (int)cardType.GetProperty("health").GetValue(card);
            var trace = new List<string>();
            string castVisual = attackId == 75 ? "<PlayIambicPentameterAnimation>" : "<PlayCubismAnimation>";
            void Drain(System.Collections.IEnumerator routine)
            {
                string name = routine.GetType().Name;
                if (name.Contains(castVisual))
                {
                    Assert.AreEqual(targetHealth, Health(target));
                    trace.Add("cast");
                    return;
                }
                if (name.Contains("<PlayHeal>"))
                {
                    CollectionAssert.AreEqual(new[] { "cast" }, trace);
                    Assert.AreEqual(!casterIsPlayer, routine.GetType().GetField("isMyCard").GetValue(routine));
                    trace.Add("heal");
                }
                if (name.Contains("<AnimateHeal>"))
                {
                    Assert.AreEqual(Math.Min(20, targetHealth + 1), Health(target));
                    trace.Add("number");
                    return;
                }
                while (routine.MoveNext())
                    if (routine.Current is System.Collections.IEnumerator nested) Drain(nested);
            }
            Drain((System.Collections.IEnumerator)runtime.GetType("BattleTimelinePlayback", true).GetMethod("Play")
                .Invoke(null, new object[] { context, steps }));
            CollectionAssert.AreEqual(attackId == 75 || targetHealth < 20
                ? new[] { "cast", "heal", "number" } : new[] { "cast" }, trace);
            Assert.AreEqual(Math.Min(20, targetHealth + 1), Health(target));
            Assert.AreEqual(9, Health(caster));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [TestCase("caster", "caster", "attack", false, 3)]
    [TestCase("caster", "target", "attack", false, 3)]
    [TestCase("other", "caster", "attack", false, 0)]
    [TestCase("caster", "caster", "famine", false, 0)]
    [TestCase("caster", "caster", "attack", true, 0)]
    public void AttackHealReaderUsesActorAndStopsAtNextAttack(string actor, string target, string source, bool nextAttack, int expected)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
        void Add(string kind, string actorId, string targetId, string valueSource)
        {
            var step = Activator.CreateInstance(stepType);
            stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, kind));
            stepType.GetField("ActorCardId").SetValue(step, actorId);
            stepType.GetField("TargetCardId").SetValue(step, targetId);
            stepType.GetField("Source").SetValue(step, valueSource);
            stepType.GetField("Amount").SetValue(step, 3);
            steps.Add(step);
        }
        Add("Attack", "caster", "target", "attack");
        if (nextAttack) Add("Attack", "other", "caster", "attack");
        Add("Heal", actor, target, source);
        var read = runtime.GetType("BattleTimelinePlayback", true).GetMethod("GetAttackHealFromTimeline",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.IsNotNull(read);
        Assert.AreEqual(expected, read.Invoke(null, new object[] { steps, 0, "caster" }));
    }

    [TestCase(1, true, 8, true)]
    [TestCase(1, false, 1, true)]
    [TestCase(16, true, 8, true)]
    [TestCase(16, false, 1, false)]
    [TestCase(4, true, 8, true)]
    [TestCase(4, false, 1, true)]
    [TestCase(24, true, 8, false)]
    [TestCase(24, false, 1, true)]
    public void TypedPassiveDamageAwaitsEachAnimationThenChangesHpOnce(int effectType, bool isPlayer, int health, bool withAnimations)
    {
        VerifyPassiveValuePlayback(effectType, isPlayer, health, withAnimations);
    }

    [TestCase(true, 9)]
    [TestCase(false, 10)]
    public void TypedFamineKeepsContinueAtFullHealthWithoutZeroHealAnimation(bool isPlayer, int health)
    {
        VerifyPassiveValuePlayback(7, isPlayer, health, true);
    }

    private static void VerifyPassiveValuePlayback(int effectType, bool isPlayer, int health, bool withAnimations)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        var root = new GameObject("PassiveValueOrder", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            var card = root.AddComponent(cardType);
            cardType.GetField("cardId").SetValue(card, "subject");
            cardType.GetField("cardName").SetValue(card, "Subject");
            cardType.GetProperty("health").SetValue(card, health);
            cardType.GetProperty("maxHealth").SetValue(card, 10);
            var context = Activator.CreateInstance(contextType);
            contextType.GetField(isPlayer ? "PlayerCard" : "EnemyCard").SetValue(context, card);
            contextType.GetField("PlayerCardId").SetValue(context, isPlayer ? "subject" : "observer");
            contextType.GetField("EnemyCardId").SetValue(context, isPlayer ? "observer" : "subject");
            contextType.GetField("CardAnimator").SetValue(context, root.AddComponent(runtime.GetType("MultiplayerCardAnimator", true)));
            contextType.GetField("FinalDelaySeconds").SetValue(context, 0f);
            Type textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
            var text = root.AddComponent(textType);
            contextType.GetField("DialogText").SetValue(context, text);
            if (withAnimations)
            {
                Type attackType = runtime.GetType("Attack", true);
                var attack = root.AddComponent(attackType);
                attackType.GetField("attackAnimations").SetValue(attack, root.AddComponent(runtime.GetType("AttackAnimations", true)));
                contextType.GetField("AttackComponent").SetValue(context, attack);
            }
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            bool heal = effectType == 7;
            int instances = !heal && health > 1 ? 2 : 1;
            int amount = heal ? 10 - health : Math.Min(health, 2);
            string source = effectType == 1 ? "bleed" : effectType == 16 ? "burn" : effectType == 4 ? "exposure" : heal ? "famine" : "poison";
            for (int i = 0; i < instances; i++)
            {
                var step = Activator.CreateInstance(stepType);
                stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, heal ? "Heal" : "Damage"));
                stepType.GetField("ActorCardId").SetValue(step, "subject");
                stepType.GetField("TargetCardId").SetValue(step, "subject");
                stepType.GetField("EffectType").SetValue(step, effectType);
                stepType.GetField("Amount").SetValue(step, amount);
                stepType.GetField("Source").SetValue(step, source);
                steps.Add(step);
            }
            int Health() => (int)cardType.GetProperty("health").GetValue(card);
            string visual = effectType == 1 ? "PlayBleedContinueAnimation" : effectType == 16 ? "PlayBurnContinueAnimation"
                : effectType == 4 ? "PlayExposureAnimation" : heal ? "PlayFamineContinueAnimation" : "PlayPoisonContinueAnimation";
            var trace = new List<string>();
            int values = 0;
            int visuals = 0;
            int ticks = 0;
            void Drain(System.Collections.IEnumerator routine)
            {
                string name = routine.GetType().Name;
                if (routine.GetType().DeclaringType?.Name == "AttackAnimations" && name.Contains("<" + visual + ">"))
                {
                    Assert.AreEqual(health + (heal ? amount : -amount) * values, Health(), "Animation must finish before its value");
                    if (effectType == 1)
                        Assert.AreEqual(amount, routine.GetType().GetField("drops").GetValue(routine));
                    visuals++;
                    trace.Add("visual");
                    return;
                }
                if (name.Contains(heal ? "<PlayHeal>" : "<PlayDamage>"))
                {
                    Assert.AreEqual(withAnimations ? values + 1 : 0, visuals);
                    Assert.AreEqual(health + (heal ? amount : -amount) * values, Health());
                    values++;
                    trace.Add("value");
                }
                if (name.Contains("<AnimateDamage>") || name.Contains("<AnimateHeal>"))
                {
                    Assert.AreEqual(health + (heal ? amount : -amount) * values, Health());
                    Assert.AreEqual(amount, routine.GetType().GetField(heal ? "healAmount" : "damageAmount").GetValue(routine));
                    trace.Add("number");
                    return;
                }
                if (name.Contains("<ShowDialog>"))
                {
                    Assert.AreEqual(health - amount * values, Health());
                    trace.Add("copy");
                }
                while (routine.MoveNext())
                {
                    if (routine.Current is System.Collections.IEnumerator nested) Drain(nested);
                    else if (routine.Current is WaitForSeconds && name.Contains("<Play>")) ticks++;
                }
            }
            Drain((System.Collections.IEnumerator)runtime.GetType("BattleTimelinePlayback", true).GetMethod("Play")
                .Invoke(null, new object[] { context, steps }));
            var expected = new List<string>();
            for (int i = 0; i < instances; i++)
            {
                if (withAnimations) expected.Add("visual");
                expected.Add("value");
                if (amount > 0) expected.Add("number");
                if (!heal) expected.Add("copy");
            }
            CollectionAssert.AreEqual(expected, trace);
            Assert.AreEqual(health + (heal ? amount : -amount) * instances, Health());
            Assert.AreEqual(heal ? 0 : instances, ticks, "Retain one tick wait per damaging instance");
            if (!heal) StringAssert.Contains(effectType == 4 ? "is irradiated" : "-" + amount + " HP", (string)textType.GetProperty("text").GetValue(text));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void BattleTimelinePlayback_DoesNotContainSharedDamageFallbackAfterHandlerExecution()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("yield return AttackRegistry.Execute(attackId, attackContext);", source);
        Assert.IsFalse(source.Contains("defender.health == defenderHealthBeforeAttack"));
        Assert.IsFalse(source.Contains("[AttackPlayback] Applying shared damage fallback"));
        Assert.IsFalse(source.Contains("private IEnumerator PlayTimelineDamageAnimation("));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void UtilityAndBlockadeKeepSeparateStatAnimationsAndAttackDamageIsNotReplayed(bool isPlayer)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        var root = new GameObject("ValueAndStatOwners", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            var card = root.AddComponent(cardType);
            cardType.GetField("cardId").SetValue(card, "subject");
            cardType.GetProperty("health").SetValue(card, 10);
            cardType.GetProperty("maxHealth").SetValue(card, 10);
            cardType.GetProperty("knowledge").SetValue(card, 2);
            cardType.GetProperty("strength").SetValue(card, 5);
            var context = Activator.CreateInstance(contextType);
            contextType.GetField(isPlayer ? "PlayerCard" : "EnemyCard").SetValue(context, card);
            contextType.GetField("PlayerCardId").SetValue(context, isPlayer ? "subject" : "observer");
            contextType.GetField("EnemyCardId").SetValue(context, isPlayer ? "observer" : "subject");
            contextType.GetField("FinalDelaySeconds").SetValue(context, 0f);
            contextType.GetField("CardAnimator").SetValue(context, root.AddComponent(runtime.GetType("MultiplayerCardAnimator", true)));
            Type attackType = runtime.GetType("Attack", true);
            var attack = root.AddComponent(attackType);
            attackType.GetField("attackAnimations").SetValue(attack, root.AddComponent(runtime.GetType("AttackAnimations", true)));
            contextType.GetField("AttackComponent").SetValue(context, attack);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            void Add(string kind, string source, int amount, int effectType = 0, string stat = null)
            {
                var step = Activator.CreateInstance(stepType);
                foreach (var pair in new Dictionary<string, object> {
                    { "Type", Enum.Parse(eventType, kind) }, { "ActorCardId", "subject" }, { "TargetCardId", "subject" },
                    { "Source", source }, { "Amount", amount }, { "EffectType", effectType }, { "StatName", stat } })
                    stepType.GetField(pair.Key).SetValue(step, pair.Value);
                if (kind == "Blocked") stepType.GetField("BlockedBy").SetValue(step, (int?)12);
                steps.Add(step);
            }
            for (int i = 0; i < 2; i++) { Add("SatelliteTick", "satellite", 1, 18); Add("StatChange", "satellite", 1, 0, "KNO"); }
            Add("Damage", "blockade", 2);
            Add("StatChange", "blockade", -1, 0, "STR");
            Add("Blocked", "effect", 0, 12);
            Add("Damage", "attack", 2);
            int Value(string key) => (int)cardType.GetProperty(key).GetValue(card);
            var trace = new List<string>();
            void Drain(System.Collections.IEnumerator current)
            {
                string name = current.GetType().Name;
                if (name.Contains("<PlaySatelliteAnimation>"))
                {
                    Assert.AreEqual(10, Value("health"));
                    Assert.AreEqual(2 + trace.FindAll(s => s == "stat").Count, Value("knowledge"));
                    trace.Add("satellite"); return;
                }
                if (name.Contains("<AnimateStatChange>")) { trace.Add("stat"); return; }
                if (name.Contains("<AnimateDamage>"))
                {
                    Assert.AreEqual(8, Value("health")); Assert.AreEqual(5, Value("strength"));
                    trace.Add("damage"); return;
                }
                if (name.Contains("<PlayBlocadeWaitAnimation>"))
                {
                    Assert.AreEqual(8, Value("health")); Assert.AreEqual(4, Value("strength"));
                    trace.Add("blockade"); return;
                }
                while (current.MoveNext()) if (current.Current is System.Collections.IEnumerator nested) Drain(nested);
            }
            Drain((System.Collections.IEnumerator)runtime.GetType("BattleTimelinePlayback", true).GetMethod("Play")
                .Invoke(null, new object[] { context, steps }));
            CollectionAssert.AreEqual(new[] { "satellite", "stat", "satellite", "stat", "damage", "stat", "blockade" }, trace);
            Assert.AreEqual(8, Value("health")); Assert.AreEqual(4, Value("knowledge")); Assert.AreEqual(4, Value("strength"));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void StandardDamageHandlers_UseBattleValuePlayback()
    {
        string[] helperHandlers =
        {
            "Attack1Handler.cs",
            "Attack2Handler.cs",
            "Attack5Handler.cs",
            "Attack8Handler.cs",
            "Attack10Handler.cs",
            "Attack12Handler.cs",
            "Attack13Handler.cs",
            "Attack16Handler.cs",
            "Attack17Handler.cs",
            "Attack18Handler.cs",
            "Attack19Handler.cs",
            "Attack20Handler.cs",
            "Attack23Handler.cs",
            "Attack24Handler.cs",
            "Attack25Handler.cs",
            "Attack28Handler.cs",
            "Attack32Handler.cs",
            "Attack34Handler.cs",
            "Attack36Handler.cs",
            "Attack45Handler.cs",
            "Attack46Handler.cs",
        };

        foreach (string fileName in helperHandlers)
        {
            string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", fileName);
            StringAssert.Contains("BattleValuePlayback.PlayDamage(", source, $"Expected shared damage helper in {fileName}");
            StringAssert.Contains("!isMyAttack", source, $"Defender damage should target the opposite health bar in {fileName}");
            Assert.IsFalse(source.Contains("defender.health -= damage;"), $"Manual defender damage should be removed from {fileName}");
        }
    }

    [Test]
    public void OnlyExplicitSpecialCaseHandlers_UseManualParallelDamagePlayback()
    {
        string handlersDir = Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "AttackHandlers");
        string[] allHandlerFiles = Directory.GetFiles(handlersDir, "Attack*Handler.cs");

        var manualDamageHandlers = new List<string>();
        foreach (string path in allHandlerFiles)
        {
            string source = File.ReadAllText(path);
            if (source.Contains("AnimateDamage(defender, damage)") || source.Contains("AnimateDamage(attacker, attackerSelfDamage)"))
            {
                manualDamageHandlers.Add(Path.GetFileName(path));
            }
        }

        CollectionAssert.AreEquivalent(
            new[] { "Attack7Handler.cs", "Attack41Handler.cs" },
            manualDamageHandlers
        );
    }

    [Test]
    public void SharedBattleValuePlayback_OwnsHealthMutationAndUiSync()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleValuePlayback.cs");
        StringAssert.Contains("public static IEnumerator PlayDamage(", source);
        StringAssert.Contains("public static IEnumerator PlayHeal(", source);
        StringAssert.Contains("public static void ApplyDamage(", source);
        StringAssert.Contains("public static void ApplyHeal(", source);
        StringAssert.Contains("public static void SyncHealthBar(", source);
    }

    [Test]
    public void SharedBattleStatPlayback_OwnsStatMutationAndAnimation()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleStatPlayback.cs");
        StringAssert.Contains("public static IEnumerator PlayCardStatChanges(", source);
        StringAssert.Contains("public static IEnumerator PlayTimelineStatChange(", source);
        StringAssert.Contains("cardAnimator.AnimateStatChange(card, change, statName)", source);
    }

    [Test]
    public void BattleTimelinePlayback_UsesSharedStatPlaybackService()
    {
        string timelineSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string statSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleStatPlayback.cs");

        StringAssert.Contains("BattleStatPlayback.PlayTimelineStatChange(", timelineSource);
        StringAssert.Contains("public static IEnumerator PlayCardStatChanges(", statSource);
        Assert.IsFalse(timelineSource.Contains("GetStatApplier("));
        Assert.IsFalse(timelineSource.Contains("BattleStatApplier"));
    }

    [Test]
    public void BattleTimelinePlayback_UsesSharedEffectPlaybackService()
    {
        string timelineSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string effectSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");

        StringAssert.Contains("BattleEffectPlayback.AddEffectIconOnly(", timelineSource);
        StringAssert.Contains("BattleEffectPlayback.RemoveEffectIconOnly(", timelineSource);
        StringAssert.Contains("public static void SyncEffectIconsFromSnapshot(", effectSource);
        StringAssert.Contains("public static string GetEffectName(", effectSource);
        Assert.IsFalse(timelineSource.Contains("GetEffectVisuals("));
        Assert.IsFalse(timelineSource.Contains("BattleEffectVisuals"));
    }

    [Test]
    public void SharedBattleEffectPlayback_OwnsEffectIconAndEndAnimations()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        StringAssert.Contains("public static IEnumerator AddEffectIconOnly(", source);
        StringAssert.Contains("public static IEnumerator RemoveEffectIconOnly(", source);
        StringAssert.Contains("public static void SyncEffectIconsFromSnapshot(", source);
        StringAssert.Contains("public static string GetEffectName(", source);
        StringAssert.Contains("PlayFamineEndAnimation(card.transform)", source);
    }

    [TestCase(1, "Bleed")]
    [TestCase(2, "Asceticism")]
    [TestCase(3, "Sleep")]
    [TestCase(27, "Sleep")]
    [TestCase(4, "Exposure")]
    [TestCase(6, "Fury")]
    [TestCase(7, "Famine")]
    [TestCase(8, "Electricity")]
    [TestCase(9, "Tether")]
    [TestCase(12, "Blockade")]
    [TestCase(13, "Depression")]
    [TestCase(16, "Burn")]
    [TestCase(17, "Confusion")]
    [TestCase(18, "Satellite")]
    [TestCase(19, "Fear")]
    [TestCase(21, "Calm")]
    [TestCase(24, "Poison")]
    public void SharedBattleEffectPlayback_MapsRegisteredEffectIcons(int type, string expected)
    {
        var playback = Type.GetType("BattleEffectPlayback, Assembly-CSharp", true);
        Assert.AreEqual(expected, playback.GetMethod("GetEffectName").Invoke(null, new object[] { type }));
    }

    [TestCase(5)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(14)]
    [TestCase(15)]
    [TestCase(20)]
    [TestCase(22)]
    [TestCase(23)]
    [TestCase(26)]
    public void SharedBattleEffectPlayback_UnsupportedTypesUseTheUnknownPath(int type)
    {
        var playback = Type.GetType("BattleEffectPlayback, Assembly-CSharp", true);
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
            "[BattleEffectPlayback] Unknown effect type: " + type);
        Assert.IsNull(playback.GetMethod("GetEffectName").Invoke(null, new object[] { type }));
    }


    [Test]
    public void BattleTimelinePlayback_UsesExistingPoisonTickAnimation()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("animations.PlayPoisonContinueAnimation(card.transform)", source);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", source);
    }

    [Test]
    public void ConfusionRemovalUsesExistingEndAnimationWithoutForgotBranch()
    {
        string effects = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        string playback = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string animations = ReadProjectFile("Assets", "Scripts", "Game", "Animations", "AttackAnimations.cs");
        StringAssert.Contains("effectType == 17", effects);
        StringAssert.Contains("animations.PlayConfusionEndAnimation(card.transform)", effects);
        Assert.IsFalse(playback.Contains("case BattleStepType.ConfusionForgot:"));
        Assert.IsFalse(animations.Contains("PlayConfusionForgetAnimation("));
    }

    [Test]
    public void BattleTimelinePlayback_PlaysRandomConfusionCueWithApprovedDialog()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string cue = ExtractMethodBlock(source, "private static IEnumerator PlayEffectTriggeredAnimation(");
        StringAssert.Contains("animations.PlayConfusionDecisionAnimation(card.transform)", cue);
        StringAssert.Contains("is confused. Screw the plan!", cue);
        StringAssert.Contains("card.cardId == context.PlayerCardId", cue);
        Assert.IsFalse(ExtractMethodBlock(source, "public static IEnumerator Play(")
            .Contains("case BattleStepType.ConfusionForgot:"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ConfusionDialogWritesMessageAndActorColorBeforeItsWait(bool isPlayer)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(),
            assembly => assembly.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type playbackType = runtime.GetType("BattleTimelinePlayback", true);
        var context = Activator.CreateInstance(contextType);
        Type textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
        var root = new GameObject("ConfusionDialog", typeof(RectTransform));
        try
        {
            var text = root.AddComponent(textType);
            textType.GetProperty("text").SetValue(text, "previous");
            textType.GetProperty("color").SetValue(text, Color.green);
            contextType.GetField("DialogText").SetValue(context, text);
            contextType.GetField("DialogDelaySeconds").SetValue(context, 0.75f);
            var method = playbackType.GetMethod("ShowDialog",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var routine = (System.Collections.IEnumerator)method.Invoke(null,
                new object[] { context, "NAME is confused. Screw the plan!", (bool?)isPlayer });
            Assert.IsTrue(routine.MoveNext());
            Assert.AreEqual("NAME is confused. Screw the plan!", textType.GetProperty("text").GetValue(text));
            Assert.AreEqual(isPlayer ? Color.blue : Color.red, textType.GetProperty("color").GetValue(text));
            Assert.IsInstanceOf<WaitForSeconds>(routine.Current);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void BattleSubmitter_PollsViaGetBattleStatusNotDummyAttackZero()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleSubmitter.cs");
        StringAssert.Contains("serverFunctionsManager.GetBattleStatus(", source);
        Assert.IsFalse(source.Contains("attackId = 0"));
        Assert.IsFalse(source.Contains("attackSlot = 0"));
        Assert.IsFalse(source.Contains("dummySubmission"));
    }

    [Test]
    public void MissingRoomMonitorRoutesTerminalRecoveryThroughLobbyNotice()
    {
        string service = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerService.cs");
        int handlerStart = service.IndexOf("private void HandleMissingOwnedRoom", System.StringComparison.Ordinal);
        int monitorStart = service.IndexOf("private IEnumerator MatchStateMonitorLoop", System.StringComparison.Ordinal);

        Assert.GreaterOrEqual(handlerStart, 0);
        Assert.GreaterOrEqual(monitorStart, 0);
        string monitor = service.Substring(monitorStart, handlerStart - monitorStart);

        StringAssert.Contains("HandleMissingOwnedRoom(expectedRoom);", monitor);
        StringAssert.Contains("PrepareMissingOwnedRoomRecovery(expectedRoom)", service);
        StringAssert.Contains("SceneManager.LoadScene(MultiplayerLobbyScene);", service);
        Assert.IsFalse(service.Contains("missingRoomDestinationScene"));
    }

    [Test]
    public void ServerFunctionsManager_ExposesGetBattleStatus()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Networking", "ServerFunctionsManager.cs");
        StringAssert.Contains("public void GetBattleStatus(", source);
        StringAssert.Contains("CallFunctionWithRetry(\"getBattleStatus\"", source);
    }

    [Test]
    public void MultiplayerCleanupManager_ResolvesServerFunctionsManagerLazily()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerCleanupManager.cs");
        StringAssert.Contains("ResolveServerFunctionsManager();", source);
        StringAssert.Contains("FindFirstObjectByType<ServerFunctionsManager>(FindObjectsInactive.Include)", source);
    }

    [Test]
    public void BattleSubmitter_DoesNotUseLegacyBattlePollingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleSubmitter.cs");
        Assert.IsFalse(source.Contains("PollForBattleResultLegacy("));
        Assert.IsFalse(source.Contains("falling back to legacy battle polling"));
        Assert.IsFalse(source.Contains("switching to legacy GetBattleStatus polling"));
    }

    [Test]
    public void BattleRoundCoordinator_DoesNotUseLegacyNextTurnPollingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleRoundCoordinator.cs");
        Assert.IsFalse(source.Contains("PollForNextTurnReadyLegacy("));
        Assert.IsFalse(source.Contains("switching to legacy CheckNextTurnReady polling"));
    }

    [Test]
    public void MultiplayerBoardManager_DoesNotFallbackToSelectedCardsPolling()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerBoardManager.cs");
        Assert.IsFalse(source.Contains("WaitForOpponentSelectionLegacyAsync"));
        Assert.IsFalse(source.Contains("falling back to getSelectedCards polling"));
    }

    [Test]
    public void MultiplayerService_EmergencyLeaveRoom_DoesNotPassNullCallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerService.cs");
        Assert.IsFalse(source.Contains("LeaveRoom(myPlayerId, null)"));
        StringAssert.Contains("LeaveRoom(myPlayerId, roomCode);", source);
    }

    [Test]
    public void MultiplayerLobbyScene_DoesNotContainObsoleteMissingMatchmakingComponent()
    {
        string source = ReadProjectFile("Assets", "Scenes", "MultiplayerLobby.unity");
        Assert.IsFalse(source.Contains("guid: 5a48533f13544dc47b7d808bc555a419"));
        Assert.IsFalse(source.Contains("component: {fileID: 1347824464}"));
    }

    [Test]
    public void MissingRoomAcknowledgementBelongsToLobbyScene()
    {
        string lobby = ReadProjectFile("Assets", "Scenes", "MultiplayerLobby.unity");
        string battle = ReadProjectFile("Assets", "Scenes", "Multiplayer.unity");

        StringAssert.Contains("m_Name: MatchUnavailableDialog", lobby);
        StringAssert.Contains("matchUnavailableDialog:", lobby);
        Assert.IsFalse(battle.Contains("m_Name: MatchUnavailableDialog"));
        Assert.IsFalse(battle.Contains("matchUnavailableDialog:"));
    }

    [Test]
    public void LegacyPlaceholderScripts_AreNotPresent()
    {
        string[] legacyPaths =
        {
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerLobby.cs"),
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerLobbyUI.cs"),
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerMatchmakingUI.cs"),
            Path.Combine(Application.dataPath, "Scripts", "ServerFunctionsManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerNetworkManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerPlayerManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerRoomManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Album", "Animations", "Animations.cs"),
        };

        foreach (string path in legacyPaths)
        {
            Assert.IsFalse(File.Exists(path), $"Legacy placeholder script should not exist: {path}");
            Assert.IsFalse(File.Exists(path + ".meta"), $"Legacy placeholder meta should not exist: {path}.meta");
        }
    }

    [Test]
    public void MultiplayerCardAnimator_FloatingEffectAnimationsUseDetachedVisibleParent()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerCardAnimator.cs");

        StringAssert.Contains("Vector3 cardPosition = card.transform.position;", source);
        StringAssert.Contains("GameObject notsureTemplate = card.notsureGO;", source);
        StringAssert.Contains("ResolveDetachedEffectParent(card)", source);
        StringAssert.Contains("card.GetComponentInParent<Canvas>()", source);
        StringAssert.Contains("card.transform.parent", source);
        StringAssert.Contains("effectObject = Instantiate(notsureTemplate, effectParent, false);", source);
        StringAssert.Contains("effectObject = Instantiate(effectAnimationPrefab, effectParent, false);", source);
        Assert.IsFalse(source.Contains("Transform effectParent = transform;"));
        Assert.IsFalse(source.Contains("Instantiate(card.notsureGO, card.transform)"));
        Assert.IsFalse(source.Contains("Instantiate(effectAnimationPrefab, card.transform)"));
        Assert.IsFalse(source.Contains("(Vector2)card.transform.position + randomPosition"));
        Assert.IsFalse(source.Contains(" - card.transform.position"));
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesMultiplayerBlockEffectAnimations()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("PlayBlockAnimation(context, actor, step.BlockedBy)", source);
        StringAssert.Contains("animations.PlayConfusionHurtItselfAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlaySleepAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayElectricityAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayTetherAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayBlocadeWaitAnimation(card.transform)", source);
        Assert.IsFalse(source.Contains("attack was blocked"));
    }

    [Test]
    public void SharedSelfDamageRemainsWithoutTheRetiredConfusionOutcome()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string method = ExtractMethodBlock(source, "private static IEnumerator PlaySelfDamageAnimation(");
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", method);
        Assert.IsFalse(method.Contains("PlayConfusionHurtItselfAnimation"));
        StringAssert.Contains("animations.PlayConfusionHurtItselfAnimation(card.transform)", source);
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesSharedEffectApplicationAndSelfRemoval()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("BattleEffectPlayback.AddEffectIconOnly(", source);
        StringAssert.Contains("BattleEffectPlayback.RemoveEffectIconOnly(", source);
        Assert.IsFalse(source.Contains("SyncEffectIcons(defender, attackerResult.effectsApplied)"));
        Assert.IsFalse(source.Contains("SyncEffectIcons(attacker, attackerResult.attackerEffectsApplied)"));
    }

    [Test]
    public void BattleResultDto_PreservesTimelineV2ForSharedPlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleContracts", "BattleContractDtos.cs");
        string builder = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelineBuilder.cs");

        StringAssert.Contains("public Dictionary<string, object> timelineV2;", source);
        StringAssert.Contains("TryBuild(BattleResultDto battleResult", builder);
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesTimelineV2InsteadOfLegacyEffectFields()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattlePlayback.cs");

        StringAssert.Contains("BattleTimelineBuilder.TryBuild(envelope.battleResult", source);
        StringAssert.Contains("BattleTimelinePlayback.Play(", source);
        Assert.IsFalse(source.Contains("PlayPreAttackPassiveEffectsAsync"));
        Assert.IsFalse(source.Contains("attackerResult.bleedDamage"));
        Assert.IsFalse(source.Contains("attackerResult.exposureDamage"));
        Assert.IsFalse(source.Contains("attackerResult.exposureRemoved"));
    }

    [Test]
    public void SharedTimelinePlayback_CoversEffectLifecycleSteps()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("case BattleStepType.EffectTriggered:", source);
        StringAssert.Contains("case BattleStepType.EffectApplied:", source);
        StringAssert.Contains("case BattleStepType.EffectRemoved:", source);
        StringAssert.Contains("case BattleStepType.Blocked:", source);
        StringAssert.Contains("case BattleStepType.OngoingActionProgress:", source);
        StringAssert.Contains("case BattleStepType.OngoingActionResolved:", source);
    }

    [Test]
    public void SharedTimelinePlayback_RemovesEndedEffectIconsAtTimelineStep()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string effects = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");

        StringAssert.Contains("RemoveEndedEffectIcon(context, target, step.EffectType)", source);
        StringAssert.Contains("animations.PlaySleepEndAnimation(card.transform)", effects);
        StringAssert.Contains("animations.PlayAscetismEndAnimation(card.transform)", effects);
        StringAssert.Contains("animations.PlayExposureEndAnimation(card.transform)", effects);
    }

    [TestCase("recovered", true)]
    [TestCase("cleansed", false)]
    public void CanonicalExposureExit_UsesOneSharedRemovalAndReasonSpecificCopy(string reason, bool showsRecovery)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(),
            assembly => assembly.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        Type playbackType = runtime.GetType("BattleTimelinePlayback", true);
        Type textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
        var root = new GameObject("EffectExit", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            var card = root.AddComponent(cardType);
            cardType.GetField("cardId").SetValue(card, "recipient");
            cardType.GetField("cardName").SetValue(card, "Recipient");
            var icons = new GameObject("Icons", typeof(RectTransform));
            icons.transform.SetParent(root.transform, false);
            cardType.GetField("effectIconContainer").SetValue(card, icons.transform);
            var dialogRoot = new GameObject("Dialog", typeof(RectTransform));
            dialogRoot.transform.SetParent(root.transform, false);
            var text = dialogRoot.AddComponent(textType);
            textType.GetProperty("text").SetValue(text, "previous");
            var context = Activator.CreateInstance(contextType);
            contextType.GetField("PlayerCard").SetValue(context, card);
            contextType.GetField("PlayerCardId").SetValue(context, "recipient");
            contextType.GetField("DialogText").SetValue(context, text);
            contextType.GetField("DialogDelaySeconds").SetValue(context, 0f);
            contextType.GetField("TickDelaySeconds").SetValue(context, 0f);
            contextType.GetField("FinalDelaySeconds").SetValue(context, 0f);
            var step = Activator.CreateInstance(stepType);
            stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, "EffectRemoved"));
            stepType.GetField("ActorCardId").SetValue(step, "caster");
            stepType.GetField("TargetCardId").SetValue(step, "recipient");
            stepType.GetField("EffectType").SetValue(step, 4);
            stepType.GetField("RemovalReason").SetValue(step, reason);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            steps.Add(step);
            var routine = (System.Collections.IEnumerator)playbackType.GetMethod("Play").Invoke(null, new object[] { context, steps });
            int removals = 0;
            void Drain(System.Collections.IEnumerator current)
            {
                if (current.GetType().Name.Contains("<RemoveEffectIconOnly>")) removals++;
                while (current.MoveNext())
                    if (current.Current is System.Collections.IEnumerator nested) Drain(nested);
            }
            Drain(routine);
            Assert.AreEqual(1, removals);
            Assert.AreEqual(showsRecovery ? "Recipient's irradiation is gone" : "previous", textType.GetProperty("text").GetValue(text));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [TestCase(true, 5)]
    [TestCase(false, 5)]
    [TestCase(true, 1)]
    [TestCase(false, 1)]
    public void AsceticismCueCompletesBeforeOneSharedDamageAndShake(bool isPlayer, int initialHealth)
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(),
            assembly => assembly.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Type cardType = runtime.GetType("Kard", true);
        Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
        Type stepType = runtime.GetType("BattleStep", true);
        Type eventType = runtime.GetType("BattleStepType", true);
        Type playbackType = runtime.GetType("BattleTimelinePlayback", true);
        Type attackType = runtime.GetType("Attack", true);
        var root = new GameObject("AsceticismOrder", typeof(RectTransform));
        root.SetActive(false);
        try
        {
            var card = root.AddComponent(cardType);
            cardType.GetField("cardId").SetValue(card, "ascetic");
            cardType.GetField("cardName").SetValue(card, "Ascetic");
            cardType.GetProperty("health").SetValue(card, initialHealth);
            cardType.GetProperty("maxHealth").SetValue(card, initialHealth);
            var attack = root.AddComponent(attackType);
            attackType.GetField("attackAnimations").SetValue(attack,
                root.AddComponent(runtime.GetType("AttackAnimations", true)));
            var context = Activator.CreateInstance(contextType);
            contextType.GetField(isPlayer ? "PlayerCard" : "EnemyCard").SetValue(context, card);
            contextType.GetField("PlayerCardId").SetValue(context, isPlayer ? "ascetic" : "observer");
            contextType.GetField("EnemyCardId").SetValue(context, isPlayer ? "observer" : "ascetic");
            contextType.GetField("AttackComponent").SetValue(context, attack);
            contextType.GetField("CardAnimator").SetValue(context,
                root.AddComponent(runtime.GetType("MultiplayerCardAnimator", true)));
            contextType.GetField("FinalDelaySeconds").SetValue(context, 0f);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            object AddStep(string name)
            {
                var step = Activator.CreateInstance(stepType);
                stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, name));
                stepType.GetField("ActorCardId").SetValue(step, "ascetic");
                stepType.GetField("TargetCardId").SetValue(step, "ascetic");
                stepType.GetField("EffectType").SetValue(step, 2);
                stepType.GetField("Source").SetValue(step, "asceticism");
                steps.Add(step);
                return step;
            }
            AddStep("EffectTriggered");
            stepType.GetField("Amount").SetValue(AddStep("Damage"), 1);
            stepType.GetField("BlockedBy").SetValue(AddStep("Blocked"), (int?)2);
            if (initialHealth == 1) AddStep("Death");

            int Health() => (int)cardType.GetProperty("health").GetValue(card);
            var trace = new List<string>();
            bool fistCompleted = false;
            void Drain(System.Collections.IEnumerator current)
            {
                string name = current.GetType().Name;
                // Drive real sequencing, but complete visual leaves without scene assets.
                if (name.Contains("<PlayConfusionHurtItselfAnimation>"))
                {
                    Assert.AreEqual(initialHealth, Health(), "Fist must finish before HP changes");
                    trace.Add("fist");
                    fistCompleted = true;
                    return;
                }
                if (name.Contains("<AnimateDamage>"))
                {
                    Assert.IsTrue(fistCompleted, "Shake must follow the completed fist");
                    trace.Add("shake");
                    return;
                }
                if (name.Contains("<PlayDamage>"))
                {
                    Assert.IsTrue(fistCompleted, "Damage must await the fist cue");
                    Assert.AreEqual(initialHealth, Health());
                    trace.Add("damage");
                }
                while (current.MoveNext())
                    if (current.Current is System.Collections.IEnumerator nested) Drain(nested);
            }
            Drain((System.Collections.IEnumerator)playbackType.GetMethod("Play")
                .Invoke(null, new object[] { context, steps }));
            CollectionAssert.AreEqual(new[] { "fist", "damage", "shake" }, trace);
            Assert.AreEqual(initialHealth - 1, Health());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void MultiplayerTimeline_UsesSharedTimelinePlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");

        StringAssert.Contains("new BattleTimelinePlaybackContext", source);
        StringAssert.Contains("BattleTimelinePlayback.Play(context, steps)", source);
    }

    [Test]
    public void BattleRuntime_HasNoRetiredTimelinePilotPolicy()
    {
        var runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name == "Assembly-CSharp");
        Assert.IsNotNull(runtime);
        Assert.IsNull(runtime.GetType("BattleTimelinePilotPolicy"));
        string path = Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePilotPolicy.cs");
        Assert.IsFalse(File.Exists(path));
        Assert.IsFalse(File.Exists(path + ".meta"));
    }

    [Test]
    public void MultiplayerTimeline_UsesExistingValidatorWithoutAPilotGate()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("BattleTimelineBuilder.TryBuild(", source);
        Assert.IsFalse(source.Contains("BattleTimelinePilotPolicy"));
        Assert.IsFalse(source.Contains("ShouldRunTimeline"));
        Assert.IsFalse(source.Contains("PlayerPrefs"));
    }

    [Test]
    public void SnapshotEffectIcons_UseOneSharedOwnerAcrossAllBattleClients()
    {
        string[] callers =
        {
            ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs"),
            ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs"),
            ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs"),
            ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.ServerRefresh.cs"),
        };
        foreach (string source in callers)
        {
            StringAssert.Contains("BattleEffectPlayback.SyncEffectIconsFromSnapshot(", source);
            Assert.IsFalse(source.Contains("private void SyncEffectIconsFromSnapshot("));
            Assert.IsFalse(source.Contains("DestroyEffectIconBeforeReposition("));
            Assert.IsFalse(source.Contains("card.AddEffectIcon("));
        }
    }

    [Test]
    public void SnapshotEffectIcons_PreserveCountsAndSurvivingObjectsAcrossSameFrameSync()
    {
        using var fixture = new EffectIconFixture();
        fixture.Sync(1, 1);
        Assert.AreEqual(2, fixture.Icons.childCount);
        Transform first = fixture.Icons.GetChild(0);
        Transform second = fixture.Icons.GetChild(1);
        fixture.Sync(1, 1);
        Assert.AreSame(first, fixture.Icons.GetChild(0));
        Assert.AreSame(second, fixture.Icons.GetChild(1));
        fixture.Sync(1);
        Assert.AreEqual(1, fixture.Icons.childCount, "Excess icon must detach before scheduled destruction");
        Assert.AreSame(first, fixture.Icons.GetChild(0));
        fixture.Sync(1, 1);
        Assert.AreEqual(2, fixture.Icons.childCount, "Detached pending destruction must not satisfy a new instance");
        Assert.AreSame(first, fixture.Icons.GetChild(0));
        Assert.AreNotSame(second, fixture.Icons.GetChild(1));
        fixture.Sync();
        Assert.AreEqual(0, fixture.Icons.childCount);
        fixture.Sync();
        Assert.AreEqual(0, fixture.Icons.childCount);
    }

    [Test]
    public void SnapshotEffectIcons_RemoveStaleTypesWhilePreservingOtherInstances()
    {
        using var fixture = new EffectIconFixture();
        fixture.Sync(1, 1, 24);
        Transform poison = fixture.Icons.GetChild(2);
        fixture.Sync(24, 24);
        Assert.AreEqual(2, fixture.Icons.childCount);
        Assert.AreSame(poison, fixture.Icons.GetChild(0));
        foreach (Transform icon in fixture.Icons)
            StringAssert.StartsWith("PoisonIcon_", icon.name);
    }

    [Test]
    public void SnapshotEffectIcons_SharedSleepVisualNameDoesNotCreateDuplicatesOnReplacement()
    {
        using var fixture = new EffectIconFixture();
        fixture.Sync(3);
        Transform sleep = fixture.Icons.GetChild(0);
        fixture.Sync(27);
        Assert.AreEqual(1, fixture.Icons.childCount);
        Assert.AreSame(sleep, fixture.Icons.GetChild(0));
    }

    [Test]
    public void SnapshotEffectIcons_DoNotMutateGameplayOrExposeUnusedApplicationPlayback()
    {
        using var fixture = new EffectIconFixture();
        string[] stats = { "strength", "speed", "attack", "defense", "knowledge", "charisma" };
        for (int i = 0; i < stats.Length; i++)
            fixture.CardType.GetProperty(stats[i]).SetValue(fixture.Card, i + 2);
        fixture.Sync(1, 1, 24);
        fixture.Sync(24);
        Assert.AreEqual(9, fixture.CardType.GetProperty("health").GetValue(fixture.Card));
        Assert.AreEqual(20, fixture.CardType.GetProperty("maxHealth").GetValue(fixture.Card));
        for (int i = 0; i < stats.Length; i++)
            Assert.AreEqual(i + 2, fixture.CardType.GetProperty(stats[i]).GetValue(fixture.Card), stats[i]);
        Assert.IsNull(fixture.CardType.GetField("effects"));
        Assert.IsNull(fixture.EffectPlaybackType.GetMethod("DisplayMultipleEffects"));
        Assert.IsNull(fixture.EffectPlaybackType.GetMethod("DisplayEffectIcon",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
    }

    [UnityEngine.TestTools.UnityTest]
    public System.Collections.IEnumerator OrderedEffectIcons_PreserveTwoApplicationsAndTwoSeparateRemovals()
    {
        using var fixture = new EffectIconFixture();
        yield return fixture.Play("EffectApplied", "EffectApplied");
        Assert.AreEqual(2, fixture.Icons.childCount);
        fixture.Sync(1, 1);
        Assert.AreEqual(2, fixture.Icons.childCount, "Snapshot must not replay applications");
        yield return fixture.Play("EffectRemoved");
        Assert.AreEqual(1, fixture.Icons.childCount);
        Transform survivor = fixture.Icons.GetChild(0);
        fixture.Sync(1);
        Assert.AreSame(survivor, fixture.Icons.GetChild(0));
        yield return fixture.Play("EffectRemoved");
        Assert.AreEqual(0, fixture.Icons.childCount);
        Assert.AreEqual(9, fixture.CardType.GetProperty("health").GetValue(fixture.Card));
    }

    private sealed class EffectIconFixture : IDisposable
    {
        public readonly Type CardType;
        public readonly Type EffectPlaybackType;
        public readonly Component Card;
        public readonly Transform Icons;
        private readonly System.Reflection.Assembly runtime;
        private readonly GameObject root;
        private readonly Type effectDataType;

        public EffectIconFixture()
        {
            runtime = Array.Find(AppDomain.CurrentDomain.GetAssemblies(),
                assembly => assembly.GetName().Name == "Assembly-CSharp");
            Assert.IsNotNull(runtime);
            CardType = runtime.GetType("Kard", true);
            EffectPlaybackType = runtime.GetType("BattleEffectPlayback", true);
            effectDataType = runtime.GetType("SelectedCardData+EffectData", true);
            root = new GameObject("EffectIconSnapshot", typeof(RectTransform));
            root.SetActive(false);
            Card = root.AddComponent(CardType);
            CardType.GetField("cardId").SetValue(Card, "recipient");
            CardType.GetField("cardName").SetValue(Card, "Recipient");
            CardType.GetProperty("health").SetValue(Card, 9);
            CardType.GetProperty("maxHealth").SetValue(Card, 20);
            Icons = new GameObject("Icons", typeof(RectTransform)).transform;
            Icons.SetParent(root.transform, false);
            CardType.GetField("effectIconContainer").SetValue(Card, Icons);
        }

        public void Sync(params int[] types)
        {
            var method = EffectPlaybackType.GetMethod("SyncEffectIconsFromSnapshot");
            Assert.IsNotNull(method, "Existing effect playback must own silent snapshot count reconciliation");
            Array effects = Array.CreateInstance(effectDataType, types.Length);
            for (int i = 0; i < types.Length; i++)
            {
                object effect = Activator.CreateInstance(effectDataType);
                effectDataType.GetField("type").SetValue(effect, types[i].ToString());
                effectDataType.GetField("duration").SetValue(effect, 2);
                effects.SetValue(effect, i);
            }
            method.Invoke(null, new object[] { Card, effects });
        }

        public System.Collections.IEnumerator Play(params string[] names)
        {
            Type stepType = runtime.GetType("BattleStep", true);
            Type contextType = runtime.GetType("BattleTimelinePlaybackContext", true);
            Type eventType = runtime.GetType("BattleStepType", true);
            var context = Activator.CreateInstance(contextType);
            contextType.GetField("PlayerCard").SetValue(context, Card);
            contextType.GetField("PlayerCardId").SetValue(context, "recipient");
            contextType.GetField("DialogDelaySeconds").SetValue(context, 0f);
            contextType.GetField("TickDelaySeconds").SetValue(context, 0f);
            contextType.GetField("FinalDelaySeconds").SetValue(context, 0f);
            var steps = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stepType));
            foreach (string name in names)
            {
                object step = Activator.CreateInstance(stepType);
                stepType.GetField("Type").SetValue(step, Enum.Parse(eventType, name));
                stepType.GetField("ActorCardId").SetValue(step, "recipient");
                stepType.GetField("TargetCardId").SetValue(step, "recipient");
                stepType.GetField("EffectType").SetValue(step, 1);
                stepType.GetField("Duration").SetValue(step, 2);
                stepType.GetField("Source").SetValue(step, "attack");
                stepType.GetField("RemovalReason").SetValue(step, "cleansed");
                steps.Add(step);
            }
            return (System.Collections.IEnumerator)runtime.GetType("BattleTimelinePlayback", true)
                .GetMethod("Play").Invoke(null, new object[] { context, steps });
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void RoyalRumbleShellRebuild_RendersReplacementHandOnlyOnce()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string rebuildMethod = ExtractMethodBlock(source, "private void RebuildShellView()");

        StringAssert.Contains("RestorePlayerActiveOrHand();", rebuildMethod);
        Assert.IsFalse(
            rebuildMethod.Contains("RenderPlayerHand();"),
            "RebuildShellView must not render the player hand directly; RestorePlayerActiveOrHand owns that decision."
        );
    }

    [Test]
    public void CampaignOnlineShell_RendersAndCleansEnemyHand()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string renderMethod = ExtractMethodBlock(source, "private void RenderCampaignView()");
        string clearMethod = ExtractMethodBlock(source, "private void ClearRenderedCards()");

        StringAssert.Contains("private readonly List<Kard> renderedEnemyHandCards", source);
        StringAssert.Contains("RenderEnemyHand(selectedEnemy?.cardId);", renderMethod);
        StringAssert.Contains("private void RenderEnemyHand(string excludeCardId)", source);
        StringAssert.Contains("foreach (Kard card in renderedEnemyHandCards)", clearMethod);
        StringAssert.Contains("renderedEnemyHandCards.Clear();", clearMethod);
    }

    [Test]
    public void CampaignOnlineShell_UsesExplicitSceneReferencesWithoutRuntimeListenerReplacement()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");

        StringAssert.Contains("public bool HasRequiredSceneReferences(out string error)", source);
        StringAssert.Contains("public void ConfigureForMission(string configuredCampaignId, int configuredMissionId)", source);
        Assert.IsFalse(source.Contains("ReplaceSceneButtonListenersForOnlineCampaign"));
        Assert.IsFalse(source.Contains("FindButtonByName("));
        Assert.IsFalse(source.Contains("FindPlayers("));
        Assert.IsFalse(source.Contains("GameObject.Find("));
        Assert.IsFalse(source.Contains("gameObject.AddComponent<CampaignOnlineService>()"));
        Assert.IsFalse(source.Contains("gameObject.AddComponent<CampaignOnlineBattlePlayback>()"));
    }

    [Test]
    public void SingleplayerBattleSceneRouter_OwnsModeSelectionAndSharedInput()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Game", "SingleplayerBattleSceneRouter.cs");

        StringAssert.Contains("GameParameters.MissionID > 0", source);
        StringAssert.Contains("campaignShell.ConfigureForMission(", source);
        StringAssert.Contains("public void AttackButton(int attackSlot)", source);
        StringAssert.Contains("public void ConfirmAttackButton()", source);
        StringAssert.Contains("campaignShell?.AttackButton(attackSlot)", source);
        StringAssert.Contains("royalRumbleShell?.AttackButton(attackSlot)", source);
    }

    [Test]
    public void GameScene_RoutesAllSharedBattleInputThroughSingleplayerRouter()
    {
        string source = ReadProjectFile("Assets", "Scenes", "Game.unity");

        StringAssert.Contains("m_Name: SingleplayerBattleSceneRouter", source);
        StringAssert.Contains("m_Name: CampaignOnline", source);
        Assert.AreEqual(
            5,
            CountOccurrences(source, "m_TargetAssemblyTypeName: SingleplayerBattleSceneRouter, Assembly-CSharp"),
            "Four attack events and one confirm event must target the scene router."
        );
        Assert.IsFalse(source.Contains("m_TargetAssemblyTypeName: RoyalRumbleShellController, Assembly-CSharp"));
        Assert.IsFalse(source.Contains("m_TargetAssemblyTypeName: RoyalRumbleBattleCoordinator, Assembly-CSharp"));
    }

    [Test]
    public void LocalCampaignBattleAndProgressAuthority_IsRemoved()
    {
        string campaignDirectory = Path.Combine(Application.dataPath, "Scripts", "Campaign");
        string onlineDirectory = Path.Combine(campaignDirectory, "Online");

        Assert.IsFalse(File.Exists(Path.Combine(onlineDirectory, "CampaignOnlineGameBootstrap.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(campaignDirectory, "FightSystemCampaign.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(campaignDirectory, "CampaignManager.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(Application.dataPath, "Scripts", "FightSystem.cs")));
    }

    [Test]
    public void LegacyFightSystemAndDragKardRuntime_IsRemoved()
    {
        string scriptsDirectory = Path.Combine(Application.dataPath, "Scripts");
        string multiplayerScene = ReadProjectFile("Assets", "Scenes", "Multiplayer.unity");
        string cardPrefab = ReadProjectFile("Assets", "Prefabs", "Kard.prefab");
        string attackDescriptions = ReadProjectFile("Assets", "Scripts", "AttackDescriptions.cs");
        string rrShell = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string rrCoordinator = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs");
        string campaignShell = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string multiplayerHand = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerHandManager.cs");

        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "FightSystem.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "FightSystem.cs.meta")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "DragKard.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "DragKard.cs.meta")));

        Assert.IsFalse(multiplayerScene.Contains("FightSystem, Assembly-CSharp"));
        Assert.IsFalse(multiplayerScene.Contains("5e54471e2c9ac384396981d6824289d5"));
        Assert.IsFalse(cardPrefab.Contains("4892ce4360c797f419cc643828d8ec67"));
        Assert.IsFalse(attackDescriptions.Contains("FightSystem"));
        Assert.IsFalse(rrShell.Contains("DragKard"));
        Assert.IsFalse(rrCoordinator.Contains("DragKard"));
        Assert.IsFalse(campaignShell.Contains("DragKard"));
        Assert.IsFalse(multiplayerHand.Contains("DragKard"));
    }

    [Test]
    public void CampaignOnlineShell_RefreshesReplacementHandDraggabilityAfterBusyStateClears()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string updateMethod = ExtractMethodBlock(source, "private void UpdateAttackButtons()");
        string refreshMethod = ExtractMethodBlock(source, "private void RefreshPlayerHandDraggability()");

        StringAssert.Contains("RefreshPlayerHandDraggability();", updateMethod);
        StringAssert.Contains("foreach (Kard card in renderedPlayerHandCards)", refreshMethod);
        StringAssert.Contains("card.isDragable = CanDragCard(card);", refreshMethod);
    }

    [Test]
    public void CampaignMissionManager_LoadsServerAuthoritativeProgressWithoutLocalFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "MissionManager.cs");

        StringAssert.Contains("private async void Start()", source);
        Assert.IsFalse(source.Contains("private async void OnEnable()"));
        StringAssert.Contains("campaignService.GetProgressAsync(playerId, campaignId)", source);
        StringAssert.Contains("ApplyLevelAccess(envelope.progress.highestUnlockedMissionId);", source);
        StringAssert.Contains("campaignContentRoot", source);
        StringAssert.Contains("SetCampaignContentVisible(false);", source);
        StringAssert.Contains("SceneLoadingOverlay.SetMessage(\"LOADING...\");", source);
        StringAssert.Contains("SceneLoadingOverlay.Show();", source);
        StringAssert.Contains("SceneLoadingOverlay.Hide();", source);
        StringAssert.Contains("SetCampaignContentVisible(true);", source);
        Assert.IsFalse(source.Contains("CampaignManager.Instance.LoadCampaignData"));
    }

    [Test]
    public void RoyalRumbleShell_DoesNotMaintainLocalOngoingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");

        Assert.IsFalse(
            source.Contains("localPendingOngoingAction"),
            "RR shell must not keep local pending ongoing state; server session ongoingActions are authoritative."
        );
        Assert.IsFalse(
            source.Contains("UpdateLocalPendingOngoingFallback"),
            "RR shell must not infer ongoing state from battle results as a fallback."
        );
        Assert.IsFalse(
            source.Contains("TryCreateStartedOngoingFallback"),
            "RR shell must not synthesize started ongoing actions locally."
        );
        Assert.IsFalse(
            source.Contains("Created local pending ongoing fallback"),
            "RR shell must not log or create local ongoing fallback state."
        );
    }

    [Test]
    public void RoyalRumbleShell_PendingOngoingReadsCanonicalSessionStateWithoutAdapterComponent()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string pendingMethod = ExtractMethodBlock(source, "private PendingOngoingActionTurnData GetPendingOngoingAction()");

        StringAssert.Contains("RoyalRumbleTurnAdapter.CreatePendingOngoingAction(selected)", pendingMethod);
        Assert.IsFalse(
            pendingMethod.Contains("turnAdapter != null"),
            "RR shell pending ongoing detection must not depend on an optional scene adapter component."
        );
    }

    [Test]
    public void RoyalRumbleClients_DoNotSelectEnemyCardLocallyAsFallback()
    {
        string shell = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string coordinator = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs");

        Assert.IsFalse(
            shell.Contains("Falling back locally"),
            "RR shell must not locally choose an enemy card when server active enemy state is missing."
        );
        Assert.IsFalse(
            coordinator.Contains("Falling back locally"),
            "RR coordinator must not locally choose an enemy card when server active enemy state is missing."
        );
        Assert.IsFalse(
            shell.Contains("SetActiveEnemyCardId("),
            "RR shell must not mutate active enemy state locally."
        );
        Assert.IsFalse(
            coordinator.Contains("SetActiveEnemyCardId("),
            "RR coordinator must not mutate active enemy state locally."
        );
    }

    private static string ReadProjectFile(params string[] relativeParts)
    {
        var allParts = new List<string> { Application.dataPath };
        allParts.AddRange(relativeParts[1..]);
        string fullPath = Path.Combine(allParts.ToArray());
        Assert.IsTrue(File.Exists(fullPath), $"Missing project file: {fullPath}");
        return File.ReadAllText(fullPath);
    }

    private static string ExtractMethodBlock(string source, string signature)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, $"Missing method signature: {signature}");

        int bodyStart = source.IndexOf('{', start);
        Assert.GreaterOrEqual(bodyStart, 0, $"Missing method body for: {signature}");

        int depth = 0;
        for (int i = bodyStart; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(start, i - start + 1);
                }
            }
        }

        Assert.Fail($"Unterminated method body for: {signature}");
        return string.Empty;
    }

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
