using System. Collections.Generic;
using System.Windows. Media;

namespace SatisfactoryPlanner.Models;

public class BuildingType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double WidthMeters { get; set; }
    public double HeightMeters { get; set; }
    public Color Color { get; set; }
    public List<IOPort> Ports { get; set; }
        
    public BuildingType()
    {
        Ports = new List<IOPort>();
    }
}