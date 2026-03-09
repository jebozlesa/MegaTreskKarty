# Card Death System

Last updated: 2026-03-09

## Purpose

Defines how multiplayer handles card death on client side after server battle results.

## Responsibilities

1. Detect card death from synchronized card state (`health <= 0`).
2. Remove dead card from board UI.
3. Sync dead-card clear with server selected-card state.
4. Transition game state correctly (select replacement / win / lose).

## Canonical Flow

1. Server resolves turn and returns battle result.
2. Client applies animations/effects/HP updates.
3. `BattleResultProcessor` evaluates outcome.
4. For each dead card:
   - remove from board owner,
   - clear selected card on server (`clearSelectedCards` with specific cardId),
   - refresh selected cards state.
5. If player has cards in hand, enter replacement selection flow.
6. If no valid replacement, enter terminal state (lost/won as applicable).

## Required Invariants

- Dead card must not remain as active board card reference.
- Dead card must not stay in room selectedCards.
- UI and server state must converge before next attack submission.
- Replacement card selection reuses existing drag/drop + select-card pipeline.

## Integration Points

- `BattleResultProcessor` (death detection + transitions)
- `ServerFunctionsManager` (`clearSelectedCards`, `getSelectedCards`)
- Board/hand manager responsible for unlock/select flow
- Fight state machine (`PLAYERDEATH`, `WON`, `LOST`)

## Edge Cases to Validate

- Both cards die in same turn.
- Player card dies while enemy survives.
- Enemy card dies while player survives.
- Dead card clear succeeds after retry.
- Replacement selected while opponent also replacing.

## Validation Checklist

- Card GameObject removed from board.
- Active card refs cleaned.
- Server selected card entry removed for dead cardId.
- No attack submission uses a dead card.
- Next turn starts only after valid active cards are selected.

## Related Docs

- `CARD_REPLACEMENT_SYSTEM.md`
- `ATTACK_SELECTION_SYSTEM.md`
- `NETWORK_RETRY_SYSTEM.md`
- `UNITY_BATTLE_SETUP.md`
