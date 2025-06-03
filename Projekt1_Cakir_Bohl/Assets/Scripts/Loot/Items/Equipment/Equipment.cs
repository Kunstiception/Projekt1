using System;

[Serializable]
public class Equipment : Item
{
    public enum EquipmentType
    {
        IsRing = 0,
        IsAmulet = 1,
        IsSword = 2
    }

    public EquipmentType equipmentType;
}
