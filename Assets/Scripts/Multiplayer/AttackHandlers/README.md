# Attack Handlers

Modularna struktura pre attack animacie a logiku v multiplayer systeme.

##  Struktura

```
AttackHandlers/
  Attack1Handler.cs   - Punch (Sleep 20%)
  Attack2Handler.cs   - Kick (Crit 20%)
  Attack3Handler.cs   - Heal (Self-heal + cleanse)
  Attack4Handler.cs   - Forgiveness (Asceticism 75%)
  Attack5Handler.cs   - Crusade (STR damage + DEF debuff)
  Attack6Handler.cs   - WaterToWine (Self buff)
  Attack7Handler.cs   - CarHit (Random damage, multi-effect)
  Attack8Handler.cs   - MonkeyWrench (STR/2 + crit/sleep)
  Attack9Handler.cs   - Radiation (Exposure risk)
  Attack10Handler.cs  - Scratch (Bleed 20%)
  Attack11Handler.cs  - Scientific Lecture
  Attack12Handler.cs  - Chi Sau
  Attack13Handler.cs  - One Inch Punch
  Attack14Handler.cs  - Up In Smoke
  Attack15Handler.cs  - Sing
  Attack16Handler.cs  - Revolver
  Attack17Handler.cs  - Artillery Regiment
  Attack18Handler.cs  - Bloodthirst
  Attack19Handler.cs  - Sword
  Attack20Handler.cs  - Pike
  Attack21Handler.cs  - Terrify
  Attack22Handler.cs  - Drink Wine
  Attack23Handler.cs  - Flaming Gun
  Attack24Handler.cs  - Cleaver
  Attack25Handler.cs  - Pan
  Attack26Handler.cs  - Boost
  Attack27Handler.cs  - Temptation
  Attack28Handler.cs  - Shamshir
  Attack29Handler.cs  - Diplomacy
  Attack30Handler.cs  - Siege
  Attack32Handler.cs  - Tomahawk
  Attack33Handler.cs  - Peace Pipe
  Attack35Handler.cs  - Fury
  Attack36Handler.cs  - Guerilla
  Attack37Handler.cs  - Famine
  Attack38Handler.cs  - Marxism
  Attack39Handler.cs  - Tesla Coil
  Attack40Handler.cs  - Wireless Charger
  Attack41Handler.cs  - Experiment
  ...
  Attack123Handler.cs
```

##  Pattern

Kazdy handler ma static Execute() metodu:

```csharp
public class Attack{ID}Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        bool isMyAttack,
        AttackAnimations animations,
        CardAnimator cardAnimator,
        HPBar playerLifeBar,
        HPBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses AttackName!");
        yield return animations.PlayAttackAnimation(...);
        
        // Apply damage/heal/buffs/debuffs
        // Update HP bars
        // Show result dialog
    }
}
```

## [RETRY] Routing

`BattleResultProcessor.cs` -> `ExecuteAttackAnimation()` vola handler:

```csharp
switch (attackId)
{
    case 1:
        yield return Attack1Handler.Execute(...);
        break;
    case 2:
        yield return Attack2Handler.Execute(...);
        break;
    // ... 123 cases total
}
```

##  Pridat dalsi Attack

1. **Create handler:** `Attack{ID}Handler.cs`
2. **Add case:** BattleResultProcessor.cs (+3 lines)
3. **Done!**

## Stat Rules

- Attack handlers must not call HandleAttack/HandleStrength/HandleDefense/HandleKnowledge/HandleSpeed/HandleCharisma directly for battle-result stat changes.
- Attack handlers must not call AnimateStatChange(...) directly for battle-result stat changes.
- BattleResultProcessor and attack handlers should use BattleStatPlayback for shared stat mutation + popup playback.
- BattleResultProcessor and attack handlers should use BattleEffectPlayback for shared effect icon and effect-end visual playback.
- Server-driven stat changes are rendered only through shared battle playback/timeline flow in BattleResultProcessor.
- Standard target damage and self-heal playback should default to BattleValuePlayback shared helpers.
- New handlers should focus on attack animation, sequencing, special-case visuals, and dialogs.
- Do not add fallback damage logic in BattleResultProcessor for ordinary attack damage.
- Do not add new ordinary attack handlers that manually subtract defender HP unless the mechanic truly requires custom damage timing.
- Explicit timing exceptions such as Attack7Handler and Attack41Handler must stay documented and intentional.

##  Vyhody

- **Modularnost:** 123 suborov po ~30-100 lines vs 1 switch 2000+ lines
- **Udrzba:** Kazdy utok samostatne testovatelny
- **Skalovatelnost:** Attack 123 = 123 handlers + minimal router
- **Citatelnost:** Attack9Handler.cs = iba Radiation logic

##  Server Parity

Rovnaka struktura ako server:
- **Server:** `api/attacks/implementations/attack{ID}.js`
- **Unity:** `AttackHandlers/Attack{ID}Handler.cs`

Progress: 41/123 attacks (33.3%)























