# Kill Counter System (Multiplayer)

## Purpose
Track round wins (card kills) and enforce match win condition.

## Behavior
- Increment killer side when opposing selected card reaches 0 HP and death is confirmed.
- Update UI counters immediately.
- Trigger end-of-match flow when threshold is reached.

## Data Source Priority
1. Server-confirmed card death (`battleResult`, refreshed selected cards).
2. Local UI state only for presentation.

## Invariants
- One death event increments exactly one side once.
- No double increment during retries/resync.
- Counter state survives card replacement between rounds.

## Debug Signals
- Kill increment logs include dead cardId and winner side.
- End condition logs include current counts and threshold.
