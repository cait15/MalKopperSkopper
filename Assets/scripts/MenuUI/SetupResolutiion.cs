using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; 

public class ResolutionSettings : MonoBehaviour
{
    // Singleton Instance: Keeps the object alive across scenes
    public static ResolutionSettings Instance;

    // UI Reference for the dropdown
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle; 

    // Data and Keys
    private Resolution[] resolutions;
    private const string ResolutionKey = "ResolutionIndex";
    private const string FullscreenKey = "Fullscreen";

    void Awake()
    {
        // Singleton pattern: Ensure only one instance exists and persists
       
    }

    void Start()
    {
        SetupResolution();
        SetupFullscreen();
    }

    // Call this from your main menu scene to assign the UI elements when the scene loads
    public void InitializeResolutionUI(TMP_Dropdown resolution, Toggle fullscreen)
    {
        resolutionDropdown = resolution;
        fullscreenToggle = fullscreen;
        
        SetupResolution();
        SetupFullscreen();
    }
    
    private void SetupResolution()
    {
        if (resolutionDropdown == null) return;

        // 1. Get all available resolutions (includes different refresh rates)
        resolutions = Screen.resolutions; 
        resolutionDropdown.ClearOptions();

        var options = new List<string>();

        // 2. Format the resolution options for the dropdown
        for (int i = 0; i < resolutions.Length; i++)
        {
            // Format: "1920 x 1080 @ 60Hz"
            string optionText = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
            options.Add(optionText);
        }
        
        resolutionDropdown.AddOptions(options);

        // 3. Load saved index or find current resolution index
        int savedIndex = PlayerPrefs.GetInt(ResolutionKey, GetCurrentResolutionIndex());
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        // 4. Apply the setting immediately (important for applying saved setting on start)
        ApplyResolution(savedIndex);

        // 5. Add Listener for future changes
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(index =>
        {
            ApplyResolution(index);
            PlayerPrefs.SetInt(ResolutionKey, index);
            PlayerPrefs.Save();
        });
    }

    private void ApplyResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;
        
        Resolution res = resolutions[index];
        bool isFullscreen = Screen.fullScreen; // Get the current fullscreen state

        // Apply the resolution, including the refresh rate
        Screen.SetResolution(res.width, res.height, isFullscreen, res.refreshRate);
    }

    private int GetCurrentResolutionIndex()
    {
        // Try to find the exact match (width, height, and refresh rate)
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                return i;
        }

        // Fallback to finding one with matching width/height
         for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                return i;
        }

        return 0; // Default to the first available resolution
    }

    // --- Fullscreen Setup (Needed to apply resolution correctly) ---

    private void SetupFullscreen()
    {
        if (fullscreenToggle == null) return;

        // Load saved setting or use current screen state as default
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;

        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.AddListener(value =>
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
            PlayerPrefs.Save();
            
            // Re-apply the resolution when fullscreen is toggled, if needed
            ApplyResolution(resolutionDropdown.value); 
        });
    }
}