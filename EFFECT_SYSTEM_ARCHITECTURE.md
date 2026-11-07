# Effect System Architecture (Multiplayer)

Last updated: 2026-11-02

## Principle
Server is authoritative for effect application, duration, ticking, and removal.

## What Client Does
- Validate and play the complete server `timelineV2` contract.
- Render effect icons, animations, and values from ordered timeline events;
  server snapshots hydrate final displayed state between turns.
- Apply HP/effect visuals in timeline order.
- Refresh from server-selected card snapshots between turns.

## What Server Does
- Resolve attack logic.
- Apply/stack/remove effects.
- Process DOT ticks (for example bleed) at defined turn phase.
- Return consistent `battleResult` and `timelineV2`.

## Ordering Requirements
- Timeline order is source of truth for visuals.
- Each authoritative mutation has one executable timeline owner. Aggregate
  result fields must not reconstruct duplicate effect playback.
- Death check must respect effect ticks and blocked/skip semantics.
- Client must not invent effect durations.
- Unknown attacks and malformed executable combo payloads fail whole-timeline
  validation; there is no Punch or partial-playback fallback.

## Regression Checklist
- Effect icon appears/disappears according to server `effects`.
- Passive HP changes match typed timeline `Damage`/`Heal` steps and their
  `effectType`; retired tick aliases are not replayed.
- Blocked attacks still preserve effect-side rules from server output.
