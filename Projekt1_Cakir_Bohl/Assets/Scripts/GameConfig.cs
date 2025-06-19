public static class GameConfig
{
    // Player
    public static int PlayerStartingHealth = 10;
    public static int PlayerStartingEgo = 10;

    // Dialogue
    public static float TimeBetweenChars = 0.05f;
    public static float TimeBeforeNextLine = 0.75f;
    public static float TimeBeforeLevelLoad = 0.5f;
    public static float AnticipationCharsSpeed = 0.25f;
    public static float TimeAfterAnticipation = 0.75f;
    public static int ChanceForSecondLine = 7;

    // Map
    public static float MovementSpeed = 1.5f;
    public static string[] WayPointTypes = { "Empty", "Fight", "Loot", "Interaction", "Resting" };

    // Combat
    public static float BarsLerpSpeed = 0.75f;
    public static float TimeBeforeHealthbarUpdate = 0.25f;
    public static float MaximumDamageModifier = 1.5f;
    public static float HitParticlesLength = 0.25f;
    public static float ExclamationLength = 1f;
    public static int EnemyFleeChance = 5;

    // Resting
    public static int AmbushChance = 10;
    public static int RoomCost = 8;
    public static int VampireCaughtChance = 3;

    // Looting
    public static int MaximumLootableItems = 3;
    public static int EquipmentToAdd = 2;
    public static float WaitTimeAfterLoot = 0.25f;

    // Items
    public static int MinimumHeal = 4;
    public static int MaximumHeal = 7;
    public static int AdditionalHealthPoints = 2;
    public static int AdditionalEgoPoints = 2;

    // Inventory
    public static int MaxInventorySlots = 10;

    // Conditions
    public static int VampireSunDamage = 1;
    public static int SleepDeprivedAccuracyModifier = 1;
    public static int SleepDeprivedEvasionModifier = 1;
    public static int SleepDeprivedInitiativeModifier = 1;
    public static int VampireEvasionModifier = 2;
    public static int VampireInsultDamageModifier = 2;
    public static int VampireHealthBoost = 2;
    public static int VampireEgoBoost = 2;
    public static int WerewolfAttackStrengthModifier = 2;
    public static int WerewolfInsultDamageModifier = 1;
    public static int ConditionScreenWaitTime = 2;

    // UI
    public static UnityEngine.Vector3 HealthbarDefaultPosition = new UnityEngine.Vector3(-379f, 29f, 0f);
    public static UnityEngine.Vector3 HealthbarAlternativePosition = new UnityEngine.Vector3(113, 29f, 0f);
}
