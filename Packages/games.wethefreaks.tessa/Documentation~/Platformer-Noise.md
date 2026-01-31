# Noise Platform Algorithm

## Purpose
Uses Perlin noise to decide which rows receive platforms, producing natural variation without strict tiers.

## Core Idea
- Sample Perlin noise by row.
- If noise value exceeds a threshold, place a platform on that row.
- Use noise value to scale platform length.

## Parameters
- `NoiseScale`: lower values = smoother, larger values = noisier.
- `Threshold`: minimum noise required to place a platform.
- `MinLength` / `MaxLength`: platform length in tiles.
- `MaxPlatforms`: upper limit on total platforms.
- `Seed`: noise seed.

## When To Use
- You want organic density variation.
- You want some rooms to feel sparse and others dense.

## Suggested Defaults
- `NoiseScale = 0.15`
- `Threshold = 0.5`
- `MinLength = 4`, `MaxLength = 10`
- `MaxPlatforms = 4`

## Notes
- Lower `Threshold` increases platform count.
- Use a stable seed for deterministic generation.
