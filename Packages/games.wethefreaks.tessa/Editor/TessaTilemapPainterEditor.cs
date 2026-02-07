using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TessaMetroidvaniaTilemapPainter))]
[CanEditMultipleObjects]
public class TessaTilemapPainterEditor : Editor
{
    private SerializedProperty floorTilemap;
    private SerializedProperty wallTilemap;
    private SerializedProperty platformTilemap;

    private SerializedProperty floorTile;
    private SerializedProperty wallTopTile;
    private SerializedProperty wallBottomTile;
    private SerializedProperty wallLeftTile;
    private SerializedProperty wallRightTile;
    private SerializedProperty wallCornerTopLeftTile;
    private SerializedProperty wallCornerTopRightTile;
    private SerializedProperty wallCornerBottomLeftTile;
    private SerializedProperty wallCornerBottomRightTile;
    private SerializedProperty gateTile;
    private SerializedProperty platformTile;

    private SerializedProperty minRoomSizeTiles;
    private SerializedProperty maxRoomSizeTiles;
    private SerializedProperty useFixedRoomSize;
    private SerializedProperty stepTiles;
    private SerializedProperty cellPaddingTiles;

    private SerializedProperty layoutOriginTiles;

    private SerializedProperty corridorThicknessTiles;

    private SerializedProperty floorSortingOrder;
    private SerializedProperty platformSortingOrder;
    private SerializedProperty wallSortingOrder;

    private SerializedProperty platformChance;
    private SerializedProperty platformLengthTiles;
    private SerializedProperty platformHorizontalPadding;
    private SerializedProperty platformVerticalPadding;

    private SerializedProperty platformAlgorithm;

    private SerializedProperty tieredMinPlatforms;
    private SerializedProperty tieredMaxPlatforms;
    private SerializedProperty tieredMinLength;
    private SerializedProperty tieredMaxLength;
    private SerializedProperty tieredTierCount;
    private SerializedProperty tieredMinVerticalSpacing;

    private SerializedProperty poissonMinLength;
    private SerializedProperty poissonMaxLength;
    private SerializedProperty poissonMinRowSpacing;
    private SerializedProperty poissonMaxPlatforms;
    private SerializedProperty poissonMaxAttempts;

    private SerializedProperty criticalMinPlatformLength;
    private SerializedProperty criticalMaxPlatformLength;
    private SerializedProperty criticalMinStepX;
    private SerializedProperty criticalMaxStepX;
    private SerializedProperty criticalMaxStepY;
    private SerializedProperty criticalExtraPlatforms;

    private SerializedProperty noiseScale;
    private SerializedProperty noiseThreshold;
    private SerializedProperty noiseMinLength;
    private SerializedProperty noiseMaxLength;
    private SerializedProperty noiseMaxPlatforms;
    private SerializedProperty noiseSeed;

    private SerializedProperty patternMaxPatternsPerRoom;

