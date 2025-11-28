using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Item Database")]
    public ItemDatabaseSO itemDatabase; 

    // --- Item Library (Runtime Dictionary for quick lookup by ID) ---
    private Dictionary<int, ItemSO> itemLibrary = new Dictionary<int, ItemSO>();

    // Inventory Data Instances
    [Header("Inventory Data")]
    public InventoryData playerInventoryData = new InventoryData { maxSlots = 20 };
    [HideInInspector] public InventoryData activeContainerData = null; 
    
    [Header("UI References")]
    public GameObject slotPrefab;
    public Transform mainInventoryUI;
    public Transform playerPanel; 
    public Transform containerPanel; 
    public int maxContainerUISlots = 15; 
    
    private List<InventorySlot> containerUISlots = new List<InventorySlot>();

    // --- UI State ---
    private bool isUIOpen = false;

    // ----------------- CURSOR/DRAG STATE -----------------
    public static InventorySlot cursorSlot = new InventorySlot();
    public Image cursorItemImage; 
    public TextMeshProUGUI cursorQuantityText; 

    // --- Unity Methods ---

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        // Initialize the item library from the database asset
        InitializeItemLibrary();
    }

    void Start()
    {
        // FIX: Ensure the Cursor UI element ignores all raycasts so the pointer can click/drop on slots beneath it.
        if (cursorItemImage != null)
        {
            cursorItemImage.raycastTarget = false;
        }
        if (cursorQuantityText != null)
        {
            // TextMeshProUGUI has a 'raycastTarget' property inherited from Graphic.
            cursorQuantityText.raycastTarget = false;
        }

        InitializeInventoryUI();
        UpdateCursorUI();

        // Ensure UI is hidden at start
        mainInventoryUI.gameObject.SetActive(false);
        
        // Initialize the CraftingManager dependency (Ensure CraftingManager exists in the scene!)
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.Initialize(this);
        }
        else
        {
            Debug.LogError("CraftingManager not found in the scene. Please ensure it is attached to a GameObject.");
        }
    }

    void Update()
    {
        cursorItemImage.transform.position = Input.mousePosition;
        
        // Input check for toggling inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            HandleInventoryToggle(!isUIOpen);
        }
        
        // Handle dropping item outside of UI bounds (Only check if UI is open)
        if (isUIOpen && cursorSlot.quantity > 0 && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                DropAllHeldItemsToWorld();
            }
        }
    }
    
    // --- Item Library Helper ---
    
    void InitializeItemLibrary()
    {
        itemLibrary.Clear();
        
        if (itemDatabase != null && itemDatabase.allItems.Count > 0)
        {
            Debug.Log("Loading items from ItemDatabaseSO asset.");
            foreach (ItemSO item in itemDatabase.allItems)
            {
                if (item != null && !itemLibrary.ContainsKey(item.itemID))
                {
                    itemLibrary.Add(item.itemID, item);
                }
            }
        }
        else
        {
            Debug.LogError("Item Database asset not assigned or empty. Inventory will not function without ItemSO assets loaded.");
        }
    }

    public ItemSO GetItemSO(int id)
    {
        if (itemLibrary.ContainsKey(id))
            return itemLibrary[id];
        return null;
    }

    // --- UI Toggle and Container Management ---
    
    public void HandleInventoryToggle(bool state)
    {
        isUIOpen = state;
        mainInventoryUI.gameObject.SetActive(state);

        if (state == false)
        {
            // IMPORTANT: If closing, check if the player is holding an item.
            if (cursorSlot.quantity > 0)
            {
                DropAllHeldItemsToWorld();
            }
            // Ensure the container is closed if it was open
            CloseContainer();
        }
    }

    // Called when the player interacts with a chest
    public void OpenContainer(ChestInventory chest)
    {
        if (chest == null) return;
        
        activeContainerData = chest.chestData;
        containerPanel.gameObject.SetActive(true);
        
        LinkUIToContainerData(activeContainerData);

        // Ensure the main UI is visible
        HandleInventoryToggle(true);
    }
    
    // Called to hide the chest interface
    public void CloseContainer()
    {
        containerPanel.gameObject.SetActive(false);
        activeContainerData = null;
        
        // Ensure unused UI slots are hidden and references cleared
        for (int i = 0; i < containerUISlots.Count; i++)
        {
            containerUISlots[i].itemIcon.transform.parent.gameObject.SetActive(false);
        }
    }

    // --- Data Access Helper ---

    public InventoryData GetInventoryData(bool isPlayerSlot)
    {
        if (!isPlayerSlot && activeContainerData == null)
        {
            Debug.LogError("Attempted to access container data when none is open.");
            return null; 
        }
        // FIX: Replaced 'activeInventoryData' (a typo) with 'activeContainerData'
        return isPlayerSlot ? playerInventoryData : activeContainerData; 
    }

    // --- Initialization ---

    void InitializeInventoryUI()
    {
        // 1. Initialize Player Inventory Data
        for (int i = 0; i < playerInventoryData.maxSlots; i++)
        {
            playerInventoryData.slots.Add(new InventorySlot());
        }
        
        // 2. Initialize and build Player UI (always visible)
        InitializePanel(playerPanel, playerInventoryData, true, playerInventoryData.maxSlots);
        
        // 3. Initialize Container UI structure (builds the max possible slots)
        InitializeContainerPanel(containerPanel, maxContainerUISlots); 
    }
    
    // Helper to initialize Player or Fixed Panels
    void InitializePanel(Transform panel, InventoryData data, bool isPlayer, int slotCount)
    {
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, panel);
            
            Image icon = slotGO.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI quantity = slotGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            
            data.slots[i].itemIcon = icon;
            data.slots[i].quantityText = quantity;

            UIDraggableItem dragHandler = slotGO.AddComponent<UIDraggableItem>();
            dragHandler.slotIndex = i;
            dragHandler.inventoryManager = this;
            dragHandler.isPlayerSlot = isPlayer; 
            
            UpdateSlotUI(data.slots[i]);
        }
    }
    
    // Helper to initialize the reusable Container UI Panel
    void InitializeContainerPanel(Transform panel, int maxSlots)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, panel);
            
            // Store references for later use (these references point to the UI elements)
            InventorySlot uiSlot = new InventorySlot
            {
                itemIcon = slotGO.transform.GetChild(0).GetComponent<Image>(),
                quantityText = slotGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>()
            };
            containerUISlots.Add(uiSlot);
            
            UIDraggableItem dragHandler = slotGO.AddComponent<UIDraggableItem>();
            dragHandler.slotIndex = i;
            dragHandler.inventoryManager = this;
            dragHandler.isPlayerSlot = false; 
            
            uiSlot.itemIcon.transform.parent.gameObject.SetActive(false);
        }
    }
    
    // Links the active data slots to the container UI slots and updates visuals
    void LinkUIToContainerData(InventoryData data)
    {
        for (int i = 0; i < maxContainerUISlots; i++)
        {
            GameObject slotGO = containerUISlots[i].itemIcon.transform.parent.gameObject;
            
            if (i < data.maxSlots)
            {
                // Link data slots to UI references
                data.slots[i].itemIcon = containerUISlots[i].itemIcon;
                data.slots[i].quantityText = containerUISlots[i].quantityText;
                
                // Show the slot and update its content
                slotGO.SetActive(true);
                UpdateSlotUI(data.slots[i]);
            }
            else
            {
                // Hide any excess UI slots not used by this container
                slotGO.SetActive(false);
            }
        }
    }

    // --- UI Update Methods ---

    public void UpdateSlotUI(InventorySlot slot)
    {
        if (slot.itemIcon == null || slot.quantityText == null) return;

        if (slot.IsEmpty || slot.itemData == null)
        {
            slot.itemIcon.enabled = false;
            slot.quantityText.enabled = false;
        }
        else
        {
            slot.itemIcon.enabled = true;
            slot.itemIcon.sprite = slot.itemData.icon;
            
            bool showQuantity = slot.itemData.maxStack > 1; 
            slot.quantityText.enabled = showQuantity;
            if(showQuantity)
                slot.quantityText.text = slot.quantity.ToString();
        }
    }

    public void UpdateCursorUI()
    {
        if (cursorSlot.IsEmpty || cursorSlot.itemData == null)
        {
            cursorItemImage.enabled = false;
            cursorQuantityText.enabled = false;
        }
        else
        {
            cursorItemImage.enabled = true;
            cursorItemImage.sprite = cursorSlot.itemData.icon;
            cursorQuantityText.enabled = true;
            cursorQuantityText.text = cursorSlot.quantity.ToString();
        }
    }

    // --- Item Manipulation Methods (General Utility) ---

    public void AddItemToSlot(InventoryData data, int index, ItemSO item, int quantity)
    {
        if (index >= data.slots.Count) return; 

        data.slots[index].itemData = item;
        data.slots[index].quantity = quantity;
        UpdateSlotUI(data.slots[index]);
    }
    
    public void RemoveQuantityFromSlot(InventoryData data, int index, int amount)
    {
        if (index >= data.slots.Count) return; 

        data.slots[index].quantity -= amount;
        
        if (data.slots[index].quantity <= 0)
        {
            data.slots[index].quantity = 0;
            data.slots[index].itemData = null;
        }
        
        UpdateSlotUI(data.slots[index]);
    }

    // Public method for adding items to the inventory from external scripts.
    public void AddItemToInventory(ItemSO item, int quantityToAdd)
    {
        if (item == null || quantityToAdd <= 0) return;

        int remaining = quantityToAdd;

        // Attempt 1: Stackable items - Fill existing stacks first
        if (item.maxStack > 1)
        {
            // Find existing partial stacks of the same item
            var partialStacks = playerInventoryData.slots
                .Where(s => !s.IsEmpty && s.itemData != null && s.itemData.itemID == item.itemID && s.quantity < item.maxStack)
                .ToList();

            foreach (var slot in partialStacks)
            {
                if (remaining <= 0) break;
                
                int space = item.maxStack - slot.quantity;
                int amountToMove = Mathf.Min(space, remaining);

                slot.quantity += amountToMove;
                remaining -= amountToMove;
                UpdateSlotUI(slot);
            }
        }
        

        // Attempt 2: Use empty slots for remaining quantity (if not stackable or stacks are full)
        while (remaining > 0)
        {
            int amountToPutInSlot = Mathf.Min(remaining, item.maxStack);
            
            // Find the first empty slot
            var emptySlotIndex = playerInventoryData.slots.FindIndex(s => s.IsEmpty);

            if (emptySlotIndex != -1)
            {
                // Put a stack into the empty slot
                AddItemToSlot(playerInventoryData, emptySlotIndex, item, amountToPutInSlot);
                remaining -= amountToPutInSlot;
            }
            else
            {
                // Inventory is full, drop the rest (real world)
                Debug.LogWarning($"Inventory full. Dropping {remaining} x {item.itemName} to the world.");
                // In a real game, you would instantiate a WorldItem for 'remaining' quantity here.
                remaining = 0; 
                break;
            }
        }
    }
    
    /// <summary>
    /// Checks if the player has the required ingredients and, if so, consumes them.
    /// Used by the CraftingManager.
    /// </summary>
    /// <param name="ingredients">List of items and quantities to consume.</param>
    /// <returns>True if items were consumed, False otherwise (due to missing ingredients).</returns>
    public bool TryConsumeItemsFromInventory(List<RecipeIngredient> ingredients)
    {
        var inventory = playerInventoryData.slots;
        
        // 1. CHECK PASS: Verify that all required items exist in the necessary quantities.
        foreach (var ingredient in ingredients)
        {
            // Safety check for ingredient item data
            if (ingredient.item == null)
            {
                Debug.LogError("Crafting failed: One or more recipe ingredients are null (unassigned ItemSO).");
                return false;
            }

            int totalNeeded = ingredient.quantity;
            int totalFound = 0;
            
            // Count total quantity of this ingredient across all slots
            foreach (var slot in inventory)
            {
                if (!slot.IsEmpty && slot.itemData != null && slot.itemData.itemID == ingredient.item.itemID)
                {
                    totalFound += slot.quantity;
                }
            }
            
            if (totalFound < totalNeeded)
            {
                Debug.LogWarning($"Crafting failed: Missing {totalNeeded - totalFound} x {ingredient.item.itemName}.");
                return false; // Fail fast if any ingredient is missing
            }
        }
        
        // 2. CONSUME PASS: If the check passed, safely consume the items.
        foreach (var ingredient in ingredients)
        {
            
            int remainingToConsume = ingredient.quantity;
            
            // Iterate through slots and consume until the needed amount is gone
            for (int i = 0; i < inventory.Count && remainingToConsume > 0; i++)
            {
                var slot = inventory[i];
                
                if (!slot.IsEmpty && slot.itemData != null && slot.itemData.itemID == ingredient.item.itemID)
                {
                    int amountToTake = Mathf.Min(remainingToConsume, slot.quantity);
                    RemoveQuantityFromSlot(playerInventoryData, i, amountToTake);
                    remainingToConsume -= amountToTake;
                }
            }
        }
        
        // 3. Update UI after consumption
        // This is necessary because RemoveQuantityFromSlot only updates the individual slots it touches.
        // We iterate over the whole inventory to ensure a full refresh.
        foreach (var slot in inventory)
        {
            UpdateSlotUI(slot);
        }
        
        return true;
    }

    // --- Sorting (Updated to consolidate and then sort) ---

    public void SortInventory(InventoryData data = null)
    {
        if (data == null || data.slots.Count == 0) data = playerInventoryData;

        // 1. Group all items and accumulate total quantity for each ItemSO type
        Dictionary<ItemSO, int> itemTotals = new Dictionary<ItemSO, int>();

        foreach (var slot in data.slots)
        {
            if (!slot.IsEmpty && slot.itemData != null)
            {
                if (itemTotals.ContainsKey(slot.itemData))
                {
                    itemTotals[slot.itemData] += slot.quantity;
                }
                else
                {
                    itemTotals.Add(slot.itemData, slot.quantity);
                }
            }
        }
        
        // --- Sorting the Items Before Re-Insertion ---
        // Convert the dictionary to a list of KeyValuePairs for sorting: 
        // Primary sort: by sortOrder (defined on ItemSO)
        // Secondary sort: by itemName (alphabetical)
        var sortedItems = itemTotals
            .OrderBy(kv => kv.Key.sortOrder)
            .ThenBy(kv => kv.Key.itemName)
            .ToList();

        // Preserve UI references and clear the old list
        List<Image> oldIcons = data.slots.Select(s => s.itemIcon).ToList();
        List<TextMeshProUGUI> oldTexts = data.slots.Select(s => s.quantityText).ToList();
        
        // Clear the data and prepare for reconstruction
        data.slots.Clear();
        int currentSlotIndex = 0;

        // 2. Re-insert items, consolidated into full stacks first
        foreach (var pair in sortedItems)
        {
            ItemSO item = pair.Key;
            int totalQuantity = pair.Value;
            int maxStack = item.maxStack;

            // CRITICAL LOOP: Ensures totalQuantity always decreases and currentSlotIndex always increases
            // to prevent an infinite loop (which would crash the Editor).
            while (totalQuantity > 0 && currentSlotIndex < oldIcons.Count)
            {
                // Calculate the quantity for the current stack (maxStack or remaining total)
                int quantityToPlace = Mathf.Min(totalQuantity, maxStack);

                // Create a new slot and assign the UI references
                InventorySlot newSlot = new InventorySlot
                {
                    itemData = item,
                    quantity = quantityToPlace,
                    itemIcon = oldIcons[currentSlotIndex],
                    quantityText = oldTexts[currentSlotIndex]
                };

                data.slots.Add(newSlot);
                UpdateSlotUI(newSlot); // Update the UI immediately
                
                totalQuantity -= quantityToPlace;
                currentSlotIndex++;
            }
            
            // Note: If totalQuantity > 0 here, the inventory is full. The remaining quantity is functionally 'lost' 
            // in this simple manager, but in a real game, it would be dropped to the world.
        }
        
        // 3. Fill the remaining slots with empty InventorySlot objects
        while (currentSlotIndex < oldIcons.Count)
        {
            InventorySlot emptySlot = new InventorySlot
            {
                itemIcon = oldIcons[currentSlotIndex],
                quantityText = oldTexts[currentSlotIndex]
            };
            data.slots.Add(emptySlot);
            UpdateSlotUI(emptySlot); // Update to show empty
            currentSlotIndex++;
        }

        Debug.Log($"Inventory consolidated and sorted. (Is Player: {data == playerInventoryData})");
    }
    
    // --- Game World Interaction ---

    public void DropAllHeldItemsToWorld()
    {
        if (cursorSlot.quantity > 0)
        {
            // TODO: Replace this with actual game logic to instantiate a world item
            Debug.Log($"[GAME WORLD] Dropped {cursorSlot.quantity} x {cursorSlot.itemData.itemName} at player location.");

            // Clear the cursor slot state
            cursorSlot.itemData = null;
            cursorSlot.quantity = 0;
            UpdateCursorUI();
        }
    }
}