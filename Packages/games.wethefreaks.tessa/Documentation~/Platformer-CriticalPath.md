# Critical Path Platform Algorithm

## Purpose
Creates a "main traversal" line of platforms from entry to exit, then optionally adds extra platforms for exploration.

## Core Idea
- Start at entry and march toward exit.
- Each step places a platform and advances X by a configurable step range.
- Y changes within a maximum jump delta.
- Optional extra platforms are sprinkled randomly.

## Parameters
- `MinPlatformLength` / `MaxPlatformLength`: platform length in tiles.
- `MinStepX` / `MaxStepX`: horizontal advance between platforms.
- `MaxStepY`: vertical delta between consecutive platforms.
- `ExtraPlatforms`: additional random platforms outside the main path.

## When To Use
- You want a clear critical route with optional side jumps.
- You want to guarantee room traversal is possible.

## Suggested Defaults
- `MinStepX = 2`, `MaxStepX = 6`
- `MaxStepY = 2`
- `MinPlatformLength = 4`, `MaxPlatformLength = 10`
- `ExtraPlatforms = 1`

## Notes
- Provide entry/exit points in `PlatformPlacementContext` for best results.
- Reduce `MinStepX` if rooms are narrow.
