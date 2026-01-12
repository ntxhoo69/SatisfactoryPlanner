using System.Windows;

namespace SatisfactoryPlanner.Models;

public class Building
{
    public string Id { get; set; }
    public BuildingType Type { get; set; }
    public Point Position { get; set; } // Position in meters on the grid
    public double Rotation { get; set; } // For future rotation support
        
    public Building(BuildingType type, Point position)
    {
        Id = System.Guid.NewGuid().ToString();
        Type = type;
        Position = position;
        Rotation = 0;
    }
        
    // Get absolute position of a port
    public Point GetPortAbsolutePosition(IOPort port)
    {
        return new Point(
            Position.X + port.RelativePosition.X,
            Position.Y + port.RelativePosition.Y
        );
    }
}