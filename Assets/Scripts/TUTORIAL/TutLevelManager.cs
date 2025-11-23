using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialLevelManager : MonoBehaviour
{
    public static TutorialLevelManager Instance;
    
    [Header("Tutorial Settings")]
    public bool isTutorial = true;
    public int tutorialWave = 1;
    public int tutorialTotalWaves = 1;
    
    [Header("UI References")]
    public TextMeshProUGUI tutorialText;
    public GameObject tutorialPanel;
    public GameObject hotBar;
    public GameObject UiText;
    public Button homeButton;
    public string mainMenuSceneName = "MainMenu"; // Change this to your main menu scene name
    
    [Header("Tutorial Steps")]
    private TutorialStep currentStep = TutorialStep.CameraControls;
    private bool dialogueFinished = false;
    private bool stepAdvancePressed = false;
    
    private bool unitPlaced = false;
    private bool waveStarted = false;
    private bool waveComplete = false;
    
    public enum TutorialStep
    {
        CameraControls,
        UIIndicators,
        PlaceFirstUnit,
        WaitForBattle,
        BattleInProgress,
        WaveComplete,
        TutorialFinished
    }
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Setup home button
        if (homeButton != null)
        {
            homeButton.gameObject.SetActive(false);
            homeButton.onClick.AddListener(OnHomeButtonPressed);
        }
        
        if (isTutorial)
        {
            InitializeTutorial();
        }
    }
    
    void InitializeTutorial()
    {
        // Don't show message here - wait for dialogue to finish first
        dialogueFinished = false;
    }
    
    void Update()
    {
        if (!isTutorial) return;
        
        // Wait for dialogue to finish before starting tutorial
        if (!dialogueFinished)
        {
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
            {
                dialogueFinished = true;
                StartCoroutine(StartTutorialStepsAfterDelay());
            }
            return;
        }
        
        // Camera controls step - press any KEY (not mouse) to continue
        if (currentStep == TutorialStep.CameraControls)
        {
            if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
            {
                if (!stepAdvancePressed)
                {
                    stepAdvancePressed = true;
                    AdvanceStep();
                }
            }
            else
            {
                stepAdvancePressed = false;
            }
        }
        
        // UI Indicators step - press any KEY (not mouse) to continue
        if (currentStep == TutorialStep.UIIndicators)
        {
            if (Input.anyKey && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
            {
                if (!stepAdvancePressed)
                {
                    stepAdvancePressed = true;
                    AdvanceStep();
                }
            }
            else
            {
                stepAdvancePressed = false;
            }
        }
        
        // Check if a unit has been placed
        if (currentStep == TutorialStep.PlaceFirstUnit)
        {
            if (HasUnitBeenPlaced())
            {
                unitPlaced = true;
                AdvanceStep();
            }
        }
        
        // Check if battle has started
        if (currentStep == TutorialStep.WaitForBattle)
        {
            if (TutGameManager.Instance.currentPhase == GamePhase.Battle)
            {
                waveStarted = true;
                AdvanceStep();
            }
        }
        
        // Check if wave is complete
        if (currentStep == TutorialStep.BattleInProgress)
        {
            if (TutGameManager.Instance.currentPhase == GamePhase.Victory)
            {
                waveComplete = true;
                AdvanceStep();
            }
        }
    }
    
    IEnumerator StartTutorialStepsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ShowTutorialMessage("Welcome to Tower Defense!\n\nPress and hold the Middle Mouse Button and move your mouse to rotate the camera. Please press space to continue");
        currentStep = TutorialStep.CameraControls;
    }
    
    bool HasUnitBeenPlaced()
    {
        List<OfficerUnit> activeUnits = TutGameManager.Instance.GetActiveUnits();
        return activeUnits.Count > 0;
    }
    
    void AdvanceStep()
    {
        switch (currentStep)
        {
            case TutorialStep.CameraControls:
                currentStep = TutorialStep.UIIndicators;
                UiText.SetActive(true);
                ShowTutorialMessage("Good!\n\nThese are your UI Indicators at the top:\n\n- Health: Your tower's health points\n- Money: Your available resources\n- Wave: Current wave number\n\nPress space to continue.");
                break;
                
            case TutorialStep.UIIndicators:
                UiText.SetActive(false);
                hotBar.SetActive(true);
                currentStep = TutorialStep.PlaceFirstUnit;
                ShowTutorialMessage("Now let's place your first unit!\n\nClick on a blue placement spot on the ground to place a unit.");
                break;
            
            case TutorialStep.PlaceFirstUnit:
                hotBar.SetActive(true);
                currentStep = TutorialStep.WaitForBattle;
                ShowTutorialMessage("Great job!\n\nWait for the setup timer to finish, or press ENTER to start the battle immediately.");
                // Start the setup phase countdown
                if (TutGameManager.Instance != null)
                {
                    TutGameManager.Instance.StartSetupPhaseNow();
                }
                break;
                
            case TutorialStep.WaitForBattle:
                currentStep = TutorialStep.BattleInProgress;
                ShowTutorialMessage("Battle started!\n\nEnemies are coming. Your units will automatically attack them. Defend your tower!");
                break;
                
            case TutorialStep.BattleInProgress:
                currentStep = TutorialStep.WaveComplete;
                ShowTutorialMessage("Victory!\n\nYou've completed the tutorial! Well done. You're ready for the real waves.");
                HideTutorialPanel(3f);
                ShowHomeButton();
                break;
                
            case TutorialStep.WaveComplete:
                currentStep = TutorialStep.TutorialFinished;
                break;
        }
    }
    
    void ShowTutorialMessage(string message)
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        if (tutorialText != null)
            tutorialText.text = message;
        
        Debug.Log($"[TUTORIAL] {message}");
    }
    
    void HideTutorialPanel(float delay)
    {
        StartCoroutine(HidePanelCoroutine(delay));
    }
    
    IEnumerator HidePanelCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
    
    void ShowHomeButton()
    {
        if (homeButton != null)
        {
            homeButton.gameObject.SetActive(true);
        }
    }
    
    void OnHomeButtonPressed()
    {
        Debug.Log("Going back to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}