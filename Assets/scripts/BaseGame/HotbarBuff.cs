using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HotbarBuffButton : MonoBehaviour
{
    [Header("UI References")]
    public Image buttonImage;
    public Image iconImage;
    public TextMeshProUGUI buffNameText;
    public Button button;
    public CanvasGroup canvasGroup;
    
    [Header("Icon Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    
    [HideInInspector]
    public BuffType buffType;
    
    private Action<BuffType> onClickCallback;
    private bool isUnlocked = false;
    
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        button.onClick.AddListener(OnButtonClicked);
    }
    
    public void Initialize(BuffType type, Action<BuffType> callback, string displayName)
    {
        buffType = type;
        onClickCallback = callback;
        
        if (buffNameText != null)
            buffNameText.text = displayName;
    }
    
    public void UpdateVisuals(bool unlocked)
    {
        isUnlocked = unlocked;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = unlocked ? 1f : 0.5f;
        }
        
        button.interactable = unlocked;
        
        // Swap icon sprite
        if (iconImage != null)
        {
            iconImage.sprite = unlocked ? unlockedSprite : lockedSprite;
        }
        
        if (buttonImage != null)
        {
            buttonImage.color = unlocked ? Color.white : Color.gray;
        }
    }
    
    void OnButtonClicked()
    {
        if (!isUnlocked)
        {
            Debug.Log("Buff not unlocked yet!");
            return;
        }
        
        onClickCallback?.Invoke(buffType);
    }
}