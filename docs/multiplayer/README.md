# Multiplayer smoke notes

## BUG-019 Entry Guard Accepted And Closed (2026-11-10)

Main MP/Bushido entry checks canonical server readiness under the standard
loading overlay. Missing deck uses its own authored notice; failure to verify
uses a distinct notice, not a false missing-deck claim. Server independently
checks before new MP membership/seat claim and campaign session creation.
Existing MP RoomCode goes through unchanged Lobby reconciliation.

UI is already created/wired in Main/Lobby: three new authored prefabs under
Assets/Prefabs/ModeEntry and Main's ModeEntryLoading. Text lives in those prefabs;
no runtime copy overwrite, no use of the timeout/coward popup. No manual Inspector
work required. New popup layout is relative; global Canvas settings are unchanged.

Final Unity Pipeline 453/453 GREEN; server suites and 55 isolated Mongo cases
GREEN. User reports smoke PASS; supplied client/server logs confirm MP/Bushido
admission for an eligible account and repeated authored denial for an ineligible
account. getModeEntryState registration/deployment now works. No fresh full MP
match or offline-device run is inferred from this entry smoke. User explicitly
keeps immediate loading; no delay/cache change. Closure reran server test:decks
successfully; Unity 453/453 is the prior implementation baseline, not a new run.
[Acceptance and final regression checklist](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-019-mode-entry-deck-guard-delivery.md#acceptance-and-closure).
[Unity CLI workflow](../UNITY_CLI_WORKFLOW.md) records the working tool setup.

## Accepted Timeout Recovery Checkpoint (2026-11-09)

BUG-004/BUG-011 core started-match expiry/recovery is user accepted and closed.
Read the sibling
server [approved contract and delivery](../../../mega-tresk-server/docs/multiplayer/2026-11-08-started-match-presence-expiry.md).
Server rule is 60 seconds since last confirmed presence; MultiplayerService and
the saved Multiplayer scene use ten-second heartbeat. Existing polling discovers
expiry independently of the absent client's return. Timeout nonwinners return
to Lobby with the existing single-use account-scoped MatchUnavailableDialog,
including retained completed rooms. Connected winner keeps the canonical result;
both expired have no winner. Normal battle-result playback remains unchanged.

That earlier core correction needed no new production class, Inspector wiring
or popup. The user's saved MultiplayerLobby visuals were preserved; its scene
edit was the Multiplayer heartbeat value 30 -> 10. BUG-019's separately approved
new UI/scene wiring is recorded above. EXNWH1 long-timeout/Lobby return and Y8M4YL
short resume with continued gameplay are accepted. Recorded automation is full
Unity 426/426, server test:all and isolated MongoDB GREEN, not new closure runs.

Remaining both-absent/no-winner, fresh Connect after acknowledgement and original
purged-room edge matrix move to TASK-019 final release smoke, not new device PASS.
Use two accounts with playable MP decks. No manual RoomCode deletion/reset.
BUG-019 entry deck checks, countdown/blocker and attack timer are not implemented
by this correction. Detailed automated evidence is kept in the linked delivery.

Resolved BUG-022: Lobby uses existing ConfirmDialogController's new
ShowAuthored without writing text. Removed old Lobby copy field/constant and
its serialized override; user's artwork, TMP copy and button wiring remain.
One existing popup is approved for timeout and missing old match, not unrelated
situations. Targeted 38/38 and full automatic Unity 433/433 GREEN; no new Inspector
wiring or server deploy. User reports FGHC3J real-popup visual smoke PASS on
2026-11-09; accepted/closed. Logs show server expiry before phone Lobby recovery.
One post-recovery Heartbeat failed remains an unclassified null-result diagnostic
for TASK-019 triage, not a proved server fault or reason to suppress the log.
[Delivery](../../../mega-tresk-server/docs/multiplayer/2026-11-09-authored-recovery-popup.md).

Former BUG-021 is retired as a standalone bug: only a TASK-019 final-QA check
for continuation after a long replacement wait remains (tester stopped selecting;
no failed selection was reproduced). The approved core/popup correction is closed.
Native crash/freeze BUG-020 stays a
known open risk; repair is deferred until after first release but required before
unrestricted public availability. No graphics API
change was made and one successful Vulkan resume is not a crash fix. Do not
continue the old GLES3 diagnostic approval without resuming that separate topic.

## Historical A9 Resume Point (2026-10-06)

Active topic: TASK-012 / A9 Startup Readiness And No-Contest. Approved code and
automation are complete (Unity PlayMode Tests 239/239); coordinated server/client
deployment, PlayFab ready-endpoint registration and device acceptance are pending.
Read the [A9 cross-PC testing handover](../../../mega-tresk-server/docs/multiplayer/2026-10-06-a9-testing-handover.md)
in the sibling server repository before testing or changing code. Server source of
truth is mega-tresk-server/api, never the deprecated vercel-api mirror in this repo.

No new A9 Inspector wiring is required. Transfer both repositories, including the
new MultiplayerStartupRegressionTests.cs and its .meta and the user's saved lobby
scene edit. Keep Unity 6000.0.80f1. Do not manually clear RoomCode to bypass recovery.
Automatic offline waiting-search abandonment remains a separate future step.

The notes below are the historical pre-A8/A9 baseline, not current lifecycle/error
policy. In particular, startup creates active visuals behind loading but blocks
interaction until both clients acknowledge readiness; expected leave/terminal
outcomes must be validated for the exact room, not silently suppressed.

## Historical Baseline

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
