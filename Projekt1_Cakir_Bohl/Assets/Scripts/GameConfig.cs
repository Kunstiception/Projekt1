public static class GameConfig
{
    // Player
    public static int PlayerStartingHealth = 10;
    public static int PlayerStartingEgo = 10;

    // Dialogue
    public static float TimeBetweenChars = 0.05f;
    public static float TimeBeforeNextLine = 0.5f;
    public static float TimeBeforeLevelLoad = 0.5f;
    public static float AnticipationCharsSpeed = 0.25f;
    public static float TimeAfterAnticipation = 0.75f;
    public static int ChanceForSecondLine = 7;

    // Map
    public static float MovementSpeed = 1.5f;
    public static string[] WayPointTypes = { "Empty", "Fight", "Loot", "Interaction", "Resting" };
    public static int TotalNumberOfDays = 7;

    // Combat
    public static float BarsLerpSpeed = 0.75f;
    public static float TimeBeforeHealthbarUpdate = 0.25f;
    public static float MaximumDamageModifier = 1.5f;
    public static float HitParticlesLength = 0.5f;
    public static float ExclamationLength = 1f;
    public static int EnemyFleeChance = 5;
    public static int TurnsBeforeSecondStage = 3;

    // Resting
    public static int AmbushChance = 4;
    public static int RoomCost = 10;
    public static int VampireCaughtChance = 3;
    public static int DogSaveChance = 5;
    public static float AnticipationLength = 3.5f;

    // Looting
    public static int LootCountDay = 1;
    public static int LootCountNight = 2;
    public static int EquipmentChance = 3;
    public static int EquipmentToAdd = 2;
    public static float WaitTimeAfterLoot = 0.25f;
    public static int MinimumCoinAmountDay = 3;
    public static int MaximumCoinAmountDay = 8;
    public static int MinimumCoinAmountNight = 6;
    public static int MaximumCoinAmountNight = 12;

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
    public static int ConditionScreenWaitTime = 4;
    public static int TotalAmountOfConditions = 4;

    // Ending
    public static float AnimationTime = 2f;

    // Sound
    public static float EffectsLoudness = 1.25f;
}
