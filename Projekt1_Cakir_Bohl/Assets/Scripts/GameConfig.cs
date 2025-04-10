public static class GameConfig
{
    //Dialogue
    public static float TimeBetweenChars = 0.075f;
    public static float TimeBeforeNextLine = 0.75f;
    public static float TimeBeforeLevelLoad = 0.75f;

    //Map
    public static float MovementSpeed = 1.5f;
    public static string[] WayPointTypes = { "Empty", "Fight", "Loot", "Interaction" };

    //Combat
    public static float BarsLerpSpeed = 0.75f;
    public static float TimeBeforeHealthbarUpdate = 0.25f;
}
