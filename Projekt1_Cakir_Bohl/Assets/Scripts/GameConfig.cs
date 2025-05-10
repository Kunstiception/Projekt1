public static class GameConfig
{
    //Player
    public static int PlayerStartingHealth = 10;
    public static int PlayerStartingEgo = 10;

    //Dialogue
    public static float TimeBetweenChars = 0.075f;
    public static float TimeBeforeNextLine = 0.75f;
    public static float TimeBeforeLevelLoad = 0.75f;

    //Map
    public static float MovementSpeed = 1.5f;
    public static string[] WayPointTypes = { "Empty", "Fight", "Loot", "Interaction", "Resting" };

    //Combat
    public static float BarsLerpSpeed = 0.75f;
    public static float TimeBeforeHealthbarUpdate = 0.25f;
    public static int AmbushChance = 2;

    //Looting
    public static int MaximumLootableItems = 3;

    //Items
    public static int MinimumHeal = 4;
    public static int MaximumHeal = 7;

    // Conditions
    public static int VampireSunDamage = 1;
}
