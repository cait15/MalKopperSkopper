using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    [Header("Spot Settings")]
    public bool isOccupied = false;
    public OfficerUnit placedUnit;
    
    [Header("Visual")]
    public SpriteRenderer highlightSprite;
    public Sprite highlightCircleSprite; // Assign in Inspector
    public Color availableColor = new Color(0, 1, 0, 0.4f); // Green transparent
    public Color occupiedColor = new Color(1, 0, 0, 0.4f); // Red transparent
    public Color hoverColor = new Color(1, 1, 0, 0.6f); // Yellow transparent
    
    private bool isHovering = false;
    private UnitType pendingUnitType;
    private bool placementMode = false;
    
    void Start()
    {
        SetupVisual();
        UpdateVisual();
    }
    
    void SetupVisual()
    {
        // Create highlight sprite if it doesn't exist
        if (highlightSprite == null)
        {
            GameObject highlight = new GameObject("Highlight");
            highlight.transform.parent = transform;
            highlight.transform.localPosition = Vector3.zero;
            
            highlightSprite = highlight.AddComponent<SpriteRenderer>();
            highlightSprite.sortingOrder = -1; // Behind everything
            
            // If no sprite assigned, create a simple circle
            if (highlightCircleSprite != null)
            {
                highlightSprite.sprite = highlightCircleSprite;
            }
        }
    }
    
    void UpdateVisual()
    {
        if (highlightSprite == null) return;
        
        if (!placementMode)
        {
            // Hide when not in placement mode
            highlightSprite.color = Color.clear;
        }
        else if (isHovering && !isOccupied)
        {
            highlightSprite.color = hoverColor;
        }
        else if (isOccupied)
        {
            highlightSprite.color = occupiedColor;
        }
        else
        {
            highlightSprite.color = availableColor;
        }
    }
    
    public void EnablePlacementMode(UnitType unitType)
    {
        placementMode = true;
        pendingUnitType = unitType;
        UpdateVisual();
    }
    
    public void DisablePlacementMode()
    {
        placementMode = false;
        isHovering = false;
        UpdateVisual();
    }
    
    void OnMouseEnter()
    {
        if (placementMode && !isOccupied)
        {
            isHovering = true;
            UpdateVisual();
        }
    }
    
    void OnMouseExit()
    {
        isHovering = false;
        UpdateVisual();
    }
    
    void OnMouseDown()
    {
        if (!placementMode || isOccupied) return;
        
        TryPlaceUnit(pendingUnitType);
    }
    
    public bool TryPlaceUnit(UnitType unitType)
    {
        if (isOccupied)
        {
            Debug.Log("Spot already occupied!");
            return false;
        }
        
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
        
        if (!GameManager.Instance.CanAfford(stats.cost))
        {
            Debug.Log("Not enough money!");
            return false;
        }
        
        // Spawn the unit
        GameObject unitPrefab = UnitDefinitions.Instance.GetUnitPrefab(unitType);
        if (unitPrefab == null)
        {
            Debug.LogError("Unit prefab not found!");
            return false;
        }
        
        GameObject unitObj = Instantiate(unitPrefab, transform.position, Quaternion.identity);
        OfficerUnit unit = unitObj.GetComponent<OfficerUnit>();
        
        if (unit != null)
        {
            unit.stats = stats;
            unit.placementSpot = this;
            placedUnit = unit;
            isOccupied = true;
            
            GameManager.Instance.SpendMoney(stats.cost);
            UpdateVisual();
            
            // Disable placement mode for all spots
            PlacementSpot[] allSpots = FindObjectsOfType<PlacementSpot>();
            foreach (PlacementSpot spot in allSpots)
            {
                spot.DisablePlacementMode();
            }
            
            Debug.Log($"Placed {stats.unitName} at position {transform.position}");
            return true;
        }
        
        return false;
    }
    
    public void RemoveUnit()
    {
        if (placedUnit != null)
        {
            Destroy(placedUnit.gameObject);
            placedUnit = null;
        }
        
        isOccupied = false;
        UpdateVisual();
    }
}