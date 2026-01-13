using System.Collections.Generic;
using System.Linq;

namespace SatisfactoryPlanner.Models;

/// <summary>
/// Calculates production rates and validates conveyor belt connections
/// </summary>
public static class ProductionCalculator
{
    /// <summary>
    /// Recalculates all production rates and validates all conveyor belts
    /// </summary>
    public static void RecalculateAll(List<Building> buildings, List<ConveyorBelt> conveyorBelts)
    {
        // First, reset all belt traffic
        foreach (var belt in conveyorBelts)
        {
            belt.ItemsPerMinute = 0;
            belt.ItemName = null;
            belt.IsValid = true;
        }
        
        // Calculate output for each building
        foreach (var building in buildings)
        {
            CalculateBuildingOutput(building, buildings, conveyorBelts);
        }
        
        // Validate all connections
        foreach (var belt in conveyorBelts)
        {
            ValidateConveyorBelt(belt, buildings, conveyorBelts);
        }
    }
    
    /// <summary>
    /// Calculates the output for a single building based on its inputs and recipe
    /// </summary>
    private static void CalculateBuildingOutput(Building building, List<Building> allBuildings, List<ConveyorBelt> conveyorBelts)
    {
        // Source buildings have fixed output
        if (building.IsSource())
        {
            var outputBelts = conveyorBelts.Where(b => b.SourceBuilding.Id == building.Id).ToList();
            foreach (var belt in outputBelts)
            {
                belt.ItemName = building.SourceItemName ?? "Unknown";
                belt.ItemsPerMinute = building.SourceItemRate;
            }
            return;
        }
        
        // Buildings with recipes
        if (building.SelectedRecipe == null)
        {
            // No recipe selected, no output
            return;
        }
        
        // Get input belts for this building
        var inputBelts = conveyorBelts.Where(b => b.TargetBuilding.Id == building.Id).ToList();
        
        // Calculate limiting factor based on inputs
        double productionMultiplier = CalculateProductionMultiplier(building.SelectedRecipe, inputBelts);
        
        // Set output on all output belts
        var outputBelts = conveyorBelts.Where(b => b.SourceBuilding.Id == building.Id).ToList();
        
        foreach (var output in building.SelectedRecipe.Outputs)
        {
            double outputRate = building.SelectedRecipe.GetOutputRatePerMinute(output.ItemName) * productionMultiplier;
            
            // Find belts for this output item
            var beltsForItem = outputBelts.Where(b => b.SourcePort.Name.Contains(output.ItemName) || outputBelts.Count == 1).ToList();
            if (beltsForItem.Count == 0 && outputBelts.Count > 0)
            {
                beltsForItem = outputBelts; // Fallback to all output belts
            }
            
            foreach (var belt in beltsForItem)
            {
                belt.ItemName = output.ItemName;
                belt.ItemsPerMinute = outputRate;
            }
        }
    }
    
    /// <summary>
    /// Calculates the production multiplier based on available inputs
    /// Returns 1.0 if all inputs are satisfied, less if inputs are insufficient
    /// </summary>
    private static double CalculateProductionMultiplier(Recipe recipe, List<ConveyorBelt> inputBelts)
    {
        if (recipe.Inputs.Count == 0)
            return 1.0; // No inputs needed
        
        double minMultiplier = 1.0;
        
        foreach (var requiredInput in recipe.Inputs)
        {
            double requiredRate = recipe.GetInputRatePerMinute(requiredInput.ItemName);
            
            // Find all belts providing this item
            var providingBelts = inputBelts.Where(b => 
                b.ItemName != null && 
                b.ItemName.Equals(requiredInput.ItemName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            double availableRate = providingBelts.Sum(b => b.ItemsPerMinute);
            
            if (requiredRate > 0)
            {
                double multiplier = availableRate / requiredRate;
                minMultiplier = Math.Min(minMultiplier, multiplier);
            }
        }
        
        return Math.Max(0, Math.Min(1.0, minMultiplier)); // Clamp between 0 and 1
    }
    
    /// <summary>
    /// Validates a conveyor belt connection
    /// </summary>
    private static void ValidateConveyorBelt(ConveyorBelt belt, List<Building> allBuildings, List<ConveyorBelt> allBelts)
    {
        // Check if the target building needs this item
        if (!belt.TargetBuilding.IsSource() && belt.TargetBuilding.SelectedRecipe != null)
        {
            var recipe = belt.TargetBuilding.SelectedRecipe;
            
            // Check if this item is needed by the recipe
            if (belt.ItemName != null)
            {
                bool itemNeeded = recipe.Inputs.Any(i => 
                    i.ItemName.Equals(belt.ItemName, StringComparison.OrdinalIgnoreCase)
                );
                
                if (!itemNeeded)
                {
                    belt.IsValid = false; // Wrong item type
                    return;
                }
            }
        }
        
        belt.IsValid = true;
    }
    
    /// <summary>
    /// Gets the total input rate for a specific item at a building
    /// </summary>
    public static double GetInputRate(Building building, string itemName, List<ConveyorBelt> conveyorBelts)
    {
        return conveyorBelts
            .Where(b => b.TargetBuilding.Id == building.Id && 
                        b.ItemName != null && 
                        b.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase))
            .Sum(b => b.ItemsPerMinute);
    }
    
    /// <summary>
    /// Gets the required input rate for a specific item at a building
    /// </summary>
    public static double GetRequiredInputRate(Building building, string itemName)
    {
        if (building.SelectedRecipe == null)
            return 0;
        
        return building.SelectedRecipe.GetInputRatePerMinute(itemName);
    }
}
