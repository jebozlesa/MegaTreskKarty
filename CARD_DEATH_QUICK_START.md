# Card Death Quick Start

Last updated: 2026-03-09

## TL;DR

When a card reaches `health <= 0`:

1. Remove it from board UI.
2. Clear it from server selectedCards.
3. Enter replacement flow if player has cards in hand.
4. Continue battle only after valid selected cards are synchronized.

## Fast Validation

- Dead card GameObject disappears.
- Active card reference is cleared.
- Server selectedCards no longer contains dead `cardId`.
- Player can select replacement card (if available).
- No next attack is sent with dead card.

## Minimal Test Scenarios

1. My card dies, I still have cards -> replacement selection opens.
2. My card dies, no cards left -> terminal lose state.
3. Enemy card dies -> enemy cleared, win/continue according to flow.
4. Both cards die -> both cleared, replacement/terminal logic stays consistent.

## Where to Look

- `CARD_DEATH_SYSTEM.md` (full behavior)
- `CARD_REPLACEMENT_SYSTEM.md` (replacement flow)
- `NETWORK_RETRY_SYSTEM.md` (server retry behavior)
- `DOCUMENTATION_INDEX.md` (active docs map)
