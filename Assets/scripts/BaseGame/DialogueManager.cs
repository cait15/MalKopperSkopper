using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;
    public Image characterImageHolder;
    
    [Header("Buttons")]
    public List<Button> choiceButtons;
    public Button continueButton;
    
    [Header("Typewriter Settings")]
    public float typeSpeed = 0.02f;
    
    [Header("Dialogue Nodes")]
    public List<DialogueNode> wave1Dialogues;
    public List<DialogueNode> wave2Dialogues;
    public List<DialogueNode> wave3Dialogues;
    public List<DialogueNode> wave4Dialogues;
    public List<DialogueNode> wave5Dialogues;
    public List<DialogueNode> wave6Dialogues;
    public List<DialogueNode> wave7Dialogues;
    public List<DialogueNode> wave8Dialogues;
    public List<DialogueNode> wave9Dialogues;
    public List<DialogueNode> wave10Dialogues;
    public List<DialogueNode> victoryDialogues;
    public List<DialogueNode> defeatDialogues;
    
    [Header("Audio")]
    public AudioClip clickSound;
    public AudioSource audioSource;
    public AudioClip dialogueStartSound;
    
    
    private DialogueNode currentNode;
    private Coroutine typingCoroutine;
    private Coroutine iconAnimationCoroutine;
    private bool isTyping = false;
    private bool isTutorial = false;
    private bool isEndgameDialogue = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        // Check if we're in tutorial mode
        isTutorial = TutorialLevelManager.Instance != null && TutorialLevelManager.Instance.isTutorial;
        
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }
    
    public void ShowDialogueForWave(int waveNumber)
    {
        List<DialogueNode> dialogues = GetDialoguesForWave(waveNumber);
        
        if (dialogues != null && dialogues.Count > 0)
        {
            DialogueNode startNode = dialogues[0];
            isEndgameDialogue = false;
            DisplayNode(startNode);
        }
        else
        {
            Debug.Log($"No dialogues configured for wave {waveNumber}");
            // Skip to setup if no dialogue exists
            OnDialogueEnd();
        }
    }
    
    public void ShowVictoryDialogue()
    {
        isEndgameDialogue = true;
        if (victoryDialogues != null && victoryDialogues.Count > 0)
        {
            DisplayNode(victoryDialogues[0]);
        }
    }
    
    public void ShowDefeatDialogue()
    {
        isEndgameDialogue = true;
        if (defeatDialogues != null && defeatDialogues.Count > 0)
        {
            DisplayNode(defeatDialogues[0]);
        }
    }
    
    List<DialogueNode> GetDialoguesForWave(int wave)
    {
        switch (wave)
        {
            case 1: return wave1Dialogues;
            case 2: return wave2Dialogues;
            case 3: return wave3Dialogues;
            case 4: return wave4Dialogues;
            case 5: return wave5Dialogues;
            case 6: return wave6Dialogues;
            case 7: return wave7Dialogues;
            case 8: return wave8Dialogues;
            case 9: return wave9Dialogues;
            case 10: return wave10Dialogues;
            default: return null;
        }
    }
    
    public void DisplayNode(DialogueNode node)
    {
        if (node == null) return;
        
        currentNode = node;
        
        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // Set character name
        if (characterNameText != null)
        {
            characterNameText.text = !string.IsNullOrEmpty(node.characterName) ? node.characterName : "";
        }
        
        // Stop any previous animation
        if (iconAnimationCoroutine != null)
            StopCoroutine(iconAnimationCoroutine);
        
        // Handle animated sprites
        if (node.animatedSprites != null && node.animatedSprites.Count > 0)
        {
            iconAnimationCoroutine = StartCoroutine(AnimateIcon(node.animatedSprites, node.animationSpeed));
        }
        else if (characterImageHolder != null)
        {
            characterImageHolder.sprite = null;
        }
        
        // Clear old button listeners and hide all choice buttons
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
        
        // Stop previous typing and start new
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText, node.nodeAudio));
    }
    
    IEnumerator TypeText(string fullText, AudioClip nodeAudio)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // Play node audio if assigned, otherwise play dialogue start sound
        if (nodeAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(nodeAudio);
        }
        else if (dialogueStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogueStartSound);
        }
        
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        
        isTyping = false;
        ShowButtons();
    }
    
    void ShowButtons()
    {
        if (currentNode.choices != null && currentNode.choices.Length > 0)
        {
            // Show choice buttons
            for (int i = 0; i < currentNode.choices.Length && i < choiceButtons.Count; i++)
            {
                DialogueChoice choice = currentNode.choices[i];
                Button button = choiceButtons[i];
                
                button.gameObject.SetActive(true);
                TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
                btnText.text = choice.choiceText;
                
                button.onClick.AddListener(() => {
                    PlayClickSound();
                    OnChoiceSelected(choice);
                });
            }
            
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            // Show continue button
            continueButton.gameObject.SetActive(true);
        }
    }
    
    public void OnContinuePressed()
    {
        PlayClickSound();
        
        if (isTyping)
        {
            // Skip typing animation
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentNode.dialogueText;
            isTyping = false;
            ShowButtons();
        }
        else if (currentNode.nextDefaultNode != null)
        {
            // Continue to next node
            DisplayNode(currentNode.nextDefaultNode);
        }
        else
        {
            // No more dialogue, end the conversation
            CloseDialogue();
            OnDialogueEnd();
        }
    }
    
    public void OnChoiceSelected(DialogueChoice choice)
    {
        if (choice.nextNode != null)
        {
            DisplayNode(choice.nextNode);
        }
        else
        {
            // No next node for this choice, end the conversation
            Debug.Log("Choice has no next node, ending dialogue");
            CloseDialogue();
            OnDialogueEnd();
        }
    }
    
    IEnumerator AnimateIcon(List<Sprite> sprites, float speed)
    {
        int index = 0;
        
        while (true)
        {
            if (characterImageHolder != null)
            {
                characterImageHolder.sprite = sprites[index];
            }
            index = (index + 1) % sprites.Count;
            yield return new WaitForSeconds(speed);
        }
    }
    
    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    public void ManualCloseDialogue()
    {
        CloseDialogue();
    }
    
    void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        // Stop animations
        if (iconAnimationCoroutine != null)
        {
            StopCoroutine(iconAnimationCoroutine);
            iconAnimationCoroutine = null;
        }
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    
    void OnDialogueEnd()
    {
        // Check if this is an endgame dialogue (victory/defeat)
        if (isEndgameDialogue)
        {
            isEndgameDialogue = false;
            StartCoroutine(LoadHomeAfterDelay(1f));
            return;
        }
        
        // Regular wave dialogue - transition to next phase
        if (isTutorial)
        {
            if (TutGameManager.Instance != null)
            {
                TutGameManager.Instance.OnDialogueEnded();
            }
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDialogueEnded();
            }
        }
    }
    
    IEnumerator LoadHomeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Main Screen"); // Change "MainMenu" to your home scene name
    }
    
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}