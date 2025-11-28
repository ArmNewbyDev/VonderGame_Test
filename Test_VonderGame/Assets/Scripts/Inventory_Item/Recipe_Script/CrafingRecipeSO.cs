using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public int recipeID; 
    public ItemSO outputItem;
    public int outputQuantity = 1;
    public int sortOrder = 0; 
    public List<RecipeIngredient> requiredIngredients = new List<RecipeIngredient>();
}
