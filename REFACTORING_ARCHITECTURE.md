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

## Why This Matters
- Changing one attack should not break others.
- Shared logic stays centralized.
- Tests can target attack contracts and timeline semantics.
