using UnityEngine.EventSystems;
using UnityEngine;

public class UIDraggableItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex;
    public bool isPlayerSlot; 
    [HideInInspector] public InventoryManager inventoryManager;

    // --- Data Access ---
    private InventoryData CurrentInventoryData => inventoryManager.GetInventoryData(isPlayerSlot);
    private InventorySlot CurrentSlot => CurrentInventoryData.slots[slotIndex];

    // --- CLICK LOGIC (Left Click ALL, Right Click HALF/ONE) ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentInventoryData == null) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
        }
        
        inventoryManager.UpdateSlotUI(CurrentSlot);
        inventoryManager.UpdateCursorUI();
    }
    
    void HandleLeftClick()
    {
        // Case 1: Slot is NOT empty, and cursor is EMPTY. (PICK UP ALL)
        if (!CurrentSlot.IsEmpty && InventoryManager.cursorSlot.IsEmpty)
        {
            InventoryManager.cursorSlot.itemData = CurrentSlot.itemData;
            InventoryManager.cursorSlot.quantity = CurrentSlot.quantity;
            inventoryManager.RemoveQuantityFromSlot(CurrentInventoryData, slotIndex, CurrentSlot.quantity); 
        }
        // Case 2: Slot is EMPTY, and cursor is FULL. (PUT DOWN ALL)
        else if (CurrentSlot.IsEmpty && !InventoryManager.cursorSlot.IsEmpty)
        {
            inventoryManager.AddItemToSlot(CurrentInventoryData, slotIndex, InventoryManager.cursorSlot.itemData, InventoryManager.cursorSlot.quantity);
            InventoryManager.cursorSlot.itemData = null;
            InventoryManager.cursorSlot.quantity = 0;
        }
        // Case 3: Slot has the SAME item, and cursor is FULL. (STACK/MERGE)
        else if (!CurrentSlot.IsEmpty && !InventoryManager.cursorSlot.IsEmpty && CurrentSlot.itemData.itemID == InventoryManager.cursorSlot.itemData.itemID)
        {
            int spaceAvailable = CurrentSlot.itemData.maxStack - CurrentSlot.quantity;
            int amountToMove = Mathf.Min(spaceAvailable, InventoryManager.cursorSlot.quantity);
            
            CurrentSlot.quantity += amountToMove;
            InventoryManager.cursorSlot.quantity -= amountToMove;
            
            if (InventoryManager.cursorSlot.quantity <= 0)
            {
                InventoryManager.cursorSlot.itemData = null;
                InventoryManager.cursorSlot.quantity = 0;
            }
        }
        // Case 4: Slot and Cursor both have different items. (SWAP)
        else if (!CurrentSlot.IsEmpty && !InventoryManager.cursorSlot.IsEmpty && CurrentSlot.itemData.itemID != InventoryManager.cursorSlot.itemData.itemID)
        {
             // Swap action logic:
             ItemSO tempItemData = CurrentSlot.itemData;
             int tempQuantity = CurrentSlot.quantity;
                
             // Slot gets cursor item
             inventoryManager.AddItemToSlot(CurrentInventoryData, slotIndex, InventoryManager.cursorSlot.itemData, InventoryManager.cursorSlot.quantity);
                
             // Cursor gets slot item (temp)
             InventoryManager.cursorSlot.itemData = tempItemData;
             InventoryManager.cursorSlot.quantity = tempQuantity;
        }
    }
    
    void HandleRightClick()
    {
        // Case 4: Slot is NOT empty, and cursor is EMPTY. (PICK UP HALF)
        if (!CurrentSlot.IsEmpty && InventoryManager.cursorSlot.IsEmpty)
        {
            int halfAmount = Mathf.CeilToInt(CurrentSlot.quantity / 2f);
            
            InventoryManager.cursorSlot.itemData = CurrentSlot.itemData;
            InventoryManager.cursorSlot.quantity = halfAmount;
            
            inventoryManager.RemoveQuantityFromSlot(CurrentInventoryData, slotIndex, halfAmount);
        }
        // Case 5: Slot is EMPTY, and cursor is FULL. (PUT DOWN ONE)
        else if (CurrentSlot.IsEmpty && !InventoryManager.cursorSlot.IsEmpty)
        {
            inventoryManager.AddItemToSlot(CurrentInventoryData, slotIndex, InventoryManager.cursorSlot.itemData, 1);
            InventoryManager.cursorSlot.quantity -= 1;
            
            if (InventoryManager.cursorSlot.quantity <= 0)
            {
                InventoryManager.cursorSlot.itemData = null;
                InventoryManager.cursorSlot.quantity = 0;
            }
        }
    }

    // --- DRAG LOGIC ---
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentInventoryData == null) return;
        
        // Only start dragging if the slot is not empty AND the cursor is empty
        if (!CurrentSlot.IsEmpty && InventoryManager.cursorSlot.IsEmpty)
        {
            // Move all items from the slot to the cursor (primary drag action)
            InventoryManager.cursorSlot.itemData = CurrentSlot.itemData;
            InventoryManager.cursorSlot.quantity = CurrentSlot.quantity;
            
            // Clear the original slot
            inventoryManager.RemoveQuantityFromSlot(CurrentInventoryData, slotIndex, CurrentSlot.quantity);
            
            inventoryManager.UpdateCursorUI();
            inventoryManager.UpdateSlotUI(CurrentSlot);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Visual dragging is handled by the InventoryManager Update loop
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CurrentInventoryData == null) return;

        // Check if the drag ended over *another* UI element (another slot)
        if (eventData.pointerEnter != null)
        {
            UIDraggableItem targetSlotHandler = eventData.pointerEnter.GetComponentInParent<UIDraggableItem>();

            // If we ended the drag on a valid slot
            if (targetSlotHandler != null && !InventoryManager.cursorSlot.IsEmpty)
            {
                InventoryData targetData = targetSlotHandler.CurrentInventoryData;
                InventorySlot targetSlot = targetSlotHandler.CurrentSlot;
                
                // Temporarily store the item that was in the target slot
                ItemSO tempItemData = targetSlot.itemData;
                int tempQuantity = targetSlot.quantity;
                
                // 1. Place the cursor item into the target slot
                inventoryManager.AddItemToSlot(targetData, targetSlotHandler.slotIndex, InventoryManager.cursorSlot.itemData, InventoryManager.cursorSlot.quantity);
                
                // 2. Move the item that was in the target slot onto the cursor
                InventoryManager.cursorSlot.itemData = tempItemData;
                InventoryManager.cursorSlot.quantity = tempQuantity;

                // 3. Update all affected UI elements
                inventoryManager.UpdateSlotUI(targetSlot);
            }
        }
        else
        {
            // If dragging outside the UI, the item is dropped to the world
            inventoryManager.DropAllHeldItemsToWorld();
        }
        
        inventoryManager.UpdateSlotUI(CurrentSlot); 
        inventoryManager.UpdateCursorUI();
    }
}
