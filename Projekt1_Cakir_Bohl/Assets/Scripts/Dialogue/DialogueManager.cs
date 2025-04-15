using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://docs.unity3d.com/Manual/class-ScriptableObject.html

[CreateAssetMenu(fileName = "New Dialogue Manager", menuName = "Scriptable Object/Dialogue Manager")]

public class DialogueManager : ScriptableObject
{
    public string[] standardLines = 
        {"dkdkdkk", 
        "ksajsdljksaldkj"};
    public string[] combatLines;
    public string[] cestingLines;
    public string[] eventLines;
    public string[] ItemLines;
    public string[] itemNames;
}
