# Attack Selection System (Multiplayer)

## Purpose
Client-side orchestration of attack selection and submission in multiplayer with server-authoritative result processing.

## Main Responsibilities
- Show attack names and counts for selected card.
- Enable attack UI only when both selected cards are known and local counts are loaded.
- Submit selected attack once, then lock UI until server response.
- Delegate battle processing to battle result/timeline layer.

## Flow
1. Player selects card (`setSelectedCard`).
2. Client refreshes selected cards (`getSelectedCards`).
3. Client loads attack counts (`getAttackCounts`).
4. Attack buttons become active when prerequisites are met.
5. Player confirms attack; client sends `executeBattle`.
6. Server returns `battleResult` + `timelineV2`; client renders steps and updates HP/effects.

## Invariants
- Counts come from server, never from local prediction.
- No second submission while request is in flight.
- UI state follows authoritative server state after each turn refresh.

## Failure Handling
- Temporary request failure: retry via networking layer.
- Missing counts before card set: expected race, retry and continue.
- If state desync is detected, force refresh via `getSelectedCards`.
