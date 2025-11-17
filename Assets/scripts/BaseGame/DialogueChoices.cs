using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Tower Defense/Dialogue Choice")]
public class DialogueChoice : ScriptableObject
{
    [TextArea(2, 5)]
    public string choiceText;
    
    public DialogueNode nextNode;
    
    [Header("Phase Control")]
    public bool isEndDialogue = false; // When true, triggers next phase
}