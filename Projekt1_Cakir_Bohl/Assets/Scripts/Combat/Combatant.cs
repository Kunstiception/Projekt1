using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    public string Name;
    public int HealthPoints;
    public int MinAttackStrength;
    public int MaxAttackStrength;
    public int ArmorStrength;
    public int Initiative;
    public int Evasion;

    public int DoDamge()
    {
        return Random.Range(MinAttackStrength, MaxAttackStrength);
    }
}
