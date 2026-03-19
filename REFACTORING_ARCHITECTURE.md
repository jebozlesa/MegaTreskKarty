# Refactoring Architecture (Current)

## High-Level Split
- Server: attack/effect/death rules, timeline generation, authoritative state updates.
- Client: UI orchestration, timeline playback, local visual state syncing.

## Client Modules
- Attack selection and submit flow.
- Battle result/timeline processing.
- Card replacement and selected-cards refresh.
- Network retry and user-facing error state.

## Server Modules
- `executeBattle` orchestration.
- Per-attack handlers.
- Effect manager.
- Room/selection persistence.

## Stat Playback Rule
- Server stat changes are authoritative and must be rendered once.
- Client attack handlers do not mutate stats directly for battle-result changes.
- BattleResultProcessor shared playback and 	imelineV2 StatChange steps are the only valid path for multiplayer buff/debuff application.

## Why This Matters
- Changing one attack should not break others.
- Shared logic stays centralized.
- Tests can target attack contracts and timeline semantics.


