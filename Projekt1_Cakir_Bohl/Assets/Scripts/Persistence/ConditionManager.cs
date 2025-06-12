using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    public enum Conditions
    {
        SleepDeprived = 0,
        Vampire = 1,
        Werewolf = 2,
        Zombie = 3
    }
    
    public static ConditionManager Instance;

    public bool IsSleepDeprived = false;
    public bool IsVampire = false;
    public bool IsWerewolf = false;
    public bool IsZombie = false;

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

    public string[] ApplyCondition(Conditions condition, bool isAffected)
    {
        switch(condition)
        {
            case Conditions.SleepDeprived:
                if(isAffected)
                {
                    ApplySleepDeprived(true);

                    return UIDialogueStorage.SleepDeprivedLines;
                }
                else
                {
                    ApplySleepDeprived(false);

                    return UIDialogueStorage.HealedSleepDeprivedLines;
                }
            
            case Conditions.Vampire:
                if(isAffected)
                {
                    //ApplyVampire(true);

                    return UIDialogueStorage.VampireLines;
                }
                else
                {
                    //ApplyVampire(false);

                    return UIDialogueStorage.HealedVampireLines;
                }

            case Conditions.Werewolf:
                if(isAffected)
                {
                    ApplyWerewolf(true);

                    return UIDialogueStorage.WerewolfLines;
                }
                else
                {
                    ApplyWerewolf(false);

                    return UIDialogueStorage.HealedWerewolfLines;
                }

            case Conditions.Zombie:
                if(isAffected)
                {
                    IsZombie = true;

                    return UIDialogueStorage.ZombieLines;
                }
                else
                {   
                    IsZombie = false;
                    return UIDialogueStorage.HealedZombieLines;
                }
        }

        return null;
    }

    public List<Conditions> GetCurrentConditions()
    {
        List<Conditions> conditions = new List<Conditions>();
        
        if(IsSleepDeprived)
        {
            conditions.Add(Conditions.SleepDeprived);
        }
        
        if(IsVampire)
        {
            conditions.Add(Conditions.Vampire);
        }

        if(IsWerewolf)
        {
            conditions.Add(Conditions.Werewolf);
        }

        if(IsZombie)
        {
            conditions.Add(Conditions.Zombie);
        }

        return conditions;
    }

    private void ApplySleepDeprived(bool isAffected)
    {
        if(isAffected)
        {
            PlayerManager.Instance.AccuracyModifier -= GameConfig.SleepDeprivedAccuracyModifier;
            PlayerManager.Instance.EvasionModifier -= GameConfig.SleepDeprivedEvasionModifier;
            PlayerManager.Instance.InitiativeModifier -= GameConfig.SleepDeprivedInitiativeModifier;

            IsSleepDeprived = true;
        }
        else
        {
            PlayerManager.Instance.AccuracyModifier += GameConfig.SleepDeprivedAccuracyModifier;
            PlayerManager.Instance.EvasionModifier += GameConfig.SleepDeprivedEvasionModifier;
            PlayerManager.Instance.InitiativeModifier += GameConfig.SleepDeprivedInitiativeModifier;

            IsSleepDeprived = false;
        }
    }

    public void ApplyVampire(bool isAffected)
    {
        if(isAffected)
        {
            PlayerManager.Instance.EvasionModifier += GameConfig.VampireEvasionModifier;
            PlayerManager.Instance.InsultDamageModifier += GameConfig.VampireInsultDamageModifier;

            IsVampire = true;
        }
        else
        {
            PlayerManager.Instance.EvasionModifier -= GameConfig.VampireEvasionModifier;
            PlayerManager.Instance.InsultDamageModifier -= GameConfig.VampireInsultDamageModifier;

            IsVampire = false;
        }
    }

    private void ApplyWerewolf(bool isAffected)
    {
        if(isAffected)
        {
            ToggleWerewolfStats(true);

            IsWerewolf = true;
        }
        else
        {
            ToggleWerewolfStats(false);

            IsWerewolf = false;
        }
    }

    public void ToggleWerewolfStats(bool isActive)
    {
        if(isActive)
        {
            PlayerManager.Instance.AttackStrengthModifier += GameConfig.WerewolfAttackStrengthModifier;
            PlayerManager.Instance.InsultDamageModifier -= GameConfig.WerewolfInsultDamageModifier;
        }
        else
        {
            PlayerManager.Instance.AttackStrengthModifier -= GameConfig.WerewolfAttackStrengthModifier;
            PlayerManager.Instance.InsultDamageModifier += GameConfig.WerewolfInsultDamageModifier;
        }
    }

    public void InitializeConditions()
    {
        IsSleepDeprived = MainManager.Instance.IsSleepDeprived;
        IsVampire = MainManager.Instance.IsVampire;
        IsWerewolf = MainManager.Instance.IsWerewolf;
        IsZombie = MainManager.Instance.IsZombie;

        // if(IsSleepDeprived)
        // {
        //     ApplySleepDeprived(true);
        // }

        // if(IsVampire)
        // {
        //     ApplyVampire(true);
        // }

        // if(IsWerewolf)
        // {
        //     ApplyWerewolf(true);
        // }
    }
}
