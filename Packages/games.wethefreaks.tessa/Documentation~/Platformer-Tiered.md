# Tiered Platform Algorithm

## Purpose
Places platforms on horizontal "tiers" so rooms feel structured and readable. Good for classic platformers where players can scan distinct height bands.

## Core Idea
- Split the room into vertical bands (tiers).
- Place platforms inside each tier with a minimum vertical spacing.
- Randomize length and horizontal offset per platform.

## Parameters
- `MinPlatforms` / `MaxPlatforms`: total platforms to place.
- `MinLength` / `MaxLength`: platform length in tiles.
- `TierCount`: number of height bands.
- `MinVerticalSpacing`: minimum vertical distance between platforms.

## When To Use
- You want consistent vertical rhythm.
- You want predictable jumps and traversal.

## Suggested Defaults
- `TierCount = 3`
- `MinPlatforms = 2`, `MaxPlatforms = 4`
- `MinLength = 4`, `MaxLength = 10`
- `MinVerticalSpacing = 2`

## Notes
- If tiers are too narrow, reduce `TierCount` or increase room height.
- Works well with corridors aligned at mid-tier height.
