using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using SatisfactoryPlanner.dto;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Data;

public static class BuildingTypes
{
    private static List<BuildingType>? _types;

    private static readonly string DefaultJsonPath =
        @"C:\Users\bedynelu\Documents\RIDER .NET\SatisfactoryPlanner\SatisfactoryPlanner\Assets\buildings.json";

    public static List<BuildingType> GetAll()
    {
        if (_types == null || _types.Count == 0)
            LoadBuildingTypesFromJson();
        return _types!;
    }

    private static void LoadBuildingTypesFromJson(string? jsonPath = null)
    {
        try
        {
            string path = string.IsNullOrWhiteSpace(jsonPath) ? DefaultJsonPath : jsonPath;
            string jsonContent = ReadJsonFile(path);

            Console.WriteLine(jsonContent);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var data = JsonSerializer.Deserialize<BuildingTypesData>(jsonContent, options);

            if (data?.Buildings == null || data.Buildings.Count == 0)
                throw new InvalidOperationException("No buildings found in JSON file (property 'buildings' is empty or missing).");

            _types = data.Buildings.Select(ConvertToBuildingType).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading buildings from JSON: {ex}");

            // WICHTIG: Nicht null lassen → verhindert NRE an Callsites
            _types = new List<BuildingType>();
        }
    }

    private static string ReadJsonFile(string path)
    {
        // 1) Direktpfad
        if (File.Exists(path))
            return File.ReadAllText(path);
        Console.WriteLine($"File not found at '{path}'");

        // 2) Relativ zum Executable
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        string altPath = Path.Combine(exeDir, path);

        if (File.Exists(altPath))
            return File.ReadAllText(altPath);

        // 3) Relativ zur AppContext BaseDirectory (WPF oft stabiler)
        string baseDir = AppContext.BaseDirectory;
        string altPath2 = Path.Combine(baseDir, path);

        if (File.Exists(altPath2))
            return File.ReadAllText(altPath2);

        throw new FileNotFoundException($"Building types JSON file not found at: '{path}', '{altPath}', or '{altPath2}'");
    }

    private static BuildingType ConvertToBuildingType(BuildingTypeDto dto)
    {
        var buildingType = new BuildingType
        {
            Id = dto.Id,
            Name = dto.Name,
            WidthMeters = dto.WidthMeters,
            HeightMeters = dto.HeightMeters,
            Color = ParseColor(dto.Color),
            Ports = dto.Ports.Select(ConvertToIOPort).ToList()
        };
        
        // Compute facing direction for each port based on its position on the building
        foreach (var port in buildingType.Ports)
        {
            port.Facing = ComputePortFacing(port.RelativePosition, buildingType.WidthMeters, buildingType.HeightMeters);
        }
        
        return buildingType;
    }
    
    /// <summary>
    /// Computes the facing direction of a port based on its position relative to the building edges.
    /// Ports on the left edge face left, right edge face right, top face up, bottom face down.
    /// </summary>
    private static Dir ComputePortFacing(Point portPosition, double buildingWidth, double buildingHeight)
    {
        const double tolerance = 0.1; // meters tolerance for edge detection
        
        double x = portPosition.X;
        double y = portPosition.Y;
        
        // Calculate distances to each edge
        double distToLeft = Math.Abs(x - 0);
        double distToRight = Math.Abs(x - buildingWidth);
        double distToTop = Math.Abs(y - 0);
        double distToBottom = Math.Abs(y - buildingHeight);
        
        // Find the minimum distance to determine which edge the port is on
        double minDist = Math.Min(Math.Min(distToLeft, distToRight), Math.Min(distToTop, distToBottom));
        
        // Determine facing based on closest edge
        if (Math.Abs(minDist - distToLeft) < tolerance)
            return Dir.Left;
        if (Math.Abs(minDist - distToRight) < tolerance)
            return Dir.Right;
        if (Math.Abs(minDist - distToTop) < tolerance)
            return Dir.Up;
        if (Math.Abs(minDist - distToBottom) < tolerance)
            return Dir.Down;
        
        // Default to right if we can't determine (shouldn't happen with valid data)
        return Dir.Right;
    }

    private static IOPort ConvertToIOPort(IOPortDto dto)
    {
        var portType = ParseEnumOrThrow<PortType>(dto.PortType, nameof(dto.PortType));
        var resourceType = ParseEnumOrThrow<ResourceType>(dto.ResourceType, nameof(dto.ResourceType));

        var position = new Point(dto.X, dto.Y);
        var facing = Dir.Right; // Default, will be overridden by ComputePortFacing
        return new IOPort(portType, resourceType, position, dto.Label, facing);
    }

    private static TEnum ParseEnumOrThrow<TEnum>(string? value, string fieldName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing enum value for '{fieldName}'.");

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return result;

        throw new InvalidOperationException($"Invalid value '{value}' for enum '{typeof(TEnum).Name}' in field '{fieldName}'.");
    }

    private static Color ParseColor(string? colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName))
            return Colors.Gray;

        try
        {
            // Named colors (e.g., "DarkOrange")
            var colorProperty = typeof(Colors).GetProperty(colorName, BindingFlags.Public | BindingFlags.Static);
            if (colorProperty?.GetValue(null) is Color named)
                return named;

            // Hex colors (#RRGGBB or #AARRGGBB)
            if (colorName.StartsWith("#", StringComparison.Ordinal))
            {
                var obj = ColorConverter.ConvertFromString(colorName);
                if (obj is Color hex)
                    return hex;
            }

            return Colors.Gray;
        }
        catch
        {
            return Colors.Gray;
        }
    }

    public static BuildingType? GetById(string id)
    {
        return GetAll().Find(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public static void Reload(string? jsonPath = null)
    {
        _types = null;
        LoadBuildingTypesFromJson(jsonPath);
    }
}
