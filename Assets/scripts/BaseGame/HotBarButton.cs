using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HotbarButton : MonoBehaviour
{
    [Header("UI References")]
    public Image buttonImage;
    public Image iconImage;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI costText;
    public Button button;
    public CanvasGroup canvasGroup;
    
    [Header("Icon Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    
    [HideInInspector]
    public UnitType unitType;
    
    private Action<UnitType> onClickCallback;
    private bool isUnlocked = false;
    private bool canAfford = false;
    
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        button.onClick.AddListener(OnButtonClicked);
    }
    
    public void Initialize(UnitType type, Action<UnitType> callback)
    {
        unitType = type;
        onClickCallback = callback;
        
        UnitStats stats = UnitDefinitions.Instance.GetUnitStats(type);
        
        if (unitNameText != null)
            unitNameText.text = stats.unitName;
        
        if (costText != null)
            costText.text = $"R{stats.cost}";
    }
    
    public void UpdateVisuals(bool unlocked, bool affordable, int playerMoney)
    {
        isUnlocked = unlocked;
        canAfford = affordable;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = unlocked ? 1f : 0.5f;
        }
        
        button.interactable = unlocked && affordable;
        
        // Swap icon sprite
        if (iconImage != null)
        {
            iconImage.sprite = unlocked ? unlockedSprite : lockedSprite;
        }
        
        if (buttonImage != null)
        {
            if (!unlocked)
            {
                buttonImage.color = Color.gray;
            }
            else if (!affordable)
            {
                buttonImage.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                buttonImage.color = Color.white;
            }
        }
        
        if (costText != null)
        {
            if (!unlocked)
            {
                costText.color = Color.gray;
            }
            else if (!affordable)
            {
                costText.color = Color.red;
            }
            else
            {
                costText.color = Color.white;
            }
        }
    }
    
    void OnButtonClicked()
    {
        if (!isUnlocked)
        {
            Debug.Log("Unit not unlocked!");
            return;
        }
        
        if (!canAfford)
        {
            Debug.Log("Not enough money!");
            return;
        }
        
        onClickCallback?.Invoke(unitType);
    }
}