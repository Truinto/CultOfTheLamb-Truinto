
namespace FireDevil
{
    public static class Helpers
    {
        public static int GetItemQuantity(this StructureBrain structure, InventoryItem.ITEM_TYPE type)
        {
            int num = 0;
            foreach (var item in structure.Data.Inventory)
                if (item.type == (int)type)
                    num += item.quantity;
            return num;
        }
    }
}