using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Config", menuName = "Scriptable Object/Game Config")]

public class GameConfig : ScriptableObject
{
    [Header("Dialogue")]
    public float timeBetweenChars = 0.1f;
}
