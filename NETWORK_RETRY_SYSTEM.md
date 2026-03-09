# Network Retry System (Multiplayer)

## Goal
Mask transient cloud/database issues and keep match flow usable.

## Strategy
- Retry critical calls with bounded attempts.
- Show temporary network error UI while retries run.
- Hide network error once any retry succeeds.

## Calls That Must Be Retried
- `executeBattle`
- `markReadyForNextTurn`
- `getAttackCounts`
- `getSelectedCards`
- `clearSelectedCards`
- `loadPlayerDecksIntoRoom` / `getRoomDecks`

## Rules
- Retries must be idempotent-safe.
- Do not duplicate gameplay side effects from client side.
- Final failure must surface clear message and safe fallback state.

## Diagnostics
Log function name, attempt count, final outcome, and room/player context.
