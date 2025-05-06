using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://docs.unity3d.com/Manual/class-ScriptableObject.html

[CreateAssetMenu(fileName = "New Dialogue Manager", menuName = "Scriptable Object/Dialogue Manager")]

public static class DialogueManager
{
    public static string[] SleepDeprivedLines = 
        {"You feel weakened by your lack of sleep."};
    public static string[] HealedSleepDeprivedLines = 
        {"A good night's sleep has made your body regain its agility."};
    public static string[] combatLines;
    public static string[] cestingLines;
    public static string[] eventLines;
    public static string[] ItemLines;
    public static string[] itemNames;
}
