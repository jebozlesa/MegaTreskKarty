# Card Creation Guide

Last updated: 2026-03-09

## Scope

This guide is for creating/adding cards used by the client and keeping data consistent with multiplayer flow.

## Current Data Model

- Card identity is `cardId` (runtime UUID) and `StyleID`/template source identity.
- Multiplayer battle logic is server-authoritative.
- Client must treat received card payload as source for rendering and local interactions.

## Asset Preparation

1. Add art to `Assets/Resources/Cards/`.
2. Use consistent naming (lowercase, no spaces).
3. Keep image and card metadata names aligned.

## Data Preparation

If local SQLite content is still used in your build pipeline:

1. Update card template rows.
2. Update visuals rows.
3. Update character-attack mapping rows.
4. Rebuild or refresh runtime DB copy used by client.

If your current build fully uses server-generated cards, treat local DB edits as fallback/legacy tooling only.

## Multiplayer Safety Rules

- Never assume local stats are final in multiplayer.
- Always consume server-provided card values for battle state.
- Keep `cardId` stable through one card lifecycle.

## Implementation Checklist

- Card image added and loadable.
- Template/visual data exists.
- Attack mapping contains valid attack IDs.
- Card can be selected, revealed, and replaced correctly.
- Card death flow works for this card without special-case code.

## Test Checklist

- Card appears in selection UI.
- Card stats render correctly.
- Card can enter battle and receive effects.
- Card death removes it from board and selectedCards.
- Replacement selection works after death.

## Related Docs

- `CARD_DEATH_SYSTEM.md`
- `CARD_REPLACEMENT_SYSTEM.md`
- `EFFECT_SYSTEM_ARCHITECTURE.md`
- `ATTACK_SELECTION_SYSTEM.md`
