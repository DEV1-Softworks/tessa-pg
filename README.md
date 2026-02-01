# Tessa Procedural Level Generator for Unity

[![License: Beerware](https://img.shields.io/badge/License-Beerware-brown.svg)](https://en.wikipedia.org/wiki/Beerware)

Tessa is a Unity library that builds small, playable 2D levels for you. Think of it as a “level layout and painting” tool: it creates rooms, connects them with corridors, and draws floors, walls, and platforms onto Tilemaps. You can use it to quickly prototype metroidvania-style spaces without hand‑placing every tile.

### Highlights

- **Automatic room layouts:** Creates a main path with side rooms so levels feel connected and varied.
- **Built‑in platforms:** Adds different platform styles for jump‑based gameplay.
- **Easy to tweak:** Change room counts, sizes, and platform style in the Inspector.
- **Works with Tilemaps:** Paints directly onto your existing Tilemap layers.
- **Fast iteration:** Generate a new layout with one click or on play.

## Platformer Algorithms (Tilemap Painter)

`TessaMetroidvaniaTilemapPainter` supports multiple platform placement algorithms. You can choose one in the Inspector and the runtime generator will use it when painting rooms.

### Inspector Parameters

Component: `TessaMetroidvaniaTilemapPainter`

- **Platform Algorithm**: choose one of
  - `Tiered`
  - `PoissonRow`
  - `CriticalPath`
  - `Noise`
  - `PatternLibrary`
- **Algorithm: Tiered**
  - `Tiered Min Platforms`, `Tiered Max Platforms`
  - `Tiered Min Length`, `Tiered Max Length`
  - `Tiered Tier Count`
  - `Tiered Min Vertical Spacing`
- **Algorithm: Poisson Row**
  - `Poisson Min Length`, `Poisson Max Length`
  - `Poisson Min Row Spacing`
  - `Poisson Max Platforms`
  - `Poisson Max Attempts`
- **Algorithm: Critical Path**
  - `Critical Min Platform Length`, `Critical Max Platform Length`
  - `Critical Min Step X`, `Critical Max Step X`
  - `Critical Max Step Y`
  - `Critical Extra Platforms`
- **Algorithm: Noise**
  - `Noise Scale`, `Noise Threshold`
  - `Noise Min Length`, `Noise Max Length`
  - `Noise Max Platforms`, `Noise Seed`
- **Algorithm: Pattern Library**
  - `Pattern Max Patterns Per Room`

## Step-by-Step Scene Setup (Unity)

This is the minimal setup to get a level generated and painted in a scene.

### 1) Create the base Tilemap hierarchy

1) Create a `Grid` GameObject.
2) Under it, create three Tilemaps:
   - `Floor` (Tilemap + TilemapRenderer)
   - `Walls` (Tilemap + TilemapRenderer)
   - `Platforms` (Tilemap + TilemapRenderer)

### 2) Add the painter

1) Create an empty GameObject named `TessaPainter`.
2) Add `TessaMetroidvaniaTilemapPainter`.
3) Assign the Tilemaps:
   - `Floor Tilemap` -> `Floor`
   - `Wall Tilemap` -> `Walls`
   - `Platform Tilemap` -> `Platforms` (optional; if left empty, walls are used for platforms)
4) Assign required tiles:
   - `Floor Tile` (required)
   - Wall tiles (`Wall Top/Bottom/Left/Right` and corners) as needed
   - `Platform Tile` if you want platforms
5) (Optional) Set sorting orders so floors render behind walls/platforms.

### 3) Add the generator

1) Create an empty GameObject named `TessaGenerator`.
2) Add `TessaGenerator`.
3) Assign `Tilemap Painter` -> `TessaPainter` (from step 2).
4) (Optional) Set:
   - `Main Path Room Count`
   - `Optional Branch Count`
   - `Unlocking Ability Id`
   - `Regenerate On Play`

### 4) Align the start room (optional)

If you want the start room aligned to the camera at play time:

1) Enable `Align Start Room To Camera`.
2) Assign `Start Camera` or leave it empty to use `Camera.main`.

### 5) Generate and verify

- Play the scene (if `Regenerate On Play` is on), or
- Right-click the `TessaGenerator` component and choose `Generate Level`.

If nothing paints, double-check that `Floor Tilemap`, `Wall Tilemap`, and `Floor Tile` are assigned.

## Implementation Notes

### Overview (Generation -> Painting)

1) `TessaGenerator.GenerateLevel` builds a `TessaLevelLayout` (rooms + connections).
2) The layout is handed to `TessaMetroidvaniaTilemapPainter.PaintLevel`.
3) The painter places room rectangles, carves corridors, and adds platform segments.

### Layout Generation (TessaGenerator)

- Main path rooms are laid out in a straight line along X (y = 0), length `mainPathRoomCount` (min 8).
- Room types on the main path:
  - index 0: `Start`
  - last index: `Boss`
  - one random index between ~2 and ~`mainPathRoomCount - 2`: `Ability`
  - everything else: `Normal`
- Optional branches are placed up or down from random main path rooms (y = +/-1), up to `optionalBranchCount`.
- One optional branch edge is locked (when any branches exist). The lock requires `unlockingAbilityId`.
- `EnsureSingleBossRoom` enforces a single boss room at the end of the main path.
- If `alignStartRoomToCamera` is on, the painter is aligned to the camera before painting.

### Painting Flow (TessaMetroidvaniaTilemapPainter)

- Each room is assigned a cell in a logical grid based on `maxRoomSizeTiles` and `cellPaddingTiles`.
- If `useFixedRoomSize` is on, every room uses `maxRoomSizeTiles`; otherwise each room size is picked in `stepTiles` increments between `minRoomSizeTiles` and `maxRoomSizeTiles`.
- Room rectangles are painted as:
  - inner fill on the floor tilemap
  - borders on the wall tilemap (with optional corner tiles)
- Corridors are carved between connected rooms by clearing wall tiles and filling floor tiles.
- Platforms are painted for every room except the boss room, using the selected algorithm.
- Sorting orders are applied in `Awake` and clamped in `OnValidate`.

### Platform Placement Algorithms

The painter creates one algorithm instance per room using the Inspector parameters:

- `Tiered`: evenly spaced tiers with a bounded platform count/length.
- `PoissonRow`: rows with Poisson-like spacing and attempt limits.
- `CriticalPath`: a primary traversal path with extra platforms.
- `Noise`: Perlin-like noise sampling for placement density.
- `PatternLibrary`: fixed patterns sampled from the default pattern set.

If `forceAtLeastOne` is true and an algorithm returns zero segments, a fallback centered platform is added.

### Core Data Types

- `TessaLevelLayout`: holds `Rooms` and `Connections`.
- `TessaRoomData`: coordinates + `RoomType` (Start, Normal, Ability, Boss, Optional).
- `TessaConnection`: `From`, `To`, `Locked`, and `RequiredAbility`.

### Extending

- Add a new algorithm by implementing `IPlatformPlacementAlgorithm` and wiring it in `CreatePlatformAlgorithm`.
- If you want custom room placement logic, update `TessaGenerator.BuildLayout`.
- To align the layout to a specific world position (spawn point, camera), call `AlignStartRoomToWorldPosition`.

Made in 🇲🇽 with ❤️ by We, The Freaks.
