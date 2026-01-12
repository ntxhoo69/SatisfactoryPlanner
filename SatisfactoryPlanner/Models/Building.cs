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
    
    /// <summary>
    /// Gets the facing direction of a port, accounting for building rotation.
    /// For now, only supports rotation=0. Can be extended for 90/180/270 degree rotations.
    /// </summary>
    public Dir GetPortFacing(IOPort port)
    {
        // TODO: Apply rotation transformation when building rotation is implemented
        // For rotation=0, just return the port's facing directly
        if (Math.Abs(Rotation) < 0.01)
        {
            return port.Facing;
        }
        
        // For future: rotate the facing by the building rotation
        // e.g., 90° rotation: Up -> Right, Right -> Down, Down -> Left, Left -> Up
        return port.Facing;
    }
}