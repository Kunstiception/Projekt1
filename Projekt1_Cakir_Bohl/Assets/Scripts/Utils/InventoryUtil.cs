public static class InventoryUtil
{
    public static int ReturnItemAmount(Item item)
    {
        return InventoryManager.Instance.InventoryAmounts[InventoryManager.Instance.InventoryItems.IndexOf(item)];
    }

    public static bool CheckIfEquipable(int currentIndex)
    {
        if (currentIndex > InventoryManager.Instance.InventoryItems.Count)
        {
            return false;
        }

        return !InventoryManager.Instance.EquippedItems[currentIndex];
    }
}
