using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

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
    
    private DialogueNode currentNode;
    private Coroutine typingCoroutine;
    private Coroutine iconAnimationCoroutine;
    private bool isTyping = false;
    
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
            DisplayNode(startNode);
        }
        else
        {
            Debug.Log($"No dialogues configured for wave {waveNumber}");
        }
    }
    
    public void ShowVictoryDialogue()
    {
        if (victoryDialogues != null && victoryDialogues.Count > 0)
        {
            DisplayNode(victoryDialogues[0]);
        }
    }
    
    public void ShowDefeatDialogue()
    {
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
        
        typingCoroutine = StartCoroutine(TypeText(node.dialogueText));
    }
    
    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";
        
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
            // End of dialogue
            if (currentNode.isEndDialogue)
            {
                CloseDialogue();
                TriggerNextPhase();
            }
            else
            {
                CloseDialogue();
            }
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
            // End of dialogue
            if (choice.isEndDialogue)
            {
                CloseDialogue();
                TriggerNextPhase();
            }
            else
            {
                CloseDialogue();
            }
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
    
    void TriggerNextPhase()
    {
        // Notify GameManager that dialogue ended
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDialogueEnded();
        }
    }
    
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}