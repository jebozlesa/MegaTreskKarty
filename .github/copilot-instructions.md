# Copilot Instructions - MegaTreskKarty

Last updated: 2026-11-02

## Goal

Keep multiplayer battle behavior consistent while allowing isolated attack iteration.

## Collaboration Style

- Use the two-step workflow for runtime changes: first analyze the current code,
  ask necessary questions, and present a scoped proposal; implement only after
  the user explicitly approves that proposal.
- Prefer small, focused changes.
- Do not mix unrelated refactors into gameplay fixes.
- Preserve existing contracts unless explicitly changing them.
- If contract changes, update client + server + tests + docs in the same change.

## Source of Truth

- Server logic and effects are authoritative.
- Client is primarily renderer/state synchronizer for server results.
- Do not move game rules to client if server already decides outcome.
- `timelineV2` is validated as a whole before playback. Unknown attacks or
  malformed executable combo payloads fail validation; do not restore a Punch
  fallback, partial timeline playback, or a second local battle engine.

## Attack Architecture (Client)

- Attack visuals/flow per attack live in `Assets/Scripts/Multiplayer/AttackHandlers/Attack{ID}Handler.cs`.
- Shared processing lives in `BattleResultProcessor` and related multiplayer managers.
- Keep per-attack behavior isolated. Avoid coupling one handler with another.
- Stat changes from server contract or `timelineV2` must be applied only by shared playback code, never directly inside an attack handler.
- Attack handlers must not mutate card stats directly for server-driven outcomes; otherwise buffs/debuffs can render twice.

## ID-Based Rules

- Always identify entities by `playerId` and `cardId`.
- Avoid position-based assumptions (`card1/card2`, `first/second`) except explicit role from server response.

## Effect Handling Rules

- Ordered timeline events are the executable effect playback source. Aggregate
  result fields remain diagnostics/read-model data unless an active contract
  explicitly names a consumer; do not replay the same mutation from both paths.
- Respect server identity and control fields (`blocked`, `blockedBy`, card IDs,
  effect IDs, removal reasons, etc.).
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

