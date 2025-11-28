using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_GetWood : MonoBehaviour, IInteraction
{
    public int woodAmount = 1;
    public ItemSO woodItem;

    public void Interact()
    {
        InventoryManager.Instance.AddItemToInventory(woodItem, woodAmount);
        Debug.Log("Collected " + woodAmount + " wood.");
    }
}
