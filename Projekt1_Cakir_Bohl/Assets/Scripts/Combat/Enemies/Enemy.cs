using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Scriptable Object/Enemy")]
public class Enemy : ScriptableObject
{
    public string Name = "Boar";
    public int HealthPoints = 7;
    public int MinAttackStrength = 2;
    public int MaxAttackStrength = 5;
    public int Initiative = 8;
}
