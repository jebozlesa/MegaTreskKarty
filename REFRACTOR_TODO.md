## Multiplayer Timeline Refactor TODOs

### Server: Fresh sleep should block second attacker in same round
- Context: manual test "second attacker blocked by fresh sleep" still lets 2nd attack execute.
- Root cause: server does not set `blocked=true`/`blockedBy=3` for second attacker when sleep/knockout is freshly applied by first attacker in the same round.
- Client workaround was rejected (do not implement on client).
- Target fix: M8 (server timelineV2 / executeBattle.js) should emit blocked step or set blocked flags consistently.

