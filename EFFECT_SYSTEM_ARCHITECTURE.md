# Effect System Architecture (Multiplayer)

## Principle
Server is authoritative for effect application, duration, ticking, and removal.

## What Client Does
- Render icons and animations from server result (`effects`, `effectsApplied`, timeline steps).
- Apply HP/effect visuals in timeline order.
- Refresh from server-selected card snapshots between turns.

## What Server Does
- Resolve attack logic.
- Apply/stack/remove effects.
- Process DOT ticks (for example bleed) at defined turn phase.
- Return consistent `battleResult` and `timelineV2`.

## Ordering Requirements
- Timeline order is source of truth for visuals.
- Death check must respect effect ticks and blocked/skip semantics.
- Client must not invent effect durations.

## Regression Checklist
- Effect icon appears/disappears according to server `effects`.
- DOT damage in HP bars matches timeline `Damage`/`BleedTick`.
- Blocked attacks still preserve effect-side rules from server output.
