using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Controls;

/// <summary>
/// Visual representation of a conveyor belt on the canvas.
/// Draws an orthogonal (axis-aligned) path between the source and target ports.
/// </summary>
public class ConveyorBeltVisual : Canvas
{
    private const double GridSize = 20; // pixels per meter (must match MainWindow)
    
    public ConveyorBelt ConveyorBelt { get; private set; }
    private Path beltPath;
    private TextBlock trafficLabel;
    
    public ConveyorBeltVisual(ConveyorBelt conveyorBelt)
    {
        ConveyorBelt = conveyorBelt;
        CreateVisual();
        
        // Make the belt clickable
        this.Cursor = System.Windows.Input.Cursors.Hand;
    }
    
    private void CreateVisual()
    {
        // Get absolute positions in meters
        Point startMeters = ConveyorBelt.StartPosition;
        Point endMeters = ConveyorBelt.EndPosition;
        
        // Get port facing directions
        Dir startFacing = ConveyorBelt.SourceBuilding.GetPortFacing(ConveyorBelt.SourcePort);
        Dir endFacing = ConveyorBelt.TargetBuilding.GetPortFacing(ConveyorBelt.TargetPort);
        
        // Convert to pixel coordinates
        Point startPixels = new Point(startMeters.X * GridSize, startMeters.Y * GridSize);
        Point endPixels = new Point(endMeters.X * GridSize, endMeters.Y * GridSize);
        
        // Compute orthogonal route
        List<Point> routePoints = BeltRouter.ComputeOrthogonalRoute(
            startPixels, 
            endPixels, 
            startFacing, 
            endFacing, 
            GridSize
        );
        
        // Create path geometry
        PathGeometry geometry = new PathGeometry();
        
        if (routePoints.Count >= 2)
        {
            PathFigure figure = new PathFigure
            {
                StartPoint = routePoints[0],
                IsClosed = false
            };
            
            // Add line segments for each point
            for (int i = 1; i < routePoints.Count; i++)
            {
                figure.Segments.Add(new LineSegment(routePoints[i], true));
            }
            
            geometry.Figures.Add(figure);
        }
        
        // Create or update the path
        if (beltPath == null)
        {
            beltPath = new Path();
            this.Children.Add(beltPath);
        }
        
        beltPath.Data = geometry;
        
        // Style the conveyor belt based on validity and resource type
        UpdateBeltStyle();
        
        // Add or update traffic label
        UpdateTrafficLabel(routePoints);
    }
    
    private void UpdateBeltStyle()
    {
        if (beltPath == null) return;
        
        // Choose color based on validity
        Color beltColor;
        if (!ConveyorBelt.IsValid)
        {
            beltColor = Colors.Red; // Invalid connection
        }
        else
        {
            beltColor = ConveyorBelt.SourcePort.ResourceType switch
            {
                ResourceType.Solid => Colors.Yellow,
                ResourceType.Fluid => Colors.Cyan,
                _ => Colors.Orange
            };
        }
        
        beltPath.Stroke = new SolidColorBrush(beltColor);
        beltPath.StrokeThickness = 3;
        beltPath.StrokeLineJoin = PenLineJoin.Round;
        beltPath.StrokeStartLineCap = PenLineCap.Round;
        beltPath.StrokeEndLineCap = PenLineCap.Round;
        beltPath.Opacity = 0.8;
        beltPath.Cursor = System.Windows.Input.Cursors.Hand;
    }
    
    private void UpdateTrafficLabel(List<Point> routePoints)
    {
        if (routePoints.Count < 2) return;
        
        // Calculate midpoint of the belt path
        int midIndex = routePoints.Count / 2;
        Point midPoint = routePoints[midIndex];
        
        // Create or update traffic label
        if (trafficLabel == null)
        {
            trafficLabel = new TextBlock
            {
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Colors.Black) { Opacity = 0.7 },
                Padding = new Thickness(2)
            };
            this.Children.Add(trafficLabel);
        }
        
        // Update traffic text
        if (!string.IsNullOrEmpty(ConveyorBelt.ItemName) && ConveyorBelt.ItemsPerMinute > 0)
        {
            trafficLabel.Text = $"{ConveyorBelt.ItemName}: {ConveyorBelt.ItemsPerMinute:F1}/min";
            trafficLabel.Visibility = Visibility.Visible;
        }
        else
        {
            trafficLabel.Text = "No traffic";
            trafficLabel.Visibility = Visibility.Collapsed;
        }
        
        // Position the label at the midpoint
        Canvas.SetLeft(trafficLabel, midPoint.X + 5);
        Canvas.SetTop(trafficLabel, midPoint.Y - 15);
    }
    
    /// <summary>
    /// Updates the visual when building positions change or traffic changes
    /// </summary>
    public void UpdateVisual()
    {
        CreateVisual();
    }
    
    /// <summary>
    /// Sets the highlight state for the conveyor belt
    /// </summary>
    public void SetHighlight(bool highlighted)
    {
        if (beltPath != null)
        {
            beltPath.StrokeThickness = highlighted ? 5 : 3;
            beltPath.Opacity = highlighted ? 1.0 : 0.8;
        }
    }
}
