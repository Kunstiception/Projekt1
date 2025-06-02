using System.Collections.Generic;
using System.Linq;

public static class InventoryUtil
{
    public static int ReturnItemAmount(Item item)
    {
        return InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(item)];
    }

    public static bool CheckIfEquipable(Item item)
    {
        if (item is not Equipment)
        {
            return false;
        }

        Equipment equipment = (Equipment)item;

        if (!InventoryManager.Instance.CurrentEquipment.Contains(equipment) &&
                equipment.equipmentType != Equipment.EquipmentType.isRing)
        {
            return true;
        }

        //https://stackoverflow.com/questions/41934402/lambda-where-expression
        List<Item> rings = InventoryManager.Instance.InventoryItems.Where(ring => ring == equipment).ToList();

        if (rings.Count > 1)
        {
            return true;
        }

        return false;
    }
}
