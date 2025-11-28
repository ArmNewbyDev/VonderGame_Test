using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots;
}
