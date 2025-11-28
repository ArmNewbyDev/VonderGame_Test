using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    [Tooltip("List of all individual ItemSO assets in the game.")]
    public List<ItemSO> allItems = new List<ItemSO>();
}