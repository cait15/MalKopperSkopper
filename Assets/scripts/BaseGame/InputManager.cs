using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;  // Isometric game camera
    public Camera uiCamera;    // Separate UI camera
    
    [Header("Ghost Preview")]
    private GameObject ghostUnit;
    private SpriteRenderer ghostRenderer;
    private UnitType selectedUnitType;
    private bool isPlacingUnit = false;
    
    [Header("Selected Unit")]
    private OfficerUnit selectedUnit;
    
    [Header("Placement Height")]
    public float placementHeight = 0f;
    
    private GameObject[] placementSpots;
    private Dictionary<GameObject, bool> spotOccupancy = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, OfficerUnit> spotUnits = new Dictionary<GameObject, OfficerUnit>();
    
    void Start()
    {
        // Auto-find cameras if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (uiCamera == null)
            uiCamera = FindObjectOfType<Camera>();
        
        placementSpots = GameObject.FindGameObjectsWithTag("Placements");
        
        if (placementSpots.Length == 0)
        {
            Debug.LogError("No placement spots found with 'Placements' tag!");
        }
        else
        {
            Debug.Log($"Found {placementSpots.Length} placement spot(s)");
            
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
    
        // Look for SpriteRenderer on the prefab or its children
        SpriteRenderer prefabRenderer = prefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer == null)
            prefabRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
    
        if (prefabRenderer != null)
        {
            ghostRenderer.sprite = prefabRenderer.sprite;
            ghostRenderer.sortingLayerName = "Units";
            ghostRenderer.sortingOrder = 100;
        }
        
        Color ghostColor = Color.white;
        ghostColor.a = 0.5f;
        ghostRenderer.color = ghostColor;
        
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
            float z = Mathf.Sin(angle) * stats.attackRange;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }
    
    void HandleGhostMovement()
    {
        if (!isPlacingUnit || ghostUnit == null) return;
        
        Vector3 mousePos = GetMouseWorldPosition();
        ghostUnit.transform.position = mousePos;
        
        GameObject nearestSpot = GetNearestPlacementSpot(mousePos);
        bool isOverValidSpot = nearestSpot != null && 
                               !spotOccupancy[nearestSpot] && 
                               Vector3.Distance(mousePos, nearestSpot.transform.position) < 1f;
        
        Color ghostColor = isOverValidSpot ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        ghostRenderer.color = ghostColor;
    }
    
    GameObject GetNearestPlacementSpot(Vector3 position)
    {
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;
        
        foreach (GameObject spot in placementSpots)
        {
            float distance = Vector3.Distance(position, spot.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = spot;
            }
        }
        
        return nearest;
    }
    
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    
        // Raycast to find the nearest placement spot or terrain
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            return hit.point;
        }
    
        // Fallback to plane if nothing hit
        Plane groundPlane = new Plane(Vector3.up, placementHeight);
        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.origin + ray.direction * enter;
        }
    
        return Vector3.zero;
    }
    void HandleUnitPlacement()
    {
        if (Input.GetMouseButtonDown(1) && isPlacingUnit)
        {
            CancelPlacement();
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isPlacingUnit)
        {
            CancelPlacement();
            return;
        }
        
        if (isPlacingUnit && Input.GetMouseButtonDown(0))
        {
            // Don't place if clicking UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            Vector3 mousePos = GetMouseWorldPosition();
            
            GameObject nearestSpot = GetNearestPlacementSpot(mousePos);
            
            if (nearestSpot != null && 
                !spotOccupancy[nearestSpot] && 
                Vector3.Distance(mousePos, nearestSpot.transform.position) < 1f)
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
        
        Vector3 spawnPos = placementSpot.transform.position;
        spawnPos.y = placementHeight;
        
        GameObject unitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
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
            // Don't select if clicking UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
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