# Multiplayer smoke notes

## BUG-013 Closed / Ready For User Commit And Push (2026-11-10)

User accepts the functional smoke and explicitly declines a separate bulk data
migration: normal canonical writes remove old owned maxima. A36 readback is
already clean (128 valid cards); no other-account audit/write pending. No new
runtime changes or wiring in close-out. Existing battle MaxHealth transport is
intentional; owned-card HP has only Health. Full Unity baseline 494/494 GREEN.
Include BattleHealthOwnershipTests.cs and its .meta in the manual client commit;
unrelated user imports/font atlas remain untouched. No agent commit/push.
[Closure and final boundaries](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#closure-and-push-preparation).

### Prior Smoke And Readback

User PASS; PC/phone MP 777OG9 ten paired card states match. Caps survive damage,
healing, death and replacement; RR/Campaign preserve injured ratios, pack purchase
succeeds. No invalid-max/Unity error in supplied exports. Existing native graphics
diagnostics and Campaign missing background sound remain TASK-042/040.

Read-only A36FA8EC0F782700 recheck: 128 cards, all permanent Health valid, zero
MaxHealth/maxHealth fields. No agent remote write; a separate cleanup here is
unnecessary. Remaining close-out decision is any specifically authorized other
accounts, NOT a blind global migration. No new runtime/tool/scene change or test
rerun at this documentation checkpoint; prior Unity 494/494 remains baseline.
[Acceptance and data inspection](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#accepted-smoke-and-read-only-cleanup-check).

### Prior Single-Health Delivery

Approved and implemented: one permanent owned Health. Shared server loader
initializes both battle HP values once; strict snapshots/Start/bar ownership
stay unchanged. Generator validates HP, canonical writes omit retired maxima,
LibraryCardSorter uses permanent Health. Existing DTO retained; no new class.
All nine server npm suites pass; Unity targeted 73/73 and full 494/494 GREEN.

User: deploy updated server and rebuild phone client, start fresh MP, then short
RR/Campaign recheck of common entry conversion. No Inspector/new registration.
Do not manually backfill the 115 old zero maxima or patch old room snapshots.
No agent remote data write; separately approved backup/dry-run cleanup remains.
[Delivery, evidence and smoke](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#single-health-contract-delivery-and-smoke).

### Prior Whole-Lifecycle Analysis

Same-account authorized read-only audit found 122 valid permanent Health stats,
115 stored MaxHealth=0 and seven equal maxima. Old local generation omitted the
added field; common server fresh-match conversion now copies explicit zero.
The proposal AWAITING approval is owned Health only, explicit battle current/max
initialized once by shared server loader, validated creation and separately
reviewed data cleanup. Keep strict battle snapshots and Start/bar ownership.
No runtime or remote data changed during this reassessment; no other account or
live Mongo audit. Prior Unity 492/492 is baseline, not proof of this proposal.
Do not repeat smoke until contract/implementation is approved and delivered.
[Current findings and proposal](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#whole-lifecycle-reassessment).

### Prior MP Follow-Up Delivery

User manually corrected Sitting Bull; scoped PlayFab read-only recheck confirms
28/28. Approved existing-hand-manager guard validates explicit maxima before
hand creation and again after initialization before preparation succeeds.
Removed MP parsing's Health substitute; existing startup abort owns failure.
No new subsystem, popup, eligibility rule, scene wiring or server runtime change.

Agent-observed RED 34 pass/5 fail; targeted health/startup 71/71 GREEN and full
batch Unity 492/492 GREEN (XML, no skipped cases). No PlayFab data write by agent.
Rebuild client and test a fresh MP match with the repaired card; solo accepted.
[Exact delivery and repeat smoke](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#mp-follow-up-delivery-and-repeat-smoke).
No Inspector wiring or new server deployment. BUG-023/024 remain deferred.

### Prior MP Failure Checkpoint

User SP smoke passes; phone logs confirm stable ratios in RR/Campaign through
damage/heal/replacement. MP CTT7CQ fails before the first attack: PC Sitting Bull
has maxHealth=0. Explicitly authorized read of only that PlayFab card confirms
Health=28, MaxHealth=0. The removed Start overwrite previously hid corrupt data.
No player write or other-card audit. Earlier 485/485 tests missed the complete
persisted MP hand boundary; do not close the bug on that baseline.

Awaiting approval: one-card data correction and TDD validation of MP hand maxima
before startup readiness using existing owners/abort handling. No new fallback
or silent general repair. BUG-023 fan feedback and BUG-024 MP background remain
deferred; user build-profile/font-atlas changes must be preserved.

### Prior Approved HP Implementation

Approved client fix preserves maximum HP through Kard.Start, copies actual card
maximum for MP selection and rejects missing/nonpositive solo parsing/mapping
maxima through existing error handling. HealthBar no longer substitutes current
HP for maximum; invalid fill is hidden with an explicit diagnostic and restores
on valid binding. No new production type, scene wiring or animation timing change.

Targeted RED 8 pass/24 fail -> 32/32 GREEN; full Pipeline Unity 485/485 GREEN.
Server solo/RR/campaign/test:all GREEN; only server tests changed, so no new
server deployment/registration. No manual device acceptance claimed.
Run updated-client RR/Campaign/MP smoke from the
[delivery](../../../mega-tresk-server/docs/multiplayer/2026-11-10-bug-013-health-ownership-analysis.md#user-smoke-next).
Historical zero DTO maxima remain an evidence limit, not a guessed data repair.

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