    private void OnEnable()
    {
        floorTilemap = serializedObject.FindProperty("floorTilemap");
        wallTilemap = serializedObject.FindProperty("wallTilemap");
        platformTilemap = serializedObject.FindProperty("platformTilemap");

        floorTile = serializedObject.FindProperty("floorTile");
        wallTopTile = serializedObject.FindProperty("wallTopTile");
        wallBottomTile = serializedObject.FindProperty("wallBottomTile");
        wallLeftTile = serializedObject.FindProperty("wallLeftTile");
        wallRightTile = serializedObject.FindProperty("wallRightTile");
        wallCornerTopLeftTile = serializedObject.FindProperty("wallCornerTopLeftTile");
        wallCornerTopRightTile = serializedObject.FindProperty("wallCornerTopRightTile");
        wallCornerBottomLeftTile = serializedObject.FindProperty("wallCornerBottomLeftTile");
        wallCornerBottomRightTile = serializedObject.FindProperty("wallCornerBottomRightTile");
        gateTile = serializedObject.FindProperty("gateTile");
        platformTile = serializedObject.FindProperty("platformTile");

        minRoomSizeTiles = serializedObject.FindProperty("minRoomSizeTiles");
        maxRoomSizeTiles = serializedObject.FindProperty("maxRoomSizeTiles");
        useFixedRoomSize = serializedObject.FindProperty("useFixedRoomSize");
        stepTiles = serializedObject.FindProperty("stepTiles");
        cellPaddingTiles = serializedObject.FindProperty("cellPaddingTiles");

        layoutOriginTiles = serializedObject.FindProperty("layoutOriginTiles");

        corridorThicknessTiles = serializedObject.FindProperty("corridorThicknessTiles");

        floorSortingOrder = serializedObject.FindProperty("floorSortingOrder");
        platformSortingOrder = serializedObject.FindProperty("platformSortingOrder");
        wallSortingOrder = serializedObject.FindProperty("wallSortingOrder");

        platformChance = serializedObject.FindProperty("platformChance");
        platformLengthTiles = serializedObject.FindProperty("platformLengthTiles");
        platformHorizontalPadding = serializedObject.FindProperty("platformHorizontalPadding");
        platformVerticalPadding = serializedObject.FindProperty("platformVerticalPadding");

        platformAlgorithm = serializedObject.FindProperty("platformAlgorithm");

        tieredMinPlatforms = serializedObject.FindProperty("tieredMinPlatforms");
        tieredMaxPlatforms = serializedObject.FindProperty("tieredMaxPlatforms");
        tieredMinLength = serializedObject.FindProperty("tieredMinLength");
        tieredMaxLength = serializedObject.FindProperty("tieredMaxLength");
        tieredTierCount = serializedObject.FindProperty("tieredTierCount");
        tieredMinVerticalSpacing = serializedObject.FindProperty("tieredMinVerticalSpacing");

        poissonMinLength = serializedObject.FindProperty("poissonMinLength");
        poissonMaxLength = serializedObject.FindProperty("poissonMaxLength");
        poissonMinRowSpacing = serializedObject.FindProperty("poissonMinRowSpacing");
        poissonMaxPlatforms = serializedObject.FindProperty("poissonMaxPlatforms");
        poissonMaxAttempts = serializedObject.FindProperty("poissonMaxAttempts");

        criticalMinPlatformLength = serializedObject.FindProperty("criticalMinPlatformLength");
        criticalMaxPlatformLength = serializedObject.FindProperty("criticalMaxPlatformLength");
        criticalMinStepX = serializedObject.FindProperty("criticalMinStepX");
        criticalMaxStepX = serializedObject.FindProperty("criticalMaxStepX");
        criticalMaxStepY = serializedObject.FindProperty("criticalMaxStepY");
        criticalExtraPlatforms = serializedObject.FindProperty("criticalExtraPlatforms");

        noiseScale = serializedObject.FindProperty("noiseScale");
        noiseThreshold = serializedObject.FindProperty("noiseThreshold");
        noiseMinLength = serializedObject.FindProperty("noiseMinLength");
        noiseMaxLength = serializedObject.FindProperty("noiseMaxLength");
        noiseMaxPlatforms = serializedObject.FindProperty("noiseMaxPlatforms");
        noiseSeed = serializedObject.FindProperty("noiseSeed");

        patternMaxPatternsPerRoom = serializedObject.FindProperty("patternMaxPatternsPerRoom");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSectionHeader("Tilemaps");
        EditorGUILayout.PropertyField(floorTilemap);
        EditorGUILayout.PropertyField(wallTilemap);
        EditorGUILayout.PropertyField(platformTilemap);

        DrawSectionHeader("Tiles");
        EditorGUILayout.PropertyField(floorTile);
        EditorGUILayout.PropertyField(wallTopTile);
        EditorGUILayout.PropertyField(wallBottomTile);
        EditorGUILayout.PropertyField(wallLeftTile);
        EditorGUILayout.PropertyField(wallRightTile);
        EditorGUILayout.PropertyField(wallCornerTopLeftTile);
        EditorGUILayout.PropertyField(wallCornerTopRightTile);
        EditorGUILayout.PropertyField(wallCornerBottomLeftTile);
        EditorGUILayout.PropertyField(wallCornerBottomRightTile);
        EditorGUILayout.PropertyField(gateTile);
        EditorGUILayout.PropertyField(platformTile);

        DrawSectionHeader("Room sizing (tiles)");
        EditorGUILayout.PropertyField(minRoomSizeTiles);
        EditorGUILayout.PropertyField(maxRoomSizeTiles);
        EditorGUILayout.PropertyField(useFixedRoomSize);
        EditorGUILayout.PropertyField(stepTiles);
        EditorGUILayout.PropertyField(cellPaddingTiles);

        DrawSectionHeader("Layout Offset (tiles)");
        EditorGUILayout.PropertyField(layoutOriginTiles);

        DrawSectionHeader("Corridors");
        EditorGUILayout.PropertyField(corridorThicknessTiles);

        DrawSectionHeader("Rendering (sorting)");
        EditorGUILayout.PropertyField(floorSortingOrder);
        EditorGUILayout.PropertyField(platformSortingOrder);
        EditorGUILayout.PropertyField(wallSortingOrder);

        DrawSectionHeader("Platforms (inner)");
        EditorGUILayout.PropertyField(platformChance);
        EditorGUILayout.PropertyField(platformLengthTiles);
        EditorGUILayout.PropertyField(platformHorizontalPadding);
        EditorGUILayout.PropertyField(platformVerticalPadding);

        DrawSectionHeader("Platform Algorithm");
        EditorGUILayout.PropertyField(platformAlgorithm);

        DrawAlgorithmSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAlgorithmSettings()
    {
        if (platformAlgorithm.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("Multiple algorithms selected. Choose a single algorithm to edit settings.", MessageType.Info);
            return;
        }

        var algorithm = (TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType)platformAlgorithm.enumValueIndex;
        switch (algorithm)
        {
            case TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.Tiered:
                DrawSectionHeader("Algorithm: Tiered");
                EditorGUILayout.PropertyField(tieredMinPlatforms);
                EditorGUILayout.PropertyField(tieredMaxPlatforms);
                EditorGUILayout.PropertyField(tieredMinLength);
                EditorGUILayout.PropertyField(tieredMaxLength);
                EditorGUILayout.PropertyField(tieredTierCount);
                EditorGUILayout.PropertyField(tieredMinVerticalSpacing);
                break;
            case TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.PoissonRow:
                DrawSectionHeader("Algorithm: Poisson Row");
                EditorGUILayout.PropertyField(poissonMinLength);
                EditorGUILayout.PropertyField(poissonMaxLength);
                EditorGUILayout.PropertyField(poissonMinRowSpacing);
                EditorGUILayout.PropertyField(poissonMaxPlatforms);
                EditorGUILayout.PropertyField(poissonMaxAttempts);
                break;
            case TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.CriticalPath:
                DrawSectionHeader("Algorithm: Critical Path");
                EditorGUILayout.PropertyField(criticalMinPlatformLength);
                EditorGUILayout.PropertyField(criticalMaxPlatformLength);
                EditorGUILayout.PropertyField(criticalMinStepX);
                EditorGUILayout.PropertyField(criticalMaxStepX);
                EditorGUILayout.PropertyField(criticalMaxStepY);
                EditorGUILayout.PropertyField(criticalExtraPlatforms);
                break;
            case TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.Noise:
                DrawSectionHeader("Algorithm: Noise");
                EditorGUILayout.PropertyField(noiseScale);
                EditorGUILayout.PropertyField(noiseThreshold);
                EditorGUILayout.PropertyField(noiseMinLength);
                EditorGUILayout.PropertyField(noiseMaxLength);
                EditorGUILayout.PropertyField(noiseMaxPlatforms);
                EditorGUILayout.PropertyField(noiseSeed);
                break;
            case TessaMetroidvaniaTilemapPainter.PlatformAlgorithmType.PatternLibrary:
                DrawSectionHeader("Algorithm: Pattern Library");
                EditorGUILayout.PropertyField(patternMaxPatternsPerRoom);
                break;
        }
    }

    private static void DrawSectionHeader(string title)
    {
        GUILayout.Space(6f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }
}
