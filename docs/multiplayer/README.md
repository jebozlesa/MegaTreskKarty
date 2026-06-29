# Multiplayer smoke notes

This note captures the current expected state of the Unity multiplayer client after the recent battle-flow cleanup.

## Current loading baseline

- `MultiplayerLobby` starts with a visible `CONNECT` label on the main button.
- Clicking `CONNECT` shows the scene-local loading overlay and switches the same button to `CANCEL`.
- Lobby loading text is owned by `LoadingOverlayView` and should move through `CONNECTING...` and `SEARCHING FOR PREY...`.
- Clicking `CANCEL` should reset the lobby without requiring a scene reload.
- `Multiplayer` battle startup should show the loading overlay while startup UI roots stay hidden.
- Loading overlay graphics must not consume raycasts in the lobby, otherwise `CANCEL` will stop working.

## What was intentionally cleaned up

- Raw `CallFunction` / `ExecuteFunction result` payload dumps are now treated as verbose logs.
- Raw `[BATTLE_RESULT]` dictionary dumps are now treated as verbose logs.
- Per-refresh `[REFRESH]` spam and lobby JSON dumps are now treated as verbose logs.

The goal is to keep the Console readable during real smoke tests while preserving actionable warnings and errors.

## What still counts as normal

- `leaveRoom` can fail with `Player not in any room` during lobby cleanup.
- `[ServerFunctionsManager] Suppressed network indicator for expected leaveRoom error`
  is expected for that case.
- Match-state polling can briefly show phase transitions like `waiting_for_result_ack`,
  `waiting_for_replacement`, and `selecting_attacks` in quick succession.

## Known editor/runtime artifacts to verify manually

These were observed in smoke logs, but they are not currently traceable to active source in this repo:

- `The referenced script (Unknown) on this Behaviour is missing!`
- `LeaveRoom: callback is null!`

If either message appears again after a clean Unity domain reload / scene reopen, treat it as an editor-side follow-up task:

1. Reopen the affected scene or prefab and inspect newly instantiated objects.
2. Check runtime-added components and disabled prefab variants.
3. Re-run the smoke test before touching server logic, because the battle flow itself currently looks healthy.

## Smoke-test baseline

The current client/server flow was smoke-tested through:

- lobby cleanup and rejoin
- lobby connect/cancel loading loop
- deck loading
- fighter selection
- attack submission / waiting states
- ongoing-action playback (`trident`, `siege`)
- damage synchronization
- death handling
- replacement selection

## Regression signals worth treating seriously

- missing `CONNECT` label on lobby load
- loading overlay staying interactive and blocking `CANCEL`
- overlay text not changing between `CONNECTING...` and `SEARCHING FOR PREY...`
- repeated network indicator popups for expected `leaveRoom` cleanup
- battle results missing `firstAttacker` / `secondAttacker`
- selected card HP diverging from server refresh
- phase getting stuck after both players are ready
- replacement flow not clearing dead cards
