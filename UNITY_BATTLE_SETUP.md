# Unity Battle Setup (Current)

## Required Components

- Main multiplayer fight controller.
- Networking manager for cloud calls.
- Battle result processor/timeline renderer.
- Attack selection UI manager.
- Kill counter manager + indicator UI.

## Required References

- Controllers cross-reference each other in inspector.
- UI buttons point to attack selection handlers.
- HP bars and effect icon roots are assigned.

## Startup Expectations

- Loading overlay is present in the battle scene and starts inactive.
- `FightSystemMultiplayer.multiplayerUI` is assigned explicitly in the Inspector.
- Startup combat UI stays hidden until multiplayer bootstrap completes.
- Room join/init finishes.
- Decks and selected cards are loaded.
- Attack counts are fetched after card selection.
- Attack UI remains disabled until prerequisites are complete.

## Pre-Play Smoke Test

- Loading overlay appears during startup and disappears before player interaction begins.
- Select card -> counts appear.
- Submit attack -> timeline plays.
- End turn -> ready checks complete.
