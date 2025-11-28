using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Inventory/Recipe Database")]
public class RecipeDatabaseSO : ScriptableObject
{
    [Tooltip("List of all individual CraftingRecipeSO assets in the game.")]
    public List<CraftingRecipeSO> allRecipes = new List<CraftingRecipeSO>();
}
