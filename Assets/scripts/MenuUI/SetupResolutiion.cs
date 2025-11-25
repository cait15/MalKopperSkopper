using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; 

public class ResolutionSettings : MonoBehaviour
{
    public static ResolutionSettings Instance;

    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
  
    private Resolution[] resolutions;
    private const string ResolutionKey = "ResolutionIndex";
    private const string FullscreenKey = "Fullscreen";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Don't setup here - wait for InitializeResolutionUI to be called
    }

    public void InitializeResolutionUI(TMP_Dropdown resolution, Toggle fullscreen)
    {
        resolutionDropdown = resolution;
        fullscreenToggle = fullscreen;
        SetupResolution();
        SetupFullscreenToggle();
    }
    
    private void SetupResolution()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions; 
        resolutionDropdown.ClearOptions();

        var options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string optionText = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
            options.Add(optionText);
        }
        
        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt(ResolutionKey, GetCurrentResolutionIndex());
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(index =>
        {
            ApplyResolution(index);
            PlayerPrefs.SetInt(ResolutionKey, index);
            PlayerPrefs.Save();
        });
    }

    public void SetupFullscreenToggle()
    {
        if (fullscreenToggle == null)
        {
            Debug.LogError("Fullscreen toggle is NULL!");
            return;
        }

        Debug.Log("Setting up fullscreen toggle...");
        
        // Load saved fullscreen state
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        Debug.Log("Loaded fullscreen state from PlayerPrefs: " + isFullscreen);
        fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

        // Add listener for toggle changes
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.AddListener(isOn =>
        {
            PlayerPrefs.SetInt(FullscreenKey, isOn ? 1 : 0);
            PlayerPrefs.Save();
            
            // Apply the current resolution with new fullscreen state
            int currentIndex = resolutionDropdown.value;
            ApplyResolution(currentIndex);
            
            Debug.Log($"Fullscreen toggled: {isOn}");
        });
    }

    private void ApplyResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;
        
        Resolution res = resolutions[index];
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        
        Debug.Log($"ApplyResolution: {res.width}x{res.height}, Fullscreen: {isFullscreen}");

        if (isFullscreen)
        {
            Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen, res.refreshRate);
        }
        else
        {
            Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed, res.refreshRate);
        }
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                return i;
        }

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                return i;
        }

        return 0;
    }
}