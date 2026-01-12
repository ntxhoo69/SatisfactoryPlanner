using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    
    public ConveyorBeltVisual(ConveyorBelt conveyorBelt)
    {
        ConveyorBelt = conveyorBelt;
        CreateVisual();
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
        
        // Style the conveyor belt based on resource type
        Color beltColor = ConveyorBelt.SourcePort.ResourceType switch
        {
            ResourceType.Solid => Colors.Yellow,
            ResourceType.Fluid => Colors.Cyan,
            _ => Colors.Orange
        };
        
        beltPath.Stroke = new SolidColorBrush(beltColor);
        beltPath.StrokeThickness = 3;
        beltPath.StrokeLineJoin = PenLineJoin.Round;
        beltPath.StrokeStartLineCap = PenLineCap.Round;
        beltPath.StrokeEndLineCap = PenLineCap.Round;
        beltPath.Opacity = 0.8;
    }
    
    /// <summary>
    /// Updates the visual when building positions change
    /// </summary>
    public void UpdateVisual()
    {
        CreateVisual();
    }
}
