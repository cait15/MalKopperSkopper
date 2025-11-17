using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Ghost Preview")]
    private GameObject ghostUnit;
    private SpriteRenderer ghostRenderer;
    private UnitType selectedUnitType;
    private bool isPlacingUnit = false;
    
    [Header("Selected Unit")]
    private OfficerUnit selectedUnit;
    
    void Update()
    {
        // Don't allow input during dialogue or battle phase
        if (GameManager.Instance.currentPhase == GamePhase.Dialogue ||
            GameManager.Instance.currentPhase == GamePhase.Battle ||
            GameManager.Instance.currentPhase == GamePhase.Victory)
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
        
        // Enable all placement spots
        PlacementSpot[] spots = FindObjectsOfType<PlacementSpot>();
        foreach (PlacementSpot spot in spots)
        {
            spot.EnablePlacementMode(unitType);
        }
        
        // Create ghost unit
        CreateGhostUnit(unitType);
        
        Debug.Log($"Click a green spot to place {stats.unitName}");
    }
    
    void CreateGhostUnit(UnitType unitType)
    {
        // Destroy existing ghost
        if (ghostUnit != null)
        {
            Destroy(ghostUnit);
        }
        
        // Get the prefab for this unit type
        GameObject prefab = UnitDefinitions.Instance.GetUnitPrefab(unitType);
        if (prefab == null) return;
        
        // Create ghost
        ghostUnit = new GameObject("GhostUnit");
        ghostRenderer = ghostUnit.AddComponent<SpriteRenderer>();
        
        // Copy sprite from prefab
        SpriteRenderer prefabRenderer = prefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer != null)
        {
            ghostRenderer.sprite = prefabRenderer.sprite;
            ghostRenderer.sortingLayerName = "Units";
            ghostRenderer.sortingOrder = 100;
        }
        
        // Make it semi-transparent
        Color ghostColor = Color.white;
        ghostColor.a = 0.5f;
        ghostRenderer.color = ghostColor;
        
        // Add circle to show range
        GameObject rangeCircle = new GameObject("RangeIndicator");
        rangeCircle.transform.parent = ghostUnit.transform;
        rangeCircle.transform.localPosition = Vector3.zero;
        
        LineRenderer lineRenderer = rangeCircle.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1, 1, 0, 0.5f);
        lineRenderer.startColor = new Color(1, 1, 0, 0.5f);
        lineRenderer.endColor = new Color(1, 1, 0, 0.5f);
        lineRenderer.sortingLayerName = "Units";
        lineRenderer.sortingOrder = 99;
        
        // Draw circle for attack range
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
        
        // Follow mouse cursor
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        ghostUnit.transform.position = mousePos;
        
        // Change color based on valid placement
        bool isOverValidSpot = IsOverValidPlacementSpot(mousePos);
        Color ghostColor = isOverValidSpot ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        ghostRenderer.color = ghostColor;
    }
    
    bool IsOverValidPlacementSpot(Vector3 position)
    {
        PlacementSpot[] spots = FindObjectsOfType<PlacementSpot>();
        foreach (PlacementSpot spot in spots)
        {
            if (!spot.isOccupied && Vector2.Distance(position, spot.transform.position) < 0.5f)
            {
                return true;
            }
        }
        return false;
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
        
        // Left click to place (handled by PlacementSpot's OnMouseDown)
        if (isPlacingUnit && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsOverValidPlacementSpot(mousePos))
            {
                Invoke(nameof(CleanupAfterPlacement), 0.1f);
            }
        }
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
        
        // Disable all placement spots
        PlacementSpot[] spots = FindObjectsOfType<PlacementSpot>();
        foreach (PlacementSpot spot in spots)
        {
            spot.DisablePlacementMode();
        }
        
        Debug.Log("Placement cancelled");
    }
    
    void HandleUnitSelection()
    {
        // Click to select units (only when not placing)
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
        // Show selected unit info
        if (selectedUnit != null && selectedUnit.isAlive)
        {
            GUI.skin.label.fontSize = 14;
            GUI.Label(new Rect(Screen.width - 300, 10, 280, 30), "=== SELECTED UNIT ===");
            GUI.Label(new Rect(Screen.width - 300, 35, 280, 30), $"Name: {selectedUnit.stats.unitName}");
            GUI.Label(new Rect(Screen.width - 300, 60, 280, 30), $"Health: {selectedUnit.currentHealth}/{selectedUnit.stats.health + selectedUnit.temporaryHealthBonus}");
            GUI.Label(new Rect(Screen.width - 300, 85, 280, 30), $"Damage: {selectedUnit.stats.damage + selectedUnit.temporaryDamageBonus}");
            GUI.Label(new Rect(Screen.width - 300, 110, 280, 30), $"Range: {selectedUnit.stats.attackRange}");
        }
        
        // Show cancel instruction when placing
        if (isPlacingUnit)
        {
            GUI.skin.label.fontSize = 16;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 50, 300, 30), "Right-click or ESC to cancel");
        }
    }
}