using UnityEngine;

public class ChestInventory : MonoBehaviour
{
    public int defaultMaxSlots = 10; // Default max size for this chest
    public InventoryData chestData = new InventoryData();
    
    void Awake()
    {
        chestData.maxSlots = defaultMaxSlots;
        for (int i = 0; i < chestData.maxSlots; i++)
        {
            chestData.slots.Add(new InventorySlot());
        }
    }


}