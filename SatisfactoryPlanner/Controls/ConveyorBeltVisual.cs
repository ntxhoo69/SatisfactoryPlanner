using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Controls;

/// <summary>
/// Visual representation of a conveyor belt on the canvas.
/// Draws a line between the source and target ports.
/// </summary>
public class ConveyorBeltVisual : Polyline
{
    private const double GridSize = 20; // pixels per meter (must match MainWindow)
    
    public ConveyorBelt ConveyorBelt { get; private set; }
    
    public ConveyorBeltVisual(ConveyorBelt conveyorBelt)
    {
        ConveyorBelt = conveyorBelt;
        CreateVisual();
    }
    
    private void CreateVisual()
    {
        // Get absolute positions in meters
        Point start = ConveyorBelt.StartPosition;
        Point end = ConveyorBelt.EndPosition;
        
        // Convert to pixel coordinates
        Point startPixels = new Point(start.X * GridSize, start.Y * GridSize);
        Point endPixels = new Point(end.X * GridSize, end.Y * GridSize);
        
        // Create a simple polyline with a slight curve for visual appeal
        // Using a 3-point line with a middle control point
        double midX = (startPixels.X + endPixels.X) / 2;
        double midY = (startPixels.Y + endPixels.Y) / 2;
        
        // Offset the middle point perpendicular to the line for a slight curve
        double dx = endPixels.X - startPixels.X;
        double dy = endPixels.Y - startPixels.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        
        if (length > 0)
        {
            // Perpendicular offset (10% of distance)
            double offsetAmount = length * 0.05;
            double perpX = -dy / length * offsetAmount;
            double perpY = dx / length * offsetAmount;
            
            midX += perpX;
            midY += perpY;
        }
        
        // Set polyline points
        this.Points = new PointCollection
        {
            startPixels,
            new Point(midX, midY),
            endPixels
        };
        
        // Style the conveyor belt based on resource type
        Color beltColor = ConveyorBelt.SourcePort.ResourceType switch
        {
            ResourceType.Solid => Colors.Yellow,
            ResourceType.Fluid => Colors.Cyan,
            _ => Colors.Orange
        };
        
        this.Stroke = new SolidColorBrush(beltColor);
        this.StrokeThickness = 3;
        this.StrokeLineJoin = PenLineJoin.Round;
        this.StrokeStartLineCap = PenLineCap.Round;
        this.StrokeEndLineCap = PenLineCap.Round;
        
        // Add slight opacity for better visual layering
        this.Opacity = 0.8;
    }
    
    /// <summary>
    /// Updates the visual when building positions change
    /// </summary>
    public void UpdateVisual()
    {
        CreateVisual();
    }
}
