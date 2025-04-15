using System.Collections.Generic;
using UnityEngine;

public static class GameConfig
{
    [Header("Dialogue")]
    public static float TimeBetweenChars = 0.075f;
    public static float TimeBeforeNextLine = 0.75f;
    public static float TimeBeforeLevelLoad = 0.75f;

    [Header("Map")]
    public static float MovementSpeed = 1.5f;
    public static string[] WayPointTypes = { "Empty", "Fight", "Loot", "Interaction" };

}
