using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttackExecutionContext
{
    public AttackExecutionContext(
        Kard attacker,
        Kard defender,
        int damage,
        int healAmount,
        bool isMyAttack,
        int attackerSelfDamage,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied,
        string attackResult,
        List<Dictionary<string, object>> attackerEffects)
    {
        Attacker = attacker;
        Defender = defender;
        Damage = damage;
        HealAmount = healAmount;
        IsMyAttack = isMyAttack;
        AttackerSelfDamage = attackerSelfDamage;
        Animations = animations;
        CardAnimator = cardAnimator;
        PlayerLifeBar = playerLifeBar;
        EnemyLifeBar = enemyLifeBar;
        ShowDialog = showDialog;
        EffectsApplied = effectsApplied;
        AttackResult = attackResult;
        AttackerEffects = attackerEffects;
    }

    public Kard Attacker { get; }
    public Kard Defender { get; }
    public int Damage { get; }
    public int HealAmount { get; }
    public bool IsMyAttack { get; }
    public int AttackerSelfDamage { get; }
    public AttackAnimations Animations { get; }
    public MultiplayerCardAnimator CardAnimator { get; }
    public HealthBar PlayerLifeBar { get; }
    public HealthBar EnemyLifeBar { get; }
    public Func<string, IEnumerator> ShowDialog { get; }
    public List<Dictionary<string, object>> EffectsApplied { get; }
    public string AttackResult { get; }
    public List<Dictionary<string, object>> AttackerEffects { get; }
}

public sealed class AttackDefinition
{
    public AttackDefinition(string name, Func<AttackExecutionContext, IEnumerator> execute)
    {
        Name = name;
        Execute = execute;
    }

    public string Name { get; }
    public Func<AttackExecutionContext, IEnumerator> Execute { get; }
}

