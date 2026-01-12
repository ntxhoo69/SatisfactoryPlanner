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

/// <summary>
/// Direction enum for port facing (cardinal directions)
/// </summary>
public enum Dir
{
    Up,    // Facing upward (negative Y)
    Right, // Facing right (positive X)
    Down,  // Facing downward (positive Y)
    Left   // Facing left (negative X)
}

public class IOPort
{
    public PortType Type { get; set; }
    public ResourceType ResourceType { get; set; }
    public Point RelativePosition { get; set; } // Position relative to building (in meters)
    public string Name { get; set; }
    public Dir Facing { get; set; } // Direction the port faces (for belt routing)

    public IOPort(PortType type, ResourceType resourceType, Point relativePosition, string name = "", Dir facing = Dir.Right)
    {
        Type = type;
        ResourceType = resourceType;
        RelativePosition = relativePosition;
        Name = name;
        Facing = facing;
    }
}