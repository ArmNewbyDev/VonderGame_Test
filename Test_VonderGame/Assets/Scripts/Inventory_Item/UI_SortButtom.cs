using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SortButtom : MonoBehaviour
{
    public void SortInventoryPlayer()
    {
        InventoryManager.Instance.SortInventory();
    }

}
