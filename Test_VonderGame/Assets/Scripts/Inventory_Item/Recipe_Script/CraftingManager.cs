using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }
    
    [Header("Recipe Database")]
    public RecipeDatabaseSO recipeDatabase;

    private InventoryManager inventoryManager;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Called by InventoryManager to link the two systems after initialization.
    /// </summary>
    public void Initialize(InventoryManager manager)
    {
        inventoryManager = manager;
        Debug.Log("CraftingManager initialized and linked to InventoryManager.");

        if (recipeDatabase == null)
        {
            Debug.LogError("RecipeDatabaseSO asset is not assigned to the CraftingManager!");
        }
    }
    
    // --- Public Crafting API ---

    /// <summary>
    /// Attempts to craft a specific recipe. This is the main function called by a UI button.
    /// </summary>
    /// <param name="recipe">The CraftingRecipeSO to attempt to create.</param>
    /// <returns>True if the craft was successful, False otherwise.</returns>
    public bool TryCraftRecipe(CraftingRecipeSO recipe)
    {
        if (recipe == null || inventoryManager == null)
        {
            Debug.LogError("Crafting failed: Recipe or Inventory Manager is missing.");
            return false;
        }

        // 1. Check if the player has enough ingredients (InventoryManager handles the check)
        // Note: The InventoryManager.TryConsumeItemsFromInventory function already contains the full check logic.
        
        // 2. Perform the Craft (Consume items and Add output)
        if (inventoryManager.TryConsumeItemsFromInventory(recipe.requiredIngredients))
        {
            // Ingredients successfully consumed, now add the output item(s)
            inventoryManager.AddItemToInventory(recipe.outputItem, recipe.outputQuantity);
            Debug.Log($"Crafting Successful! Created {recipe.outputQuantity} x {recipe.outputItem.itemName}.");
            
            // OPTIONAL: Since we consumed items, the inventory might look messy. Sort it.
            inventoryManager.SortInventory(inventoryManager.playerInventoryData);

            return true;
        }
        else
        {
            // The InventoryManager already logged the failure reason.
            return false; 
        }
    }
    
    /// <summary>
    /// Helper to find a recipe by ID (useful for a simple crafting UI)
    /// </summary>
    public CraftingRecipeSO GetRecipeByID(int recipeID)
    {
        if (recipeDatabase == null) return null;
        return recipeDatabase.allRecipes.FirstOrDefault(r => r.recipeID == recipeID);
    }

    /// <summary>
    /// Finds all recipes the player CAN currently craft.
    /// </summary>
    public List<CraftingRecipeSO> FindCraftableRecipes()
    {
        if (recipeDatabase == null || inventoryManager == null) return new List<CraftingRecipeSO>();
        
        List<CraftingRecipeSO> craftable = new List<CraftingRecipeSO>();

        foreach (var recipe in recipeDatabase.allRecipes)
        {
            if (CanCraft(recipe))
            {
                craftable.Add(recipe);
            }
        }
        
        return craftable;
    }
    
    /// <summary>
    /// Detailed check if a single recipe can be crafted without consuming items.
    /// </summary>
    public bool CanCraft(CraftingRecipeSO recipe)
    {
        if (recipe == null) return false;

        foreach (var ingredient in recipe.requiredIngredients)
        {
            int totalNeeded = ingredient.quantity;
            int totalFound = 0;
            
            // Calculate how much of the ingredient is in the player's inventory
            foreach (var slot in inventoryManager.playerInventoryData.slots)
            {
                if (!slot.IsEmpty && slot.itemData.itemID == ingredient.item.itemID)
                {
                    totalFound += slot.quantity;
                }
            }
            
            if (totalFound < totalNeeded)
            {
                return false; // Cannot craft if any ingredient is missing
            }
        }
        return true;
    }
}