using System;
using System.Windows;

namespace SatisfactoryPlanner.Models;

/// <summary>
/// Represents a conveyor belt connection between two buildings.
/// Connects an Output port of one building to an Input port of another building.
/// </summary>
public class ConveyorBelt
{
    public string Id { get; set; }
    public Building SourceBuilding { get; set; }
    public IOPort SourcePort { get; set; }
    public Building TargetBuilding { get; set; }
    public IOPort TargetPort { get; set; }
    
    // Traffic tracking
    public string? ItemName { get; set; } // What item is being transported
    public double ItemsPerMinute { get; set; } // Flow rate
    public bool IsValid { get; set; } // Whether the connection is valid (matches types, has sufficient input)

    public ConveyorBelt(Building sourceBuilding, IOPort sourcePort, Building targetBuilding, IOPort targetPort)
    {
        Id = Guid.NewGuid().ToString();
        SourceBuilding = sourceBuilding;
        SourcePort = sourcePort;
        TargetBuilding = targetBuilding;
        TargetPort = targetPort;
        IsValid = true;
        ItemsPerMinute = 0;
        
        // Validate that source is Output and target is Input
        if (sourcePort.Type != PortType.Output)
        {
            throw new ArgumentException("Source port must be an Output port", nameof(sourcePort));
        }
        
        if (targetPort.Type != PortType.Input)
        {
            throw new ArgumentException("Target port must be an Input port", nameof(targetPort));
        }
    }

    /// <summary>
    /// Gets the absolute position of the source port in meters
    /// </summary>
    public Point StartPosition => SourceBuilding.GetPortAbsolutePosition(SourcePort);

    /// <summary>
    /// Gets the absolute position of the target port in meters
    /// </summary>
    public Point EndPosition => TargetBuilding.GetPortAbsolutePosition(TargetPort);
}
