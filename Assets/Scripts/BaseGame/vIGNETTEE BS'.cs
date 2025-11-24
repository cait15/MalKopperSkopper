using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HitFlashEffectVignetteOnly : MonoBehaviour
{
    [Header("Volume Setup")]
    [SerializeField] private VolumeProfile volumeProfileAsset;
    [SerializeField] private Volume globalVolume;

    [Header("Hit Effect")]
    [SerializeField] private float maxIntensity = 0.7f;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float pulseFrequency = 10f;

    private Vignette vignette;
    private Color hitColor;

    private void Start()
    {
        if (globalVolume == null || volumeProfileAsset == null)
        {
            Debug.LogError("HitFlashEffect: Volume references are missing. Disabling script.");
            enabled = false;
            return;
        }

        if (globalVolume.profile != volumeProfileAsset)
        {
            globalVolume.profile = volumeProfileAsset;
        }

        if (!volumeProfileAsset.TryGet<Vignette>(out vignette))
        {
            Debug.LogError("HitFlashEffect: Vignette override not found in the Volume Profile. Disabling script.");
            enabled = false;
            return;
        }

        vignette.active = true;
        vignette.intensity.value = 0f;
    }

    private void OnEnable()
    {
        GameManager.OnDamagedTriggered += TakeDamagePulse;
    }

    private void OnDisable()
    {
        GameManager.OnDamagedTriggered -= TakeDamagePulse;
    }

    public void TakeDamagePulse()
    {
        StopAllCoroutines();
        StartCoroutine(HitFlashCoroutine());
        Debug.Log("CRAZY I GOT DMG'D");
    }

    private IEnumerator HitFlashCoroutine()
    {
        float flashTimer = 0f;

        // FLASH PHASE: Hold at max intensity
        while (flashTimer < flashDuration)
        {
            vignette.intensity.value = maxIntensity;
            flashTimer += Time.deltaTime;
            yield return null;
        }

        // FADE PHASE: Fade out with pulse
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            float fadeFactor = 1f - (fadeTimer / fadeDuration);
            float pulse = (Mathf.Sin(fadeTimer * pulseFrequency * Mathf.PI) * 0.5f) + 0.5f;
            float currentIntensity = maxIntensity * fadeFactor * pulse;

            vignette.intensity.value = currentIntensity;

            fadeTimer += Time.deltaTime;
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
}