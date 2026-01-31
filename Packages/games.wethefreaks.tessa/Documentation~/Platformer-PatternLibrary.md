# Pattern Library Platform Algorithm

## Purpose
Places curated platform patterns (stairs, zig-zag, tiers) to create handcrafted feel with procedural placement.

## Core Idea
- Maintain a list of reusable patterns.
- Select a random pattern and place it within the room bounds.
- Optionally place multiple patterns per room.

## Parameters
- `patterns`: list of `PlatformPattern` objects.
- `MaxPatternsPerRoom`: number of patterns to place.

## When To Use
- You want designer-driven layouts with procedural variety.
- You want recognizable micro-structures (stairs, islands, zig-zags).

## Suggested Defaults
- `MaxPatternsPerRoom = 1` or `2`

## Notes
- Patterns should be sized to fit your smallest room.
- Combine with another algorithm for additional filler platforms.
