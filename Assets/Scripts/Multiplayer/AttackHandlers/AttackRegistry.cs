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












