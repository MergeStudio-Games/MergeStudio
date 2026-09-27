# First playable: bakery merge

Working choice: retain the existing bread-themed merge game, portrait Android,
offline single-player, Turkish and English. No ads, paid purchases or account
requirement in this first playable. These choices avoid blocking the gameplay
prototype on provider credentials; commercial service integration is a later gate.

## First session

Generate bread pieces, combine equal tiers, fulfill the first order, observe the
reward, exit and return with progress intact. The first session should teach the
loop through the board itself and fit in approximately one minute.

## Acceptance

- Board adapts to its saved dimensions and screen width.
- Selected cells and valid actions are readable without relying only on color.
- Tap or drag moves an item to an empty cell or merges matching items.
- Invalid input preserves progress; reaching tier 10 never overflows the tier cap.
- The first completed order rewards once, and completed state survives relaunch.
- Instruction/HUD text is available in TR/EN.
- Visual/audio feedback has a mute option; no mandatory network connection.
- Run EditMode/PlayMode tests and capture screenshots on representative device ratios.

This is a scope/acceptance brief, not evidence that the playable has passed QA.
