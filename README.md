# Tessa

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

### Example Usage (Runtime)

```csharp
using UnityEngine;

public class PlatformAlgorithmSwitcher : MonoBehaviour
{
    [SerializeField] private TessaMetroidvaniaTilemapPainter painter;

    public void UseCriticalPath()
    {
        if (painter == null) return;
        painter.SetPlatformAlgorithm(TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.CriticalPath);
    }
}
```
