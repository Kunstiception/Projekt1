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
                    PlayerManager.Instance.AccuracyModifier -= 1;
                    PlayerManager.Instance.EvasionModifier -= 1;
                    PlayerManager.Instance.InitiativeModifier -= 1;

                    IsSleepDeprived = true;

                    return DialogueManager.SleepDeprivedLines;
                }
                else
                {
                    PlayerManager.Instance.AccuracyModifier += 1;
                    PlayerManager.Instance.EvasionModifier += 1;
                    PlayerManager.Instance.InitiativeModifier += 1;

                    IsSleepDeprived = false;

                    return DialogueManager.HealedSleepDeprivedLines;
                }
            
            case Conditions.Vampire:
                //insert
                return null;

            case Conditions.Werewolf:
                //insert
                return null;
        }

        return null;
    }
}
