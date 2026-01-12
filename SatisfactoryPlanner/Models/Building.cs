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
        Point rotatedPosition = RotatePoint(port.RelativePosition, Rotation, Type.WidthMeters, Type.HeightMeters);
        return new Point(
            Position.X + rotatedPosition.X,
            Position.Y + rotatedPosition.Y
        );
    }
    
    /// <summary>
    /// Gets the facing direction of a port, accounting for building rotation.
    /// Supports 0, 90, 180, 270 degree rotations.
    /// </summary>
    public Dir GetPortFacing(IOPort port)
    {
        return RotateDirection(port.Facing, Rotation);
    }
    
    /// <summary>
    /// Rotates a point around the building's center based on rotation angle.
    /// </summary>
    private Point RotatePoint(Point point, double rotation, double width, double height)
    {
        if (rotation == 0)
            return point;
            
        // Normalize rotation to 0, 90, 180, or 270
        int rotationSteps = ((int)(rotation / 90)) % 4;
        if (rotationSteps < 0) rotationSteps += 4;
        
        double x = point.X;
        double y = point.Y;
        
        for (int i = 0; i < rotationSteps; i++)
        {
            // Rotate 90 degrees clockwise around building center
            double tempX = x;
            double tempY = y;
            
            // Translate to origin (center of building)
            double centerX = width / 2.0;
            double centerY = height / 2.0;
            tempX -= centerX;
            tempY -= centerY;
            
            // Rotate 90 degrees clockwise: (x, y) -> (y, -x)
            double newX = tempY;
            double newY = -tempX;
            
            // Translate back, but swap width and height for 90/270 rotations
            if (i % 2 == 0)
            {
                x = newX + centerY;
                y = newY + centerX;
            }
            else
            {
                x = newX + centerX;
                y = newY + centerY;
            }
        }
        
        return new Point(x, y);
    }
    
    /// <summary>
    /// Rotates a direction based on rotation angle.
    /// </summary>
    private Dir RotateDirection(Dir direction, double rotation)
    {
        if (rotation == 0)
            return direction;
            
        // Normalize rotation to 0, 90, 180, or 270
        int rotationSteps = ((int)(rotation / 90)) % 4;
        if (rotationSteps < 0) rotationSteps += 4;
        
        Dir result = direction;
        for (int i = 0; i < rotationSteps; i++)
        {
            // Rotate 90 degrees clockwise
            result = result switch
            {
                Dir.Up => Dir.Right,
                Dir.Right => Dir.Down,
                Dir.Down => Dir.Left,
                Dir.Left => Dir.Up,
                _ => result
            };
        }
        
        return result;
    }
    
    /// <summary>
    /// Rotates the building by 90 degrees clockwise.
    /// </summary>
    public void Rotate()
    {
        Rotation = (Rotation + 90) % 360;
    }
}