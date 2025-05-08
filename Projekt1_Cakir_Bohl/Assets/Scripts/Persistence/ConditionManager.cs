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

                    return DialogueManager.SleepDeprivedLines;
                }
                else
                {
                    ApplySleepDeprived(false);

                    return DialogueManager.HealedSleepDeprivedLines;
                }
            
            case Conditions.Vampire:
                if(isAffected)
                {
                    ApplyVampire(true);

                    return DialogueManager.VampireLines;
                }
                else
                {
                    ApplyVampire(false);

                    return DialogueManager.HealedVampireLines;
                }

            case Conditions.Werewolf:
                //insert
                return null;

            case Conditions.Zombie:
                //insert
                return null;
        }

        return null;
    }

    public void EvaluateCurrentConditions()
    {
        if(IsVampire)
        {

        }
    }

    private void ApplySleepDeprived(bool isAffected)
    {
        if(isAffected)
        {
            PlayerManager.Instance.AccuracyModifier -= 1;
            PlayerManager.Instance.EvasionModifier -= 1;
            PlayerManager.Instance.InitiativeModifier -= 1;

            IsSleepDeprived = true;
        }
        else
        {
            PlayerManager.Instance.AccuracyModifier += 1;
            PlayerManager.Instance.EvasionModifier += 1;
            PlayerManager.Instance.InitiativeModifier += 1;

            IsSleepDeprived = false;
        }
    }

    private void ApplyVampire(bool isAffected)
    {
        if(isAffected)
        {
            PlayerManager.Instance.EvasionModifier += 2;
            PlayerManager.Instance.InsultDamageModifier +=2;

            IsVampire = true;
        }
        else
        {
            PlayerManager.Instance.EvasionModifier -= 2;
            PlayerManager.Instance.InsultDamageModifier -=2;

            IsVampire = false;
        }
    }
}
