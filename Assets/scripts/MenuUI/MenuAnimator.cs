using UnityEngine;
using System.Collections.Generic;

public class MenuAnimator : MonoBehaviour
{
    [Header("Title Settings")]
    public RectTransform titleRectTransform;
    public float initialScale = 0.1f;
    public float duration = 1.0f;
    public float jiggleAmount = 0.1f;
    public float jiggleElasticity = 0.5f;
    
    [Header("Title Throbbing")]
    public float throbbingScaleAmount = 0.05f; 
    public float throbbingDuration = 1.5f; 
    
    [Header("Button Settings")]
    public List<RectTransform> buttons; 
    public float buttonDropDuration = 0.5f;
    public float dropDelay = 0.1f; 

    void Start()
    {
        AnimateTitle();
        //AnimateButtons();
    }

    void AnimateTitle()
    {
        if (titleRectTransform == null)
        {
            Debug.LogError("Title RectTransform is not assigned!");
            return;
        }

        Vector3 originalScale = titleRectTransform.localScale;
        
        titleRectTransform.localScale = Vector3.one * initialScale;
        
        LeanTween.scale(titleRectTransform, originalScale * (1f + jiggleAmount), duration)
            .setEase(LeanTweenType.easeOutElastic)
            .setOvershoot(jiggleElasticity)
            .setOnComplete(() =>
            {
                LeanTween.scale(titleRectTransform, originalScale, 0.2f)
                    .setOnComplete(() =>
                    {
                        LeanTween.scale(titleRectTransform, originalScale * (1f + throbbingScaleAmount), throbbingDuration)
                            .setEase(LeanTweenType.easeInOutSine)
                            .setLoopPingPong();
                    });
            });
    }

    void AnimateButtons()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MenuAnimator must be a child of a Canvas!");
            return;
        }

        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform button = buttons[i];
            if (button == null) continue;

            // Cancel all existing tweens to prevent conflicts
            LeanTween.cancel(button.gameObject, false);
            
            Vector3 finalPosition = button.anchoredPosition;
            
            button.anchoredPosition = finalPosition + Vector3.up * canvasHeight;
            
            LeanTween.move(button, finalPosition, buttonDropDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setDelay(i * dropDelay);
        }
    }
}