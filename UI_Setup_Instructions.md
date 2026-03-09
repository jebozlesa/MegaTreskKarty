# Multiplayer UI Setup Instructions

## Must-Have UI
- Player card area, enemy card area.
- HP bars for both sides.
- Attack buttons (4) and confirm action.
- Status/error text for loading/retry states.
- Optional leave/exit button.

## Behavior Rules
- Attack controls disabled until both cards are available and counts loaded.
- Controls locked during in-flight submit.
- UI refreshes from server state after each turn.

## Exit Flow
- Leave action should call room leave/cleanup path safely.
- UI should prevent duplicate leave requests.

## Validation
- UI does not show stale HP/effects after refresh.
- Error banner appears on transient failures and clears on recovery.
