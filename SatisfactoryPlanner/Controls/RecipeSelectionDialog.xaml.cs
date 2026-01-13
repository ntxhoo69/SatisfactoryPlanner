using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SatisfactoryPlanner.Data;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Controls;

public partial class RecipeSelectionDialog : Window
{
    public Recipe? SelectedRecipe { get; private set; }
    
    public RecipeSelectionDialog(string buildingId)
    {
        InitializeComponent();
        LoadRecipes(buildingId);
    }
    
    private void LoadRecipes(string buildingId)
    {
        var recipes = RecipeTypes.GetRecipesForBuilding(buildingId);
        
        // Create view models for the recipes
        var recipeViewModels = recipes.Select(r => new RecipeViewModel(r)).ToList();
        
        RecipeListBox.ItemsSource = recipeViewModels;
        
        if (recipeViewModels.Count > 0)
        {
            RecipeListBox.SelectedIndex = 0;
        }
    }
    
    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeListBox.SelectedItem is RecipeViewModel vm)
        {
            SelectedRecipe = vm.Recipe;
            DialogResult = true;
        }
        Close();
    }
    
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    private void RecipeListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OK_Click(sender, e);
    }
}

// View model for displaying recipes in the list
public class RecipeViewModel
{
    public Recipe Recipe { get; }
    public string Name => Recipe.Name;
    
    public string InputsText
    {
        get
        {
            if (Recipe.Inputs.Count == 0) return "Inputs: None";
            var inputs = string.Join(", ", Recipe.Inputs.Select(i => $"{i.ItemName} ({Recipe.GetInputRatePerMinute(i.ItemName):F1}/min)"));
            return $"Inputs: {inputs}";
        }
    }
    
    public string OutputsText
    {
        get
        {
            if (Recipe.Outputs.Count == 0) return "Outputs: None";
            var outputs = string.Join(", ", Recipe.Outputs.Select(o => $"{o.ItemName} ({Recipe.GetOutputRatePerMinute(o.ItemName):F1}/min)"));
            return $"Outputs: {outputs}";
        }
    }
    
    public RecipeViewModel(Recipe recipe)
    {
        Recipe = recipe;
    }
}
