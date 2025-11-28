using UnityEngine;

public class CraftingButton_Test : MonoBehaviour
{
    // Assign the specific Recipe asset (e.g., SwordRecipe.asset) in the Inspector.
    [Header("Recipe to be crafted")]
    public CraftingRecipeSO recipeToCraft;
    
    // A helper button that calls this script. (Optional for actual Unity UI setup)
    public void OnCraftButtonClicked()
    {
        if (CraftingManager.Instance == null)
        {
            Debug.LogError("CraftingManager is not initialized or found in the scene.");
            return;
        }

        if (recipeToCraft == null)
        {
            Debug.LogError("No recipeToCraft asset assigned to this button script.");
            return;
        }

        // --- CORE LOGIC: Check and Execute ---
        
        bool success = CraftingManager.Instance.TryCraftRecipe(recipeToCraft);

        if (success)
        {
            Debug.Log($"Successfully crafted {recipeToCraft.outputItem.itemName}!");
        }
        else
        {
            // The InventoryManager handles logging the reason (e.g., "Missing Wood").
            Debug.LogWarning("Crafting attempt failed. Check console for details.");
        }
    }
}
