# Card Replacement System (Multiplayer)

## Purpose
Handle dead-card cleanup and replacement without stale battle state.

## Core Behavior
- Detect card death from `battleResult` / timeline and clamped HP.
- Clear dead selected card on server (`clearSelectedCards`).
- Keep room alive and wait for new card selection.
- Re-sync selected cards and counts before enabling attack UI.

## Required Server Calls
- `clearSelectedCards`
- `setSelectedCard`
- `getSelectedCards`
- `getAttackCounts`

## Critical Rules
- Never keep dead card in `selectedCards`.
- Never enable attacks with stale card/count data.
- Always rehydrate local card/effect state from server after replacement.

## Common Pitfalls
- Stale local UI after replacement -> run refresh pipeline again.
- Missing counts immediately after selection -> retry `getAttackCounts`.
- Zombie battle state -> clear selection and resync before next turn.
