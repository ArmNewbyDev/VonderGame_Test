using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public int itemID;
    public string itemName;
    public string description; 
    public Sprite icon;
    public int maxStack = 99; // Default max stack size, 1 means non-stackable / No 0, 0 is bug
    public int sortOrder = 0; 


    public ItemSO(int id, string name, string desc, Sprite icon, int maxStack, int sortOrder)
    {
        this.itemID = id;
        this.itemName = name;
        this.description = desc;
        this.icon = icon;
        this.maxStack = maxStack;
        this.sortOrder = sortOrder;
    }

}
