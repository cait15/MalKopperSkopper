using UnityEngine;
using System.Collections.Generic;

public class PathGenerator : MonoBehaviour
{
    public static PathGenerator Instance;
    
    [Header("Visual Settings")]
    public Color pathColor = Color.white;
    public Color groundColor = Color.gray;
    public Color towerColor = new Color(0.2f, 0.6f, 1f, 1f);
    
    [Header("Map Settings")]
    public int mapWidth = 20;
    public int mapHeight = 20;
    public float tileSize = 1f;
    
    [Header("Path Settings")]
    public int numberOfBranches = 2;
    public int branchLength = 7;
    public int branchSpacing = 3;
    
    [Header("Placement Settings")]
    public GameObject placementSpotPrefab;
    public int placementSpotsToGenerate = 12;
    public float placementDistanceFromPath = 1.5f;
    
    [Header("References")]
    public GameObject towerObject;
    
    private Vector2Int towerGridPos;
    private List<Vector2Int> mainPath = new List<Vector2Int>();
    private List<List<Vector2Int>> branchPaths = new List<List<Vector2Int>>();
    private List<Vector2Int> allPathTiles = new List<Vector2Int>();
    private List<Vector2Int> spawnPoints = new List<Vector2Int>();
    
    private GameObject tilesParent;
    private GameObject placementParent;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        GenerateMap();
    }
    
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        CleanUp();
        GeneratePath();
        CreateVisuals();
        PlaceTower();
        GeneratePlacementSpots();
    }
    
    void GeneratePath()
    {
        mainPath.Clear();
        branchPaths.Clear();
        allPathTiles.Clear();
        spawnPoints.Clear();
        
        // Tower at top middle
        towerGridPos = new Vector2Int(mapWidth / 2, mapHeight - 2);
        
        // Main path from bottom to tower
        GenerateMainPath();
        
        // Generate branches
        GenerateBranches();
        
        // Combine all paths
        allPathTiles.AddRange(mainPath);
        foreach (var branch in branchPaths)
        {
            foreach (var tile in branch)
            {
                if (!allPathTiles.Contains(tile))
                    allPathTiles.Add(tile);
            }
        }
    }
    
    void GenerateMainPath()
    {
        // Start from bottom middle
        Vector2Int currentPos = new Vector2Int(mapWidth / 2, 0);
        spawnPoints.Add(currentPos);
        mainPath.Add(currentPos);
        
        // Move up with slight randomization
        while (currentPos.y < towerGridPos.y)
        {
            int choice = Random.Range(0, 5);
            
            // Occasionally move left or right, but stay near center
            if (choice == 0 && currentPos.x > mapWidth / 2 - 3)
            {
                currentPos.x--;
            }
            else if (choice == 1 && currentPos.x < mapWidth / 2 + 3)
            {
                currentPos.x++;
            }
            else
            {
                currentPos.y++;
            }
            
            mainPath.Add(currentPos);
        }
    }
    
    void GenerateBranches()
    {
        if (numberOfBranches <= 0) return;
        
        // Find branch points along main path
        int segmentSize = mainPath.Count / (numberOfBranches + 1);
        
        for (int i = 0; i < numberOfBranches; i++)
        {
            int branchIndex = segmentSize * (i + 1);
            if (branchIndex >= mainPath.Count) break;
            
            Vector2Int branchStart = mainPath[branchIndex];
            bool goLeft = i % 2 == 0;
            
            List<Vector2Int> branch = GenerateBranch(branchStart, goLeft);
            branchPaths.Add(branch);
            
            // Add spawn point for this branch
            Vector2Int spawnPos = new Vector2Int(
                goLeft ? mapWidth / 4 : (mapWidth * 3) / 4,
                0
            );
            spawnPoints.Add(spawnPos);
        }
    }
    
    List<Vector2Int> GenerateBranch(Vector2Int start, bool goLeft)
    {
        List<Vector2Int> branch = new List<Vector2Int>();
        Vector2Int currentPos = start;
        branch.Add(currentPos);
        
        int direction = goLeft ? -1 : 1;
        
        // Move horizontally
        for (int i = 0; i < branchSpacing; i++)
        {
            currentPos.x += direction;
            branch.Add(currentPos);
        }
        
        // Move up
        for (int i = 0; i < branchLength; i++)
        {
            currentPos.y++;
            branch.Add(currentPos);
        }
        
        // Move back to main path
        while (currentPos.x != start.x)
        {
            currentPos.x -= direction;
            branch.Add(currentPos);
        }
        
        return branch;
    }
    
    void CreateVisuals()
    {
        tilesParent = new GameObject("Tiles");
        tilesParent.transform.parent = transform;
        
        // Create ground tiles (grey)
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorld(gridPos);
                
                bool isPath = allPathTiles.Contains(gridPos);
                CreateTile(worldPos, isPath ? pathColor : groundColor, tilesParent.transform);
            }
        }
    }
    
    void CreateTile(Vector3 position, Color color, Transform parent)
    {
        GameObject tile = new GameObject("Tile");
        tile.transform.parent = parent;
        tile.transform.position = position;
        
        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingLayerName = "Ground";
        
        tile.transform.localScale = Vector3.one * tileSize * 0.95f;
    }
    
    void PlaceTower()
    {
        Vector3 towerWorldPos = GridToWorld(towerGridPos);
        
        if (towerObject == null)
        {
            towerObject = new GameObject("Tower");
            
            SpriteRenderer sr = towerObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = towerColor;
            sr.sortingLayerName = "Units";
            sr.sortingOrder = 10;
            
            towerObject.transform.localScale = Vector3.one * tileSize * 1.5f;
        }
        
        towerObject.transform.position = towerWorldPos;
    }
    
    void GeneratePlacementSpots()
    {
        placementParent = new GameObject("PlacementSpots");
        placementParent.transform.parent = transform;
        
        int spotsGenerated = 0;
        int attempts = 0;
        
        while (spotsGenerated < placementSpotsToGenerate && attempts < 200)
        {
            attempts++;
            
            Vector2Int randomGrid = new Vector2Int(
                Random.Range(0, mapWidth),
                Random.Range(0, mapHeight)
            );
            
            if (IsValidPlacementPosition(randomGrid))
            {
                CreatePlacementSpot(GridToWorld(randomGrid));
                spotsGenerated++;
            }
        }
    }
    
    bool IsValidPlacementPosition(Vector2Int gridPos)
    {
        // Not on path
        if (allPathTiles.Contains(gridPos))
            return false;
        
        // Must be near path but not on it
        bool nearPath = false;
        foreach (var pathTile in allPathTiles)
        {
            float dist = Vector2Int.Distance(gridPos, pathTile);
            if (dist < 0.5f)
                return false;
            if (dist <= placementDistanceFromPath)
                nearPath = true;
        }
        
        if (!nearPath)
            return false;
        
        // Not too close to tower
        if (Vector2Int.Distance(gridPos, towerGridPos) < 3)
            return false;
        
        return true;
    }
    
    void CreatePlacementSpot(Vector3 position)
    {
        GameObject spot;
        
        if (placementSpotPrefab != null)
        {
            spot = Instantiate(placementSpotPrefab, position, Quaternion.identity, placementParent.transform);
        }
        else
        {
            spot = new GameObject("PlacementSpot");
            spot.transform.position = position;
            spot.transform.parent = placementParent.transform;
            
            SpriteRenderer sr = spot.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0, 1, 0, 0.3f);
            sr.sortingLayerName = "Ground";
            sr.sortingOrder = 1;
            spot.transform.localScale = Vector3.one * tileSize * 0.5f;
            
            spot.AddComponent<CircleCollider2D>();
        }
    }
    
    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.x - mapWidth / 2f) * tileSize,
            (gridPos.y - mapHeight / 2f) * tileSize,
            0
        );
    }
    
    Sprite CreateSquareSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    void CleanUp()
    {
        if (tilesParent != null)
            Destroy(tilesParent);
        if (placementParent != null)
            Destroy(placementParent);
    }
    
    // Public helper methods
    public List<Vector2> GetRandomPathWorld()
    {
        List<Vector2Int> pathToUse;
        
        if (branchPaths.Count > 0 && Random.value > 0.5f)
        {
            pathToUse = branchPaths[Random.Range(0, branchPaths.Count)];
        }
        else
        {
            pathToUse = mainPath;
        }
        
        List<Vector2> worldPath = new List<Vector2>();
        foreach (var gridPos in pathToUse)
        {
            worldPath.Add(GridToWorld(gridPos));
        }
        return worldPath;
    }
    
    public Vector2 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0)
            return GridToWorld(new Vector2Int(mapWidth / 2, 0));
        
        return GridToWorld(spawnPoints[Random.Range(0, spawnPoints.Count)]);
    }
    
    public Vector3 GetTowerPosition()
    {
        return GridToWorld(towerGridPos);
    }
}