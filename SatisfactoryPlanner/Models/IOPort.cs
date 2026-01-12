using System.Windows;

namespace SatisfactoryPlanner.Models;

public enum PortType
{
    Input,
    Output
}

public enum ResourceType
{
    Fluid,
    Solid
}

public class IOPort
{
    public PortType Type { get; set; }
    public ResourceType ResourceType { get; set; }
    public Point RelativePosition { get; set; } // Position relative to building (in meters)
    public string Name { get; set; }

    public IOPort(PortType type, ResourceType resourceType, Point relativePosition, string name = "")
    {
        Type = type;
        ResourceType = resourceType;
        RelativePosition = relativePosition;
        Name = name;
    }
}