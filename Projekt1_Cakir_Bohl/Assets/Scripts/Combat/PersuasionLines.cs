using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PeruasionLines", menuName = "Scriptable Object/PeruasionLines")]
public class PersuasionLines : ScriptableObject
{
    [Header("Insults")]
    public List<string> CorrectInsults;
    public List<string> WrongInsults;
}
