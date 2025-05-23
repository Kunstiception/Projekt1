using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueTopic", menuName = "Scriptable Object/DialogueTopic")]
public class DialogueTopic : ScriptableObject
{
    [Header("DialogueLines")]
    public DialogueLines[] IncluededDialogueLines;
}
