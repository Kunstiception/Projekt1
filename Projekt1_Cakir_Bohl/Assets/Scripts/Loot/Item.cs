using System.Collections.Generic;
using UnityEngine;

public class Item: MonoBehaviour
{
    public enum ItemTypes
    {
        isUsable = 0,
        isEquipment = 1,
        isCurrency = 2
    }

    public ItemTypes ItemType;
    
    public string Name;
    public string Description;
    public int MinimumAmountOnLoot;
    public int MaximumAmountOnLoot;
    public List<string> _lines;
}
