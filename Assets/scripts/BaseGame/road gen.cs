using UnityEngine;

public class RoadPlacer : MonoBehaviour
{
    [Header("Road Settings")]
    public Sprite roadSprite;
    public Color roadColor = new Color(1, 1, 1, 1);
    public float roadColorTolerance = 0.5f; // Increased tolerance for better detection
    
    [Header("Path Settings")]
    public int placementBuffer = 1; // Changed to 1: a 1-unit buffer is usually enough
    public float tileSize = 1f; // Changed to 1f: Tile size is now 1 world unit
    
    [Header("References")]
    public GameObject towerPrefab;

    // A constant derived from your Sprite's PPU (100)
    // This defines how many pixels make up one logical world unit/tile.
    private const int PIXELS_PER_UNIT = 100; 

    [ContextMenu("Generate Road and Placements")]
    public void GenerateRoadAndPlacement()
    {
        if (roadSprite == null)
        {
            Debug.LogError("Road sprite not assigned!");
            return;
        }

        // Clean up existing objects
        CleanupExistingObjects();
        
        // Detect road from sprite
        Texture2D texture = roadSprite.texture;
        Vector2Int mapDimensions = new Vector2Int(texture.width, texture.height);
        
        // --- Input Validation ---
        if (texture.width % PIXELS_PER_UNIT != 0 || texture.height % PIXELS_PER_UNIT != 0)
        {
            Debug.LogError("Map texture dimensions must be a multiple of the PIXELS_PER_UNIT (100)!");
            return;
        }

        // Calculate the tile-based dimensions
        int tileWidth = texture.width / PIXELS_PER_UNIT;
        int tileHeight = texture.height / PIXELS_PER_UNIT;

        // Create parent objects for organization
        GameObject roadParent = new GameObject("RoadPath");
        roadParent.transform.parent = transform;
        
        GameObject placementParent = new GameObject("PlacementSpots");
        placementParent.transform.parent = transform;
        
        // Display the road sprite (This respects the original PPU=100 setting)
        GameObject roadDisplay = new GameObject("RoadDisplay");
        roadDisplay.transform.parent = roadParent.transform;
        // Position should be (0,0,0) if RoadManager is at (0,0,0) and sprite pivot is Center
        roadDisplay.transform.position = Vector3.zero; 
        
        SpriteRenderer roadRenderer = roadDisplay.AddComponent<SpriteRenderer>();
        roadRenderer.sprite = roadSprite;
        roadRenderer.sortingLayerName = "Ground";
        roadRenderer.sortingOrder = 0;
        
        // --- 1. PARSE ROAD PIXELS INTO A TILE GRID ---
        Color[] pixels = texture.GetPixels();
        bool[,] roadGrid = new bool[tileWidth, tileHeight];
        
        for (int ty = 0; ty < tileHeight; ty++) // ty is the Tile Y index
        {
            for (int tx = 0; tx < tileWidth; tx++) // tx is the Tile X index
            {
                bool isRoadTile = false;
                
                // Check if *any* pixel within this 100x100 block is a road pixel
                for(int py = 0; py < PIXELS_PER_UNIT; py++)
                {
                    for(int px = 0; px < PIXELS_PER_UNIT; px++)
                    {
                        int pixelX = tx * PIXELS_PER_UNIT + px;
                        int pixelY = ty * PIXELS_PER_UNIT + py;
                        
                        int pixelIndex = pixelY * texture.width + pixelX;
                        Color pixelColor = pixels[pixelIndex];
                        
                        if (ColorMatch(pixelColor, roadColor, roadColorTolerance))
                        {
                            isRoadTile = true;
                            break; // Road pixel found, move to next tile
                        }
                    }
                    if (isRoadTile) break;
                }

                if (isRoadTile)
                {
                    // Flip the Y coordinate to match Unity's common coordinate system usage 
                    roadGrid[tx, tileHeight - 1 - ty] = true; 
                }
            }
        }
        
        // --- 2. CREATE TILE-BASED PLACEMENT ZONES ---
        for (int tx = 0; tx < tileWidth; tx++)
        {
            for (int ty = 0; ty < tileHeight; ty++)
            {
                // We use the FLIPPED Y-index (fy) for checking roadGrid, 
                // because roadGrid was filled in a flipped manner.
                int fy = tileHeight - 1 - ty;
                bool isRoad = roadGrid[tx, fy]; // <-- Check roadGrid using the flipped index
                
                bool isPlacementZone = false;
                
                // Only consider placing a spot if the current tile is NOT a road tile
                if (!isRoad)
                {
                    // Check if this tile should be a placement zone (adjacent to road within buffer)
                    for (int dx = -placementBuffer; dx <= placementBuffer; dx++)
                    {
                        for (int dy = -placementBuffer; dy <= placementBuffer; dy++)
                        {
                            int checkX = tx + dx;
                            int checkY = fy + dy; // <-- Check roadGrid using the flipped index

                            // Bounds check (uses original tile dimensions, which is fine)
                            if (checkX >= 0 && checkX < tileWidth && checkY >= 0 && checkY < tileHeight)
                            {
                                // If an adjacent tile (within buffer) is a road tile
                                if (roadGrid[checkX, checkY])
                                {
                                    isPlacementZone = true;
                                    goto FoundPlacementZone; // Optimized jump out of nested loops
                                }
                            }
                        }
                    }
                }

                FoundPlacementZone:
                
                if (isPlacementZone)
                {
                    // The Y position must be based on the FLIPPED Y index (fy) to align with the detected road shape
                    Vector3 worldPos = new Vector3(
                        (tx - tileWidth / 2f + 0.5f) * tileSize, // Center X
                        (fy - tileHeight / 2f + 0.5f) * tileSize, // Center Y is now based on fy
                        0
                    );
                    
                    // Create placement spot
                    GameObject spotObj = new GameObject($"PlacementSpot_{tx}_{ty}");
                    spotObj.transform.parent = placementParent.transform;
                    spotObj.transform.position = worldPos;
                    
                    PlacementSpot spot = spotObj.AddComponent<PlacementSpot>();
                    spot.spotSize = tileSize * 0.25f; 
                }
            }
        }
        
        // Place tower at center-top of map (using tile dimensions for centering)
        if (towerPrefab != null)
        {
            Vector3 towerPos = new Vector3(0, (tileHeight / 2f) * tileSize, 0);
            Instantiate(towerPrefab, towerPos, Quaternion.identity, transform);
        }
        
        Debug.Log($"Road and {placementParent.transform.childCount} placement zones generated successfully!");
    }
    
    // ColorMatch function remains the same
    bool ColorMatch(Color a, Color b, float tolerance)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               a.a > 0.5f;
    }
    
    // PixelToWorld is now UNUSED in the main loop, but kept for cleanup/reference
    // The new world position calculation is more direct and based on tile indices
    Vector3 PixelToWorld(Vector2Int pixel, Vector2Int mapDimensions)
    {
        return new Vector3(
            (pixel.x - mapDimensions.x / 2f) * (tileSize / PIXELS_PER_UNIT),
            (pixel.y - mapDimensions.y / 2f) * (tileSize / PIXELS_PER_UNIT),
            0
        );
    }
    
    void CleanupExistingObjects()
    {
        Transform roadParent = transform.Find("RoadPath");
        if (roadParent != null) DestroyImmediate(roadParent.gameObject); // Using DestroyImmediate for editor code
        
        Transform placementParent = transform.Find("PlacementSpots");
        if (placementParent != null) DestroyImmediate(placementParent.gameObject); // Using DestroyImmediate for editor code
    }
}