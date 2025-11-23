using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UnitStatsPanel : MonoBehaviour
{
    [Header("Panel References")]
    public CanvasGroup panelCanvasGroup;
    public Button toggleButton;
    public Button closeButton;
    
    [Header("Stats Display")]
    public Transform statsContainer;
    public GameObject statsDisplayPrefab;
    
    private bool isPanelOpen = false;
    
    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;
        
        DisplayAllUnits();
    }
    
    void DisplayAllUnits()
    {
        UnitType[] unitTypes = { UnitType.MeleeOfficerV1, UnitType.TankOfficer, UnitType.RangedOfficer, UnitType.Dog };
        
        foreach (UnitType type in unitTypes)
        {
            UnitStats stats = UnitDefinitions.Instance.GetUnitStats(type);
            
            GameObject displayObj = Instantiate(statsDisplayPrefab, statsContainer);
            TextMeshProUGUI displayText = displayObj.GetComponent<TextMeshProUGUI>();
            
            if (displayText != null)
            {
                displayText.text = FormatUnitStats(stats);
            }
        }
    }
    
    string FormatUnitStats(UnitStats stats)
    {
        string statsInfo = $"<b><size=120%>{stats.unitName}</size></b>\n" +
                         $"<b>Cost:</b> R{stats.cost}\n" +
                         $"<b>Health:</b> {stats.health}\n" +
                         $"<b>Speed:</b> {stats.speed}\n" +
                         $"<b>Damage:</b> {stats.damage}\n" +
                         $"<b>Attack Range:</b> {stats.attackRange}\n" +
                         $"<b>Attack Cooldown:</b> {stats.attackCooldown}s";
        
      
        return statsInfo;
    }
    
    void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = isPanelOpen ? 1f : 0f;
            panelCanvasGroup.interactable = isPanelOpen;
            panelCanvasGroup.blocksRaycasts = isPanelOpen;
        }
    }
    
    void ClosePanel()
    {
        isPanelOpen = false;
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }
}