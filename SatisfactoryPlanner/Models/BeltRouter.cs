using System;
using System.Collections.Generic;
using System.Windows;

namespace SatisfactoryPlanner.Models;

/// <summary>
/// Utility class for computing orthogonal (axis-aligned) belt routing paths.
/// Implements "straight mode" routing similar to Satisfactory conveyor belts.
/// </summary>
public static class BeltRouter
{
    /// <summary>
    /// Computes an orthogonal polyline path from start to end with proper facing alignment.
    /// Returns a list of points in pixel coordinates.
    /// </summary>
    /// <param name="startPx">Start point in pixels</param>
    /// <param name="endPx">End point in pixels</param>
    /// <param name="startFacing">Direction the belt should exit from the start port</param>
    /// <param name="endFacing">Direction the belt should enter the end port</param>
    /// <param name="gridSize">Size of one grid unit in pixels (for lead distance)</param>
    /// <returns>List of points forming an orthogonal path</returns>
    public static List<Point> ComputeOrthogonalRoute(Point startPx, Point endPx, Dir startFacing, Dir endFacing, double gridSize)
    {
        // If start and end are the same, return empty path
        if (Math.Abs(startPx.X - endPx.X) < 0.01 && Math.Abs(startPx.Y - endPx.Y) < 0.01)
        {
            return new List<Point>();
        }
        
        // Snap to grid (optional but helps with alignment)
        Point start = SnapToGrid(startPx, gridSize);
        Point end = SnapToGrid(endPx, gridSize);
        
        // Create lead points: extend from start/end in facing directions
        double leadDistance = gridSize * 1.0; // One grid unit
        Point s1 = start + FacingVector(startFacing) * leadDistance;
        Point e1 = end + FacingVector(endFacing) * leadDistance;
        
        // Build orthogonal path between s1 and e1
        List<Point> middlePoints = BuildOrthogonalPath(s1, e1, startFacing, endFacing);
        
        // Assemble final path: start -> s1 -> middle points -> e1 -> end
        List<Point> path = new List<Point> { start };
        path.Add(s1);
        path.AddRange(middlePoints);
        path.Add(e1);
        path.Add(end);
        
        // Clean up the path
        path = RemoveDuplicates(path);
        path = RemoveCollinear(path);
        
        return path;
    }
    
    /// <summary>
    /// Builds an orthogonal zig-zag path between two points.
    /// </summary>
    private static List<Point> BuildOrthogonalPath(Point s1, Point e1, Dir startFacing, Dir endFacing)
    {
        // Use heuristic to choose horizontal-first or vertical-first routing
        bool useHorizontalFirst = (startFacing == Dir.Left || startFacing == Dir.Right);
        
        List<Point> points = new List<Point>();
        
        if (useHorizontalFirst)
        {
            // Horizontal-first: go to target X first, then target Y
            Point corner1 = new Point(e1.X, s1.Y);
            if (!AreSamePoint(s1, corner1) && !AreSamePoint(corner1, e1))
            {
                points.Add(corner1);
            }
        }
        else
        {
            // Vertical-first: go to target Y first, then target X
            Point corner1 = new Point(s1.X, e1.Y);
            if (!AreSamePoint(s1, corner1) && !AreSamePoint(corner1, e1))
            {
                points.Add(corner1);
            }
        }
        
        return points;
    }
    
    /// <summary>
    /// Returns a unit vector for the given direction.
    /// </summary>
    private static Vector FacingVector(Dir dir)
    {
        return dir switch
        {
            Dir.Up => new Vector(0, -1),
            Dir.Right => new Vector(1, 0),
            Dir.Down => new Vector(0, 1),
            Dir.Left => new Vector(-1, 0),
            _ => new Vector(0, 0)
        };
    }
    
    /// <summary>
    /// Snaps a point to the nearest grid position.
    /// </summary>
    private static Point SnapToGrid(Point p, double gridSize)
    {
        return new Point(
            Math.Round(p.X / gridSize) * gridSize,
            Math.Round(p.Y / gridSize) * gridSize
        );
    }
    
    /// <summary>
    /// Removes consecutive duplicate points from the path.
    /// </summary>
    private static List<Point> RemoveDuplicates(List<Point> path)
    {
        if (path.Count <= 1) return path;
        
        List<Point> result = new List<Point> { path[0] };
        for (int i = 1; i < path.Count; i++)
        {
            if (!AreSamePoint(path[i], path[i - 1]))
            {
                result.Add(path[i]);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Removes collinear intermediate points (A->B->C where B is on line A-C).
    /// </summary>
    private static List<Point> RemoveCollinear(List<Point> path)
    {
        if (path.Count <= 2) return path;
        
        List<Point> result = new List<Point> { path[0] };
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Point prev = path[i - 1];
            Point curr = path[i];
            Point next = path[i + 1];
            
            // Check if current point is on the same line as prev and next
            bool sameX = Math.Abs(prev.X - curr.X) < 0.01 && Math.Abs(curr.X - next.X) < 0.01;
            bool sameY = Math.Abs(prev.Y - curr.Y) < 0.01 && Math.Abs(curr.Y - next.Y) < 0.01;
            
            if (!sameX && !sameY)
            {
                result.Add(curr);
            }
        }
        
        result.Add(path[path.Count - 1]);
        return result;
    }
    
    /// <summary>
    /// Checks if two points are effectively the same (within tolerance).
    /// </summary>
    private static bool AreSamePoint(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) < 0.01 && Math.Abs(a.Y - b.Y) < 0.01;
    }
}
