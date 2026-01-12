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
            
        int rotationSteps = NormalizeRotationSteps(rotation);
        
        double x = point.X;
        double y = point.Y;
        double centerX = width / 2.0;
        double centerY = height / 2.0;
        
        // Translate to origin
        x -= centerX;
        y -= centerY;
        
        // Apply rotation steps (each step is 90 degrees clockwise)
        for (int i = 0; i < rotationSteps; i++)
        {
            // Rotate 90 degrees clockwise: (x, y) -> (y, -x)
            double tempX = y;
            double tempY = -x;
            x = tempX;
            y = tempY;
        }
        
        // Translate back - note: for 90/270 rotations, width and height are swapped
        if (rotationSteps % 2 == 1)
        {
            x += centerY;  // Swapped
            y += centerX;  // Swapped
        }
        else
        {
            x += centerX;
            y += centerY;
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
            
        int rotationSteps = NormalizeRotationSteps(rotation);
        
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
    /// Normalizes rotation angle to steps (0-3) where each step is 90 degrees.
    /// </summary>
    private int NormalizeRotationSteps(double rotation)
    {
        int rotationSteps = ((int)(rotation / 90)) % 4;
        if (rotationSteps < 0) rotationSteps += 4;
        return rotationSteps;
    }
    
    /// <summary>
    /// Rotates the building by 90 degrees clockwise.
    /// </summary>
    public void Rotate()
    {
        Rotation = (Rotation + 90) % 360;
    }
}