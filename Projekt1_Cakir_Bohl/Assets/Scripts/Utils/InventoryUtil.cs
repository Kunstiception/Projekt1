using System;
using System.Linq;
using Unity.VisualScripting;

public static class InventoryUtil
{
    public static int ReturnItemAmount(Item givenItem)
    {
        if (!InventoryManager.Instance.InventoryItems.Contains(givenItem))
        {
            return 0;
        }

        return InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(givenItem)];
    }

    public static bool CheckIfEquipable(int currentIndex)
    {
        if (currentIndex > InventoryManager.Instance.InventoryItems.Count)
        {
            return false;
        }

        return !InventoryManager.Instance.EquippedItems[currentIndex];
    }

    public static Item ReturnCoinItem()
    {
        // https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable.firstordefault?view=net-8.0
        var coin = InventoryManager.Instance.InventoryItems.FirstOrDefault(item => item is Coin);

        if (coin == null)
        {
            return null;
        }

        return coin;
    }
}
