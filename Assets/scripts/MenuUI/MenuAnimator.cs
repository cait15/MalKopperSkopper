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
        AnimateButtons();
    }

    void AnimateTitle()
    {
        if (titleRectTransform == null)
        {
            Debug.LogError("Title RectTransform is not assigned!");
            return;
        }

        Vector3 originalScale = titleRectTransform.localScale;
        
        // Initial setup for the load-in jiggle
        titleRectTransform.localScale = Vector3.one * initialScale; 
        
        // Load-in Jiggle Animation
        LeanTween.scale(titleRectTransform, originalScale * (1f + jiggleAmount), duration)
            .setEase(LeanTweenType.easeOutElastic) 
            .setOvershoot(jiggleElasticity) 
            .setOnComplete(() =>
            {
                // Final snap back to original size after jiggle
                LeanTween.scale(titleRectTransform, originalScale, 0.2f)
                .setOnComplete(() => 
                {
                    // Start Continuous Throbbing Loop
                    LeanTween.scale(titleRectTransform, originalScale * (1f + throbbingScaleAmount), throbbingDuration)
                        .setEase(LeanTweenType.easeInOutSine) // Smooth pulse effect
                        .setLoopPingPong(); // Makes it scale up, then down, then up, forever
                });
            });
    }

    void AnimateButtons()
    {
        float canvasHeight = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.height;
        
        for (int i = 0; i < buttons.Count; i++)
        {
            RectTransform button = buttons[i];
            if (button == null) continue;

            Vector3 finalPosition = button.anchoredPosition; 
            
            button.anchoredPosition = finalPosition + Vector3.up * canvasHeight; 
            
            button.gameObject.AddComponent<ButtonHoverScaler>();
            
            LeanTween.move(button, finalPosition, buttonDropDuration)
                .setEase(LeanTweenType.easeOutQuad) 
                .setDelay(i * dropDelay); 
        }
    }
}