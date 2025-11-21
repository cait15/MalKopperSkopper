using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    [Header("Settings")]
    public float spotSize = 1f;
    
    [Header("State")]
    public bool isOccupied = false;
    private OfficerUnit occupyingUnit;
    
    [Header("Visuals")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInPlacementMode = false;
    private UnitType allowedUnitType;
    
    void Start()
    {
        // Setup sprite renderer for this spot
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // Create a simple green square sprite
        spriteRenderer.sprite = CreateSquareSprite();
        spriteRenderer.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
        spriteRenderer.sortingLayerName = "Ground";
        spriteRenderer.sortingOrder = 2;
        
        originalColor = spriteRenderer.color;
        
        // Add collider for mouse detection
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * spotSize;
        }
    }
    
    void OnMouseEnter()
    {
        if (isInPlacementMode && !isOccupied)
        {
            spriteRenderer.color = new Color(0, 1, 0, 0.6f); // Brighter green
        }
    }
    
    void OnMouseExit()
    {
        if (isInPlacementMode && !isOccupied)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    void OnMouseDown()
    {
        if (isInPlacementMode && !isOccupied && GameManager.Instance.currentPhase == GamePhase.Setup)
        {
            UnitStats stats = UnitDefinitions.Instance.GetUnitStats(allowedUnitType);
            
            if (GameManager.Instance.CanAfford(stats.cost))
            {
                GameManager.Instance.SpendMoney(stats.cost);
                PlaceUnit(allowedUnitType);
                DisablePlacementMode();
            }
            else
            {
                Debug.Log($"Not enough money! Need R{stats.cost}");
            }
        }
    }
    
    public void EnablePlacementMode(UnitType unitType)
    {
        isInPlacementMode = true;
        allowedUnitType = unitType;
        
        if (!isOccupied)
        {
            spriteRenderer.color = new Color(0, 1, 0, 0.3f);
        }
    }
    
    public void DisablePlacementMode()
    {
        isInPlacementMode = false;
        
        if (!isOccupied)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    public void PlaceUnit(UnitType unitType)
    {
        GameObject prefab = UnitDefinitions.Instance.GetUnitPrefab(unitType);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for unit type: {unitType}");
            return;
        }
        
        GameObject unitObj = Instantiate(prefab, transform.position, Quaternion.identity);
        OfficerUnit unit = unitObj.GetComponent<OfficerUnit>();
        
        if (unit != null)
        {
            unit.stats = UnitDefinitions.Instance.GetUnitStats(unitType);
            //unit.placementSpot = this;
            isOccupied = true;
            occupyingUnit = unit;
            
            // Hide the placement spot visually when occupied
            spriteRenderer.color = new Color(0, 1, 0, 0.1f);
        }
        
        Debug.Log($"Unit placed at spot: {gameObject.name}");
    }
    
    public void RemoveUnit()
    {
        isOccupied = false;
        occupyingUnit = null;
        spriteRenderer.color = originalColor;
        DisablePlacementMode();
    }
    
    Sprite CreateSquareSprite()
    {
        int size = 64;
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
}