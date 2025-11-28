using UnityEngine.UI;
using System;
using TMPro;

[Serializable]
public class InventorySlot
{
    public ItemSO itemData = null;
    public int quantity = 0;
    
    [NonSerialized] public Image itemIcon; 
    [NonSerialized] public TextMeshProUGUI quantityText;
    public bool IsEmpty => quantity == 0;
}

