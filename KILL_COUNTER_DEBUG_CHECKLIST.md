# Kill Counter Debug Checklist

## Quick Checks
- Manager component exists in active multiplayer scene.
- Manager is enabled and initialized once.
- Indicator references are not null.

## Event Checks
- Death event is fired for the card only once.
- Kill increment path receives correct winner/loser card IDs.
- Retry/resync does not re-fire increment for same death.

## Sync Checks
- Local HP and server HP converge after refresh.
- Selected card is cleared on server when dead.
- Replacement card does not inherit previous kill event.

## Logging Minimum
- Scene init log.
- Kill increment log with room/card IDs.
- Win-condition log with current counts.
