using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DialogueLines", menuName = "Scriptable Object/DialogueLines")]
public class DialogueLines : ScriptableObject
{
    public string[] OpeningLines;

    public string[] PlayerQuestions;

    public string[] NPCAnswers;
}
