# Kill Counter Setup

## Scene Requirements
- One manager component in multiplayer scene.
- Separate UI indicators for player side and enemy side.
- References wired in inspector before runtime.

## Wiring Checklist
- Manager has references to all indicator components.
- Manager has access to battle/death events source.
- Win threshold is configured (default: first to 3).

## Runtime Validation
- On scene start, all indicators show alive/default state.
- After confirmed kill, one indicator flips for correct side.
- At threshold, win/lose flow runs once.
