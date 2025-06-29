using UnityEngine;

public class Combatant : MonoBehaviour
{
    [SerializeField] public InsultLines InsultLines;
    public string Name;
    public int HealthPoints;
    public int EgoPoints;
    public int MinAttackStrength;
    public int MaxAttackStrength;
    public int ArmorStrength;
    public int Initiative;
    public int Accuracy;
    public int Evasion;
    public int InsultResistance;

    public virtual int RollInitiative()
    {
        return Initiative + DiceUtil.D10();
    }

    public virtual int RollDamge()
    {
        return Random.Range(MinAttackStrength, MaxAttackStrength + 1);
    }

    public virtual int RollAccuracy()
    {
        return Accuracy + DiceUtil.D6();
    }

    public virtual int RollEvasion()
    {
        return Evasion + DiceUtil.D4();
    }
}
