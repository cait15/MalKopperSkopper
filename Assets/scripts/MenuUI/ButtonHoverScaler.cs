using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    public float scaleFactor = 1.1f;
    public float scaleDuration = 0.15f;
    
    private Vector3 originalScale;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.cancel(rectTransform.gameObject, false);
        
        LeanTween.scale(rectTransform, originalScale * scaleFactor, scaleDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(rectTransform.gameObject, false);
        
        LeanTween.scale(rectTransform, originalScale, scaleDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }
}