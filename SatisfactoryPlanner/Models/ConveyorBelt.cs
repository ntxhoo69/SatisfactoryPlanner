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

    public ConveyorBelt(Building sourceBuilding, IOPort sourcePort, Building targetBuilding, IOPort targetPort)
    {
        Id = Guid.NewGuid().ToString();
        SourceBuilding = sourceBuilding;
        SourcePort = sourcePort;
        TargetBuilding = targetBuilding;
        TargetPort = targetPort;
        
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
