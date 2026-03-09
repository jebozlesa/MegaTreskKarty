# Copilot Instructions - MegaTreskKarty

Last updated: 2026-03-09

## Goal

Keep multiplayer battle behavior consistent while allowing isolated attack iteration.

## Collaboration Style

- Prefer small, focused changes.
- Do not mix unrelated refactors into gameplay fixes.
- Preserve existing contracts unless explicitly changing them.
- If contract changes, update client + server + tests + docs in the same change.

## Source of Truth

- Server logic and effects are authoritative.
- Client is primarily renderer/state synchronizer for server results.
- Do not move game rules to client if server already decides outcome.

## Attack Architecture (Client)

- Attack visuals/flow per attack live in `Assets/Scripts/Multiplayer/AttackHandlers/Attack{ID}Handler.cs`.
- Shared processing lives in `BattleResultProcessor` and related multiplayer managers.
- Keep per-attack behavior isolated. Avoid coupling one handler with another.

## ID-Based Rules

- Always identify entities by `playerId` and `cardId`.
- Avoid position-based assumptions (`card1/card2`, `first/second`) except explicit role from server response.

## Effect Handling Rules

- Respect server fields (`blocked`, `blockedBy`, `effectsApplied`, `attackerEffectsApplied`, `wokeUp`, etc.).
- Do not infer alternate outcomes when server already provides explicit state.
- Keep icon and animation behavior deterministic and role-correct.

## Logging

- Use `Debug.LogWarning` for important runtime diagnostics.
- Use `Debug.LogError` for failures.
- Avoid relying only on `Debug.Log` for critical diagnostics.

## Test/Validation Expectations

For multiplayer attack changes, validate:

1. First-attacker and second-attacker paths.
2. Blocked vs non-blocked outcomes.
3. Self-damage/effect edge cases.
4. Card death transitions and selected-card replacement flow.
5. No regressions in other attack handlers.

## Documentation Discipline

When behavior changes, update relevant active docs only:

- `CARD_DEATH_SYSTEM.md`
- `CARD_CREATION_GUIDE.md`
- `CARD_REPLACEMENT_SYSTEM.md`
- `ATTACK_SELECTION_SYSTEM.md`
- `EFFECT_SYSTEM_ARCHITECTURE.md`
- `NETWORK_RETRY_SYSTEM.md`
- `UNITY_BATTLE_SETUP.md`
- `DOCUMENTATION_INDEX.md`

Avoid creating new versioned one-off docs unless explicitly requested.
