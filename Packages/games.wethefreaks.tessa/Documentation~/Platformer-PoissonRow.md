# Poisson Row Platform Algorithm

## Purpose
Distributes platforms with a minimum vertical distance, preventing clusters while keeping randomness.

## Core Idea
- Pick random rows (Y) for platforms.
- Reject any row too close to a previously used row.
- Generate random-length platforms on accepted rows.

## Parameters
- `MinLength` / `MaxLength`: platform length in tiles.
- `MinRowSpacing`: minimum vertical spacing between rows.
- `MaxPlatforms`: maximum number of platforms.
- `MaxPlacementAttempts`: how many tries before stopping.

## When To Use
- You want organic but evenly spaced heights.
- You want to avoid stacked platforms that are too tight.

## Suggested Defaults
- `MinRowSpacing = 2`
- `MaxPlatforms = 4`
- `MaxPlacementAttempts = 24`
- `MinLength = 4`, `MaxLength = 10`

## Notes
- Increase `MaxPlacementAttempts` for large rooms.
- If you get too few platforms, reduce `MinRowSpacing`.
