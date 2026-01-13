using System.Collections.Generic;

namespace SatisfactoryPlanner.dto;

public class RecipeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BuildingId { get; set; } = string.Empty;
    public List<RecipeItemDto> Inputs { get; set; } = new();
    public List<RecipeItemDto> Outputs { get; set; } = new();
    public double CraftingTimeSeconds { get; set; } = 1.0;
}

public class RecipeItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public double Quantity { get; set; } = 0;
}

public class RecipesData
{
    public List<RecipeDto> Recipes { get; set; } = new();
}
