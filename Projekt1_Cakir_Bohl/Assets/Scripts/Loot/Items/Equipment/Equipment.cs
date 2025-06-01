using System;

[Serializable]
public class Equipment : Item
{
    public enum EquipmentType
    {
        isRing = 0,
        isAmulet = 1,
        isSword = 2
    }

    public EquipmentType equipmentType;
}
