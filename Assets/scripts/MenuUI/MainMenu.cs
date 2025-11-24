using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement; // NEW: Added for scene loading

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject guidePanel;
    public GameObject tutorialButtons;

    [Header("Settings UI")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] predefinedResolutions = new Resolution[]
    {
        new Resolution { width = 1024, height = 768 },
        new Resolution { width = 1152, height = 864 },
        new Resolution { width = 1176, height = 664 },
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1280, height = 800 },
        new Resolution { width = 1280, height = 960 },
        new Resolution { width = 1280, height = 1024 },
        new Resolution { width = 1360, height = 768 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1680, height = 1050 },
        new Resolution { width = 1768, height = 992 },
        new Resolution { width = 1920, height = 1080 }
    };
    
    private List<Resolution> availableResolutions;

    void Start()
    {
        OpenMainMenu();
        
        SetupResolutionOptions();
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    private void SetupResolutionOptions()
    {
        availableResolutions = new List<Resolution>(predefinedResolutions);
        int currentResolutionIndex = 0;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();

            int currentScreenWidth = Screen.width;
            int currentScreenHeight = Screen.height;
            
            for (int i = 0; i < availableResolutions.Count; i++)
            {
                string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
                options.Add(option);

                if (availableResolutions[i].width == currentScreenWidth &&
                    availableResolutions[i].height == currentScreenHeight)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("Game"); 
        Debug.Log("Loading SampleScene...");
    }
    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial"); 
        Debug.Log("Loading SampleScene...");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        Debug.Log("Opening Settings Panel.");
    }

    public void OpenGuide()
    {
        mainMenuPanel.SetActive(false);
        guidePanel.SetActive(true);
        Debug.Log("Opening Guide Panel (How to Play).");
    }

    public void OpenMainMenu()
    {
        settingsPanel.SetActive(false);
        guidePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Debug.Log("Returning to Main Menu.");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnResolutionChange(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Count)
        {
            Resolution newResolution = availableResolutions[resolutionIndex];
            
            Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
            
            Debug.Log("Set Resolution via Dropdown to: " + newResolution.width + "x" + newResolution.height);
        }
    }
    
    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Fullscreen set to: " + isFullscreen);
    }
}