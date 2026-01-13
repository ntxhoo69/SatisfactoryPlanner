using System.Collections.Generic;

namespace SatisfactoryPlanner.Models;

/// <summary>
/// Represents a crafting recipe that can be used in a building.
/// Defines input items required and output items produced.
/// </summary>
public class Recipe
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string BuildingId { get; set; } // Which building(s) can use this recipe
    public List<RecipeItem> Inputs { get; set; }
    public List<RecipeItem> Outputs { get; set; }
    public double CraftingTimeSeconds { get; set; } // Time to produce one batch
    
    public Recipe()
    {
        Id = string.Empty;
        Name = string.Empty;
        BuildingId = string.Empty;
        Inputs = new List<RecipeItem>();
        Outputs = new List<RecipeItem>();
        CraftingTimeSeconds = 1.0;
    }
    
    /// <summary>
    /// Gets the production rate per minute for a given output item
    /// </summary>
    public double GetOutputRatePerMinute(string itemName)
    {
        var output = Outputs.Find(o => o.ItemName == itemName);
        if (output == null) return 0;
        
        return (output.Quantity / CraftingTimeSeconds) * 60.0;
    }
    
    /// <summary>
    /// Gets the consumption rate per minute for a given input item
    /// </summary>
    public double GetInputRatePerMinute(string itemName)
    {
        var input = Inputs.Find(i => i.ItemName == itemName);
        if (input == null) return 0;
        
        return (input.Quantity / CraftingTimeSeconds) * 60.0;
    }
}

/// <summary>
/// Represents a single item in a recipe (input or output)
/// </summary>
public class RecipeItem
{
    public string ItemName { get; set; }
    public double Quantity { get; set; } // Amount per batch
    
    public RecipeItem()
    {
        ItemName = string.Empty;
        Quantity = 0;
    }
    
    public RecipeItem(string itemName, double quantity)
    {
        ItemName = itemName;
        Quantity = quantity;
    }
}
