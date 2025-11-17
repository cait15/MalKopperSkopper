using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Tower Defense/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea(3, 10)]
    public string dialogueText;
    
    public string characterName;
    
    public DialogueChoice[] choices;
    
    public DialogueNode nextDefaultNode;
    
    [Header("Visual")]
    public List<Sprite> animatedSprites; // List of sprites for animation
    public float animationSpeed = 0.75f;
    
    [Header("Phase Control")]
    public bool isEndDialogue = false; // When true, triggers next phase
}