# Multiplayer UI Setup Instructions

## Must-Have UI

- Player card area, enemy card area.
- HP bars for both sides.
- Attack buttons (4) and confirm action.
- A `Loading` overlay prefab in scenes that bootstrap multiplayer network flow.
- `LoadingOverlayView.messageText` assigned to the loading TMP label.
- Optional leave/exit button.

## Behavior Rules

- Lobby starts with a visible `CONNECT` label and reuses the same button as `CANCEL` while matchmaking is in progress.
- Lobby loading copy lives in the loading overlay (`CONNECTING...`, `SEARCHING FOR PREY...`), not in a separate status text.
- Loading overlay graphics must not block button clicks when the design expects the underlying button to stay interactive.
- Attack controls disabled until both cards are available and counts loaded.
- Controls locked during in-flight submit.
- UI refreshes from server state after each turn.

## Startup Loading Rules

- Multiplayer lobby scene should contain the loading prefab even though it starts inactive.
- Multiplayer battle scene should hide startup combat UI while the loading overlay is visible.
- Any scene-local loading text must be driven through `LoadingOverlayView`, not by hierarchy search or ad-hoc text lookups.

## Exit Flow

- Leave action should call room leave/cleanup path safely.
- UI should prevent duplicate leave requests.

## Validation

- `CONNECT` is visible immediately after the lobby scene loads.
- Clicking `CONNECT` shows the loading overlay and changes the button label to `CANCEL`.
- Clicking `CANCEL` returns the lobby to a clean idle state and is not blocked by the loading overlay.
- Battle startup hides the loading overlay after decks/cards bootstrap finishes or fails.
- UI does not show stale HP/effects after refresh.
- Error banner appears on transient failures and clears on recovery.
