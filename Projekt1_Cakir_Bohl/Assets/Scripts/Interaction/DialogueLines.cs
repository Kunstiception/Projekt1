using UnityEngine;


// basiert auf: https://www.youtube.com/watch?v=vR6H3mu_xD8&list=PLSR2vNOypvs4Pc72kaB_Y1v3AszNd-UuF
[CreateAssetMenu(fileName = "New DialogueLines", menuName = "Scriptable Object/DialogueLines")]
public class DialogueLines : ScriptableObject
{
    [Header("Lines")]
    [TextArea]
    public string[] Lines;

    [Header("Player Options")]
    [TextArea]
    public string[] PlayerOptions;
    public DialogueLines[] BranchingLines;
}
