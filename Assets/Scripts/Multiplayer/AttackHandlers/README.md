# Attack Handlers

Client multiplayer attack playback lives here.

Last updated: 2026-04-24

## Current Status

- Implemented handlers: `121/123`
- Implemented IDs: `1-30`, `32-47`, `49-123`
- Missing IDs: `31`, `48`

## Core Files

- `Attack{ID}Handler.cs` - per-attack playback/animation orchestration
- `AttackRegistry.cs` - central client registry and dispatch
- `ComboAttackPlayback.cs` - shared combo/composite attack playback
- `BattleValuePlayback.cs` - shared damage/heal playback
- `BattleStatPlayback.cs` - shared stat playback
- `BattleEffectPlayback.cs` - shared effect playback

## Important Rule

Attack handlers should focus on:

- attack animation sequencing
- branch-specific visuals
- dialogs
- exceptional timing only when really needed

Attack handlers should not reimplement ordinary shared battle mutation.

Use shared playback for:

- target damage
- self damage
- heals
- stat changes
- effect icon/effect-end visuals

## Registry Pattern

New handlers are wired through `AttackRegistry.cs`, not through a giant switch in `BattleResultProcessor.cs`.

Standard flow:

1. create `Attack{ID}Handler.cs`
2. add `[ID] = new AttackDefinition(...)` to `AttackRegistry.cs`
3. add the file to `Assembly-CSharp.csproj`
4. update this README
5. update `Assets/Tests/BattleRecentAttackCoverageTests.cs`

## Combo Attacks

Composite attacks such as `107` and `121` should use:

- server: serialized combo payload in `attackResult`
- client: `ComboAttackPlayback.cs`

Do not build one-off combo playback when the shared helper fits.

## Implemented Handler Files

```text
Attack1Handler.cs   - Punch
Attack2Handler.cs   - Kick
Attack3Handler.cs   - Heal
Attack4Handler.cs   - Forgiveness
Attack5Handler.cs   - Crusade
Attack6Handler.cs   - Water To Wine
Attack7Handler.cs   - Car Hit
Attack8Handler.cs   - Monkey Wrench
Attack9Handler.cs   - Radiation
Attack10Handler.cs  - Scratch
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
Attack34Handler.cs  - Recurve Bow
Attack35Handler.cs  - Fury
Attack36Handler.cs  - Guerilla
Attack37Handler.cs  - Famine
Attack38Handler.cs  - Marxism
Attack39Handler.cs  - Tesla Coil
Attack40Handler.cs  - Wireless Charger
Attack41Handler.cs  - Experiment
Attack42Handler.cs  - Tommy Gun
Attack43Handler.cs  - Tie Up
Attack44Handler.cs  - Corruption
Attack45Handler.cs  - Colt 1911
Attack46Handler.cs  - Mortar
Attack47Handler.cs  - Great Army
Attack49Handler.cs  - Double Envelopment
Attack50Handler.cs  - Continental Blockade
Attack51Handler.cs  - Depression
Attack52Handler.cs  - Self Isolation
Attack53Handler.cs  - Knife
Attack54Handler.cs  - Autoportrait
Attack55Handler.cs  - Gravity Pull
Attack56Handler.cs  - Kamikaze
Attack57Handler.cs  - Take Off
Attack58Handler.cs  - Air Strike
Attack59Handler.cs  - Justice Crusade
Attack60Handler.cs  - Rapier
Attack61Handler.cs  - Expeditionary Assault
Attack62Handler.cs  - Culverin
Attack63Handler.cs  - Fire Ship
Attack64Handler.cs  - Handcuff Escape
Attack65Handler.cs  - Illusion
Attack66Handler.cs  - Carcano M91
Attack67Handler.cs  - Winchester
Attack68Handler.cs  - Ambush
Attack69Handler.cs  - Space Rocket
Attack70Handler.cs  - V-2
Attack71Handler.cs  - Battle Cry
Attack72Handler.cs  - Revelation
Attack73Handler.cs  - Standard
Attack74Handler.cs  - Pen
Attack75Handler.cs  - Iambic Pentameter
Attack76Handler.cs  - Ghost
Attack77Handler.cs  - Buffalo Horns
Attack78Handler.cs  - Iklwa
Attack79Handler.cs  - Iwisa
Attack80Handler.cs  - Niten Ichi-ryu
Attack81Handler.cs  - Tessenjutsu
Attack82Handler.cs  - Iaijutsu
Attack83Handler.cs  - Katana
Attack84Handler.cs  - Nodachi
Attack85Handler.cs  - Yumi
Attack86Handler.cs  - Jujutsu
Attack87Handler.cs  - Espionage
Attack88Handler.cs  - Sabre
Attack89Handler.cs  - Gamble
Attack90Handler.cs  - Philosophy
Attack91Handler.cs  - Calm
Attack92Handler.cs  - Honesty
Attack93Handler.cs  - Valaska
Attack94Handler.cs  - Moonshine
Attack95Handler.cs  - Outlaw Band
Attack96Handler.cs  - Flintlock Pistol
Attack97Handler.cs  - Passive Resistance
Attack98Handler.cs  - Hunger Strike
Attack99Handler.cs  - Gladius
Attack100Handler.cs - Shield Bash
Attack101Handler.cs - Yperit
Attack102Handler.cs - Blitzkrieg
Attack103Handler.cs - Propaganda
Attack104Handler.cs - Retiarius
Attack105Handler.cs - Shuriken
Attack106Handler.cs - Kusarigama
Attack107Handler.cs - Ninjutsu
Attack108Handler.cs - Oriental Spice
Attack109Handler.cs - Arquebus
Attack110Handler.cs - Pirate Raid
Attack111Handler.cs - Axe
Attack112Handler.cs - Jaguar Warriors
Attack113Handler.cs - Atlatl
Attack114Handler.cs - Macuahuitl
Attack115Handler.cs - Cubism
Attack116Handler.cs - La Cosa Nostra
Attack117Handler.cs - Act a fool
Attack118Handler.cs - Football
Attack119Handler.cs - Bicycle Kick
Attack120Handler.cs - World Champion
Attack121Handler.cs - Shaolin Soccer
Attack122Handler.cs - Sport Skills
Attack123Handler.cs - Curse
ComboAttackPlayback.cs - shared combo playback helper
```
