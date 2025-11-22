using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [Header("Ghost Preview")]
    private GameObject ghostUnit;
    private SpriteRenderer ghostRenderer;
    private UnitType selectedUnitType;
    private bool isPlacingUnit = false;
    
    [Header("Selected Unit")]
    private OfficerUnit selectedUnit;
    
    private GameObject[] placementSpots;
    private Dictionary<GameObject, bool> spotOccupancy = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, OfficerUnit> spotUnits = new Dictionary<GameObject, OfficerUnit>();
    
    void Start()
    {
        // Find all placement spot objects
        placementSpots = GameObject.FindGameObjectsWithTag("Placements");
        
        if (placementSpots.Length == 0)
        {
            Debug.LogError("No placement spots found with 'Placements' tag!");
        }
        else
        {
            Debug.Log($"Found {placementSpots.Length} placement spot(s)");
            
            // Initialize occupancy tracking
            foreach (GameObject spot in placementSpots)
            {
                spotOccupancy[spot] = false;
                spotUnits[spot] = null;
            }
        }
    }
    public void ClearPlacementForUnit(OfficerUnit unit)
    {
        foreach (var kvp in spotUnits)
        {
            if (kvp.Value == unit)
            {
                spotOccupancy[kvp.Key] = false;
                spotUnits[kvp.Key] = null;
                Debug.Log($"Cleared placement spot for dead unit");
                return;
            }
        }
    }
    void Update()
    {
        // Don't allow input during dialogue or battle phase
        if (GameManager.Instance.currentPhase == GamePhase.Dialogue ||
            GameManager.Instance.currentPhase == GamePhase.Battle)
        {
            if (ghostUnit != null)
            {
                Destroy(ghostUnit);
                isPlacingUnit = false;
            }
            return;
        }
        
        HandleGhostMovement();
        HandleUnitPlacement();
        HandleUnitSelection();
    }
    
    public void StartPlacingUnit(UnitType unitType)
    {
        // Only allow during setup phase
        if (GameManager.Instance.currentPhase != GamePhase.Setup)
        {
            Debug.Log("Can only place units during Setup Phase!");
            return;
        }
        
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
        
        if (!GameManager.Instance.CanAfford(stats.cost))
        {
            Debug.Log($"Not enough money! Need R{stats.cost}, have R{GameManager.Instance.playerMoney}");
            return;
        }
        
        selectedUnitType = unitType;
        isPlacingUnit = true;
        
        CreateGhostUnit(unitType);
        
        Debug.Log($"Click a placement spot to place {stats.unitName}");
    }
    
    void CreateGhostUnit(UnitType unitType)
    {
        if (ghostUnit != null)
        {
            Destroy(ghostUnit);
        }
        
        GameObject prefab = UnitDefinitions.Instance.GetUnitPrefab(unitType);
        if (prefab == null) return;
        
        ghostUnit = new GameObject("GhostUnit");
        ghostRenderer = ghostUnit.AddComponent<SpriteRenderer>();
        
        SpriteRenderer prefabRenderer = prefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer != null)
        {
            ghostRenderer.sprite = prefabRenderer.sprite;
            ghostRenderer.sortingLayerName = "Units";
            ghostRenderer.sortingOrder = 100;
        }
        
        Color ghostColor = Color.white;
        ghostColor.a = 0.5f;
        ghostRenderer.color = ghostColor;
        
        // Add range circle
        GameObject rangeCircle = new GameObject("RangeIndicator");
        rangeCircle.transform.parent = ghostUnit.transform;
        rangeCircle.transform.localPosition = Vector3.zero;
        
        LineRenderer lineRenderer = rangeCircle.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1, 1, 0, 0.5f);
        lineRenderer.endColor = new Color(1, 1, 0, 0.5f);
        lineRenderer.sortingLayerName = "Units";
        lineRenderer.sortingOrder = 99;
        
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(unitType);
        int segments = 50;
        lineRenderer.positionCount = segments + 1;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 360f / segments * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * stats.attackRange;
            float y = Mathf.Sin(angle) * stats.attackRange;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
    
    void HandleGhostMovement()
    {
        if (!isPlacingUnit || ghostUnit == null) return;
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        ghostUnit.transform.position = mousePos;
        
        GameObject nearestSpot = GetNearestPlacementSpot(mousePos);
        bool isOverValidSpot = nearestSpot != null && 
                               !spotOccupancy[nearestSpot] && 
                               Vector2.Distance(mousePos, nearestSpot.transform.position) < 1f;
        
        Color ghostColor = isOverValidSpot ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        ghostRenderer.color = ghostColor;
    }
    
    GameObject GetNearestPlacementSpot(Vector3 position)
    {
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;
        
        foreach (GameObject spot in placementSpots)
        {
            float distance = Vector2.Distance(position, spot.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = spot;
            }
        }
        
        return nearest;
    }
    
    void HandleUnitPlacement()
    {
        // Right click to cancel
        if (Input.GetMouseButtonDown(1) && isPlacingUnit)
        {
            CancelPlacement();
            return;
        }
        
        // ESC to cancel
        if (Input.GetKeyDown(KeyCode.Escape) && isPlacingUnit)
        {
            CancelPlacement();
            return;
        }
        
        // Left click to place
        if (isPlacingUnit && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            GameObject nearestSpot = GetNearestPlacementSpot(mousePos);
            
            if (nearestSpot != null && 
                !spotOccupancy[nearestSpot] && 
                Vector2.Distance(mousePos, nearestSpot.transform.position) < 1f)
            {
                PlaceUnitAtSpot(nearestSpot);
            }
        }
    }
    
    void PlaceUnitAtSpot(GameObject placementSpot)
    {
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(selectedUnitType);
        GameObject unitPrefab = UnitDefinitions.Instance.GetUnitPrefab(selectedUnitType);
        
        if (unitPrefab == null)
        {
            Debug.LogError($"No prefab found for {selectedUnitType}");
            return;
        }
        
        if (!GameManager.Instance.CanAfford(stats.cost))
        {
            Debug.Log("Not enough money!");
            CancelPlacement();
            return;
        }
        
        GameManager.Instance.SpendMoney(stats.cost);
        
        GameObject unitObj = Instantiate(unitPrefab, placementSpot.transform.position, Quaternion.identity);
        OfficerUnit unit = unitObj.GetComponent<OfficerUnit>();
        
        if (unit != null)
        {
            unit.stats = stats;
            
            spotOccupancy[placementSpot] = true;
            spotUnits[placementSpot] = unit;
            
            Debug.Log($"Placed {stats.unitName} at {placementSpot.name}");
        }
        
        CleanupAfterPlacement();
    }
    
    void CleanupAfterPlacement()
    {
        if (ghostUnit != null)
        {
            Destroy(ghostUnit);
        }
        isPlacingUnit = false;
    }
    
    void CancelPlacement()
    {
        if (ghostUnit != null)
        {
            Destroy(ghostUnit);
        }
        
        isPlacingUnit = false;
        Debug.Log("Placement cancelled");
    }
    
    void HandleUnitSelection()
    {
        if (!isPlacingUnit && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100f);
            
            if (hit.collider != null)
            {
                OfficerUnit unit = hit.collider.GetComponent<OfficerUnit>();
                if (unit != null)
                {
                    SelectUnit(unit);
                }
            }
        }
    }
    
    void SelectUnit(OfficerUnit unit)
    {
        selectedUnit = unit;
        Debug.Log($"Selected: {unit.stats.unitName} - Health: {unit.currentHealth}/{unit.stats.health}");
    }
    
    void OnGUI()
    {
        if (isPlacingUnit)
        {
            GUI.skin.label.fontSize = 16;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 30), "Right-click or ESC to cancel");
        }
    }
}