using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SatisfactoryPlanner.dto;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Data;

public static class RecipeTypes
{
    private static List<Recipe>? _recipes;

    // Remove hard-coded path, will use relative path resolution
    private static readonly string DefaultJsonPath = "Assets/recipes.json";

    public static List<Recipe> GetAll()
    {
        if (_recipes == null || _recipes.Count == 0)
            LoadRecipesFromJson();
        return _recipes!;
    }

    public static List<Recipe> GetRecipesForBuilding(string buildingId)
    {
        return GetAll().Where(r => r.BuildingId.Equals(buildingId, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static void LoadRecipesFromJson(string? jsonPath = null)
    {
        try
        {
            string path = string.IsNullOrWhiteSpace(jsonPath) ? DefaultJsonPath : jsonPath;
            string jsonContent = ReadJsonFile(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var data = JsonSerializer.Deserialize<RecipesData>(jsonContent, options);

            if (data?.Recipes == null)
            {
                Debug.WriteLine("No recipes found in JSON file or file is empty.");
                _recipes = new List<Recipe>();
                return;
            }

            _recipes = data.Recipes.Select(ConvertToRecipe).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading recipes from JSON: {ex}");
            _recipes = new List<Recipe>();
        }
    }

    private static string ReadJsonFile(string path)
    {
        // 1) Direct path
        if (File.Exists(path))
            return File.ReadAllText(path);
        
        Console.WriteLine($"File not found at '{path}'");

        // 2) Relative to executable
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        string altPath = Path.Combine(exeDir, "Assets", "recipes.json");

        if (File.Exists(altPath))
            return File.ReadAllText(altPath);

        // 3) Relative to AppContext BaseDirectory
        string baseDir = AppContext.BaseDirectory;
        string altPath2 = Path.Combine(baseDir, "Assets", "recipes.json");

        if (File.Exists(altPath2))
            return File.ReadAllText(altPath2);

        throw new FileNotFoundException($"Recipes JSON file not found at: '{path}', '{altPath}', or '{altPath2}'");
    }

    private static Recipe ConvertToRecipe(RecipeDto dto)
    {
        return new Recipe
        {
            Id = dto.Id,
            Name = dto.Name,
            BuildingId = dto.BuildingId,
            Inputs = dto.Inputs.Select(i => new RecipeItem(i.ItemName, i.Quantity)).ToList(),
            Outputs = dto.Outputs.Select(o => new RecipeItem(o.ItemName, o.Quantity)).ToList(),
            CraftingTimeSeconds = dto.CraftingTimeSeconds
        };
    }

    public static Recipe? GetById(string id)
    {
        return GetAll().Find(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public static void Reload(string? jsonPath = null)
    {
        _recipes = null;
        LoadRecipesFromJson(jsonPath);
    }
}
