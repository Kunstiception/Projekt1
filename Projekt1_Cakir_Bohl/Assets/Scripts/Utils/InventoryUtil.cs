using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryUtil
{
    public static int ReturnItemAmount(Item item)
    {
        return InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(item)];
    }
}