public static class AttackRegistry
{
    private static readonly Dictionary<int, AttackDefinition> Definitions =
        new Dictionary<int, AttackDefinition>
        {
            [1] = new AttackDefinition("Punch", ctx =>
                Attack1Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [2] = new AttackDefinition("Kick", ctx =>
                Attack2Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [3] = new AttackDefinition("Heal", ctx =>
                Attack3Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [4] = new AttackDefinition("Forgiveness", ctx =>
                Attack4Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [5] = new AttackDefinition("Crusade", ctx =>
                Attack5Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [6] = new AttackDefinition("Water To Wine", ctx =>
                Attack6Handler.Execute(ctx.Attacker, ctx.Animations, ctx.ShowDialog)
            ),
            [7] = new AttackDefinition("Car Hit", ctx =>
                Attack7Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackerEffects
                )
            ),
            [8] = new AttackDefinition("Monkey Wrench", ctx =>
                Attack8Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [9] = new AttackDefinition("Radiation", ctx =>
                Attack9Handler.Execute(ctx.Attacker, ctx.Defender, ctx.Animations, ctx.ShowDialog)
            ),
            [10] = new AttackDefinition("Scratch", ctx =>
                Attack10Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [11] = new AttackDefinition("Scientific Lecture", ctx =>
                Attack11Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [12] = new AttackDefinition("Chi Sau", ctx =>
                Attack12Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [13] = new AttackDefinition("One Inch Punch", ctx =>
                Attack13Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [14] = new AttackDefinition("Up In Smoke", ctx =>
                Attack14Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackerEffects
                )
            ),
            [15] = new AttackDefinition("Sing", ctx =>
                Attack15Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [16] = new AttackDefinition("Revolver", ctx =>
                Attack16Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [17] = new AttackDefinition("Artillery Regiment", ctx =>
                Attack17Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [18] = new AttackDefinition("Bloodthirst", ctx =>
                Attack18Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [19] = new AttackDefinition("Sword", ctx =>
                Attack19Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [20] = new AttackDefinition("Pike", ctx =>
                Attack20Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [21] = new AttackDefinition("Terrify", ctx =>
                Attack21Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [22] = new AttackDefinition("Drink Wine", ctx =>
                Attack22Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackerEffects
                )
            ),
            [23] = new AttackDefinition("Flaming Gun", ctx =>
                Attack23Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [24] = new AttackDefinition("Cleaver", ctx =>
                Attack24Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [25] = new AttackDefinition("Pan", ctx =>
                Attack25Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [26] = new AttackDefinition("Boost", ctx =>
                Attack26Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [27] = new AttackDefinition("Temptation", ctx =>
                Attack27Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [28] = new AttackDefinition("Shamshir", ctx =>
                Attack28Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [29] = new AttackDefinition("Diplomacy", ctx =>
                Attack29Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [30] = new AttackDefinition("Siege", ctx =>
                Attack30Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [32] = new AttackDefinition("Tomahawk", ctx =>
                Attack32Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [33] = new AttackDefinition("Peace Pipe", ctx =>
                Attack33Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [34] = new AttackDefinition("Recurve Bow", ctx =>
                Attack34Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [35] = new AttackDefinition("Fury", ctx =>
                Attack35Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [36] = new AttackDefinition("Guerilla", ctx =>
                Attack36Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [37] = new AttackDefinition("Famine", ctx =>
                Attack37Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [38] = new AttackDefinition("Marxism", ctx =>
                Attack38Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [39] = new AttackDefinition("Tesla Coil", ctx =>
                Attack39Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [40] = new AttackDefinition("Wireless Charger", ctx =>
                Attack40Handler.Execute(
                    ctx.Attacker,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [41] = new AttackDefinition("Experiment", ctx =>
                Attack41Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [42] = new AttackDefinition("Tommy Gun", ctx =>
                Attack42Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [43] = new AttackDefinition("Tie Up", ctx =>
                Attack43Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied
                )
            ),
            [44] = new AttackDefinition("Corruption", ctx =>
                Attack44Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [45] = new AttackDefinition("Colt 1911", ctx =>
                Attack45Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [46] = new AttackDefinition("Mortar", ctx =>
                Attack46Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [47] = new AttackDefinition("Great Army", ctx =>
                Attack47Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [49] = new AttackDefinition("Double Envelopment", ctx =>
                Attack49Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [50] = new AttackDefinition("Continental Blockade", ctx =>
                Attack50Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [51] = new AttackDefinition("Depression", ctx =>
                Attack51Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [52] = new AttackDefinition("Self Isolation", ctx =>
                Attack52Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [53] = new AttackDefinition("Knife", ctx =>
                Attack53Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [54] = new AttackDefinition("Autoportrait", ctx =>
                Attack54Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [55] = new AttackDefinition("Gravity Pull", ctx =>
                Attack55Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [56] = new AttackDefinition("Kamikaze", ctx =>
                Attack56Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [57] = new AttackDefinition("Take Off", ctx =>
                Attack57Handler.Execute(
                    ctx.Attacker,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [58] = new AttackDefinition("Air Strike", ctx =>
                Attack58Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [59] = new AttackDefinition("Justice Crusade", ctx =>
                Attack59Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [60] = new AttackDefinition("Rapier", ctx =>
                Attack60Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [61] = new AttackDefinition("Expeditionary Assault", ctx =>
                Attack61Handler.Execute(
                    ctx.Attacker,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [62] = new AttackDefinition("Culverin", ctx =>
                Attack62Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [63] = new AttackDefinition("Fire Ship", ctx =>
                Attack63Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [64] = new AttackDefinition("Handcuff Escape", ctx =>
                Attack64Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [65] = new AttackDefinition("Illusion", ctx =>
                Attack65Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [66] = new AttackDefinition("Carcano M91", ctx =>
                Attack66Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [67] = new AttackDefinition("Winchester", ctx =>
                Attack67Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [68] = new AttackDefinition("Ambush", ctx =>
                Attack68Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [69] = new AttackDefinition("Space Rocket", ctx =>
                Attack69Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [70] = new AttackDefinition("V-2", ctx =>
                Attack70Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [71] = new AttackDefinition("Battle Cry", ctx =>
                Attack71Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [72] = new AttackDefinition("Revelation", ctx =>
                Attack72Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [73] = new AttackDefinition("Standard", ctx =>
                Attack73Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [74] = new AttackDefinition("Pen", ctx =>
                Attack74Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [75] = new AttackDefinition("Iambic Pentameter", ctx =>
                Attack75Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [76] = new AttackDefinition("Ghost", ctx =>
                Attack76Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [77] = new AttackDefinition("Buffalo Horns", ctx =>
                Attack77Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [78] = new AttackDefinition("Iklwa", ctx =>
                Attack78Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [79] = new AttackDefinition("Iwisa", ctx =>
                Attack79Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [80] = new AttackDefinition("Niten Ichi-ryu", ctx =>
                Attack80Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [81] = new AttackDefinition("Tessenjutsu", ctx =>
                Attack81Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [82] = new AttackDefinition("Iaijutsu", ctx =>
                Attack82Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [83] = new AttackDefinition("Katana", ctx =>
                Attack83Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [84] = new AttackDefinition("Nodachi", ctx =>
                Attack84Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [85] = new AttackDefinition("Yumi", ctx =>
                Attack85Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [86] = new AttackDefinition("Jujutsu", ctx =>
                Attack86Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [87] = new AttackDefinition("Espionage", ctx =>
                Attack87Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog
                )
            ),
            [88] = new AttackDefinition("Sabre", ctx =>
                Attack88Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [89] = new AttackDefinition("Gamble", ctx =>
                Attack89Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [90] = new AttackDefinition("Philosophy", ctx =>
                Attack90Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [91] = new AttackDefinition("Calm", ctx =>
                Attack91Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [92] = new AttackDefinition("Honesty", ctx =>
                Attack92Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [93] = new AttackDefinition("Valaska", ctx =>
                Attack93Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [94] = new AttackDefinition("Moonshine", ctx =>
                Attack94Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackerEffects,
                    ctx.AttackResult
                )
            ),
            [96] = new AttackDefinition("Flintlock Pistol", ctx =>
                Attack96Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog
                )
            ),
            [97] = new AttackDefinition("Passive Resistance", ctx =>
                Attack97Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [98] = new AttackDefinition("Hunger Strike", ctx =>
                Attack98Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [99] = new AttackDefinition("Gladius", ctx =>
                Attack99Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [100] = new AttackDefinition("Shield Bash", ctx =>
                Attack100Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [101] = new AttackDefinition("Yperit", ctx =>
                Attack101Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [102] = new AttackDefinition("Blitzkrieg", ctx =>
                Attack102Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [103] = new AttackDefinition("Propaganda", ctx =>
                Attack103Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [104] = new AttackDefinition("Retiarius", ctx =>
                Attack104Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [105] = new AttackDefinition("Shuriken", ctx =>
                Attack105Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [106] = new AttackDefinition("Kusarigama", ctx =>
                Attack106Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [107] = new AttackDefinition("Ninjutsu", ctx =>
                Attack107Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [108] = new AttackDefinition("Oriental Spice", ctx =>
                Attack108Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [109] = new AttackDefinition("Arquebus", ctx =>
                Attack109Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [110] = new AttackDefinition("Pirate Raid", ctx =>
                Attack110Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [111] = new AttackDefinition("Axe", ctx =>
                Attack111Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [112] = new AttackDefinition("Jaguar Warriors", ctx =>
                Attack112Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [113] = new AttackDefinition("Atlatl", ctx =>
                Attack113Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [114] = new AttackDefinition("Macuahuitl", ctx =>
                Attack114Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [115] = new AttackDefinition("Cubism", ctx =>
                Attack115Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.HealAmount,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [116] = new AttackDefinition("La Cosa Nostra", ctx =>
                Attack116Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [117] = new AttackDefinition("Act a fool", ctx =>
                Attack117Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [118] = new AttackDefinition("Football", ctx =>
                Attack118Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.EffectsApplied,
                    ctx.AttackResult
                )
            ),
            [119] = new AttackDefinition("Bicycle Kick", ctx =>
                Attack119Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.AttackerSelfDamage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [120] = new AttackDefinition("World Champion", ctx =>
                Attack120Handler.Execute(
                    ctx.Attacker,
                    ctx.Animations,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [121] = new AttackDefinition("Shaolin Soccer", ctx =>
                Attack121Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
            [123] = new AttackDefinition("Curse", ctx =>
                Attack123Handler.Execute(
                    ctx.Attacker,
                    ctx.Defender,
                    ctx.Damage,
                    ctx.IsMyAttack,
                    ctx.Animations,
                    ctx.CardAnimator,
                    ctx.PlayerLifeBar,
                    ctx.EnemyLifeBar,
                    ctx.ShowDialog,
                    ctx.AttackResult
                )
            ),
        };

    public static string GetAttackName(int attackId)
    {
        return Definitions.TryGetValue(attackId, out AttackDefinition definition)
            ? definition.Name
            : $"Attack#{attackId}";
    }

    public static IEnumerator ExecuteOrFallback(int attackId, AttackExecutionContext context)
    {
        if (Definitions.TryGetValue(attackId, out AttackDefinition definition))
        {
            return definition.Execute(context);
        }

        Debug.LogWarning(
            $"[ExecuteAttackAnimation] Unknown attackId={attackId}, using Punch as fallback"
        );
        return Definitions[1].Execute(context);
    }
}











































