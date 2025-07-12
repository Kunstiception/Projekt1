public class PlayerManager: Combatant
{ 
    public static PlayerManager Instance;

    public int HealthPointsModifier;
    public int EgoPointsModifier;
    public int AttackStrengthModifier;
    public int InitiativeModifier;
    public int EvasionModifier;
    public int InsultResistanceModifier;
    public int AccuracyModifier;
    public int InsultDamageModifier;
    public bool HasRoom = false;
    public bool HasDisadvantage = false;
    public bool GotCaught = false;
    public bool HasFinishedDay = false;
    public bool HasReachedBoss = false;
    public bool IsAuto = false;
    public ConditionManager.Conditions LatestCondition;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetStats()
    {

    }

    public void InitializePlayerStats()
    {
        HealthPointsModifier = MainManager.Instance.HealthPointsModifier;
        EgoPointsModifier = MainManager.Instance.EgoPointsModifier;
        AttackStrengthModifier = MainManager.Instance.AttackStrengthModifier;
        InitiativeModifier = MainManager.Instance.InitiativeModifier;
        EvasionModifier = MainManager.Instance.EvasionModifier;
        InsultDamageModifier = MainManager.Instance.InsultResistenceModifier;
        AccuracyModifier = MainManager.Instance.AccuracyModifier;
        InsultDamageModifier = MainManager.Instance.InsultDamageModifier;
        HealthPoints = MainManager.Instance.PlayerHealthPoints + HealthPointsModifier;
        EgoPoints = MainManager.Instance.PlayerEgoPoints + EgoPointsModifier;
    }

    public int GetStartingHealth()
    {
        return GameConfig.PlayerStartingHealth + HealthPointsModifier;
    }

    public int GetStartingEgo()
    {
        return GameConfig.PlayerStartingEgo + EgoPointsModifier;
    }

    public int GetInitiative()
    {
        return Initiative + InitiativeModifier;
    }

    public int GetEvasion()
    {
        return Evasion + EvasionModifier;
    }

    public int GetEgoResistence()
    {
        return InsultResistance + InsultResistanceModifier;
    }

    public int GetAccuracy()
    {
        return Accuracy + AccuracyModifier;
    }

    public override int RollInitiative()
    {
        return GetInitiative() + DiceUtil.D10();
    }

    public override int RollDamge()
    {
        return UnityEngine.Random.Range(MinAttackStrength, MaxAttackStrength + AttackStrengthModifier + 1);
    }

    public override int RollAccuracy()
    {
        return GetAccuracy() + DiceUtil.D6();
    }

    public override int RollEvasion()
    {
        return GetEvasion() + DiceUtil.D4();
    }
}