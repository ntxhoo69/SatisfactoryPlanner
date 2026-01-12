using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SatisfactoryPlanner. dto;

public class BuildingTypesData
{
    [JsonPropertyName("buildings")]
    public List<BuildingTypeDto> Buildings { get; set; }
}

public class BuildingTypeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("widthMeters")]
    public double WidthMeters { get; set; }
    
    [JsonPropertyName("heightMeters")]
    public double HeightMeters { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; }
    
    [JsonPropertyName("ports")]
    public List<IOPortDto> Ports { get; set; }
}

public class IOPortDto
{
    [JsonPropertyName("portType")]
    public string PortType { get; set; }
    
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; }
    
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
    
    [JsonPropertyName("label")]
    public string Label { get; set; }
}