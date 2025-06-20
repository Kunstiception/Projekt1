using UnityEngine;

[CreateAssetMenu(fileName = "New VoiceLines", menuName = "Scriptable Object/VoiceLines")]
public class VoiceLines : ScriptableObject
{
    [Header("Lines")]
    [TextArea]
    public string[] Lines;
}
