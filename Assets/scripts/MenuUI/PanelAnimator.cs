using UnityEngine;

public class PanelAnimator : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideDuration = 0.4f;
    public LeanTweenType easeType = LeanTweenType.easeOutQuad; 
    public RectTransform rectTransform;
    
    private Vector2 originalAnchoredPosition;
    private float screenWidth;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("PanelAnimator requires a RectTransform component.");
            return;
        }

        originalAnchoredPosition = rectTransform.anchoredPosition;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            screenWidth = canvasRect.rect.width;
        }
        else
        {
            screenWidth = Screen.width;
        }
    }

    public void SlideIn(System.Action onCompleteCallback = null)
    {
        Vector2 offScreenStart = originalAnchoredPosition + Vector2.left * screenWidth;
        rectTransform.anchoredPosition = offScreenStart;
        
        gameObject.SetActive(true);
        
        LeanTween.move(rectTransform, originalAnchoredPosition, slideDuration)
            .setEase(easeType)
            .setOnComplete(() => {
                onCompleteCallback?.Invoke();
            });
    }

    public void SlideOut(System.Action onCompleteCallback = null)
    {
        Vector2 offScreenEnd = originalAnchoredPosition + Vector2.left * screenWidth;
        
        LeanTween.move(rectTransform, offScreenEnd, slideDuration)
            .setEase(easeType)
            .setOnComplete(() => {
                gameObject.SetActive(false);
                onCompleteCallback?.Invoke();
            });
    }
}