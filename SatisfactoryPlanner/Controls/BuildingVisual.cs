using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner.Controls;

public class BuildingVisual : Canvas
    {
        private const double GridSize = 20; // pixels per meter
        private const double PortSize = 0.6; // meters
        
        public Building Building { get; private set; }
        private Rectangle buildingRect;
        
        // Event raised when a port is clicked
        public event EventHandler<PortClickedEventArgs>? PortClicked;
        
        public BuildingVisual(Building building)
        {
            Building = building;
            CreateVisual();
            ApplyRotation();
        }
        
        private void CreateVisual()
        {
            this.Width = Building.Type.WidthMeters * GridSize;
            this.Height = Building.Type. HeightMeters * GridSize;
            
            // Main building rectangle
            buildingRect = new Rectangle
            {
                Width = this.Width,
                Height = this.Height,
                Fill = new SolidColorBrush(Building.Type.Color) { Opacity = 0.7 },
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            this.Children.Add(buildingRect);
            
            // Building name label
            TextBlock nameLabel = new TextBlock
            {
                Text = Building. Type.Name,
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = this.Width
            };
            Canvas.SetLeft(nameLabel, 0);
            Canvas.SetTop(nameLabel, (this.Height / 2) - 10);
            this.Children.Add(nameLabel);
            
            // Draw ports
            foreach (var port in Building.Type. Ports)
            {
                DrawPort(port);
            }
        }
        
        private void DrawPort(IOPort port)
        {
            double portPixelSize = PortSize * GridSize;
            double x = (port.RelativePosition.X * GridSize) - (portPixelSize / 2);
            double y = (port.RelativePosition.Y * GridSize) - (portPixelSize / 2);
            
            // Port circle
            Ellipse portCircle = new Ellipse
            {
                Width = portPixelSize,
                Height = portPixelSize,
                Fill = GetPortColor(port),
                Stroke = port.Type == PortType.Input ? Brushes.OrangeRed : Brushes.ForestGreen,
                StrokeThickness = 2,
                Cursor = Cursors.Hand
            };
            
            // Store port reference in Tag for click handling
            portCircle.Tag = port;
            
            // Add click event handler
            portCircle.MouseLeftButtonDown += PortCircle_MouseLeftButtonDown;
            
            Canvas.SetLeft(portCircle, x);
            Canvas. SetTop(portCircle, y);
            this.Children.Add(portCircle);
            
            // Port label (smaller text)
            TextBlock portLabel = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Colors.Black) { Opacity = 0.6 }
            };
            
            Canvas.SetLeft(portLabel, x + portPixelSize + 2);
            Canvas.SetTop(portLabel, y);
            this.Children.Add(portLabel);
        }

        private Brush GetPortColor(IOPort port)
        {
            Color color = port.ResourceType switch
            {
                ResourceType.Solid => Colors.LightSlateGray,
                ResourceType.Fluid => Colors.DarkSlateBlue,
                _ => Colors.Gray
            };

            return new SolidColorBrush(color);
        }

        public void SetHighlight(bool highlighted)
        {
            buildingRect. Stroke = highlighted ? Brushes. Yellow : Brushes.White;
            buildingRect.StrokeThickness = highlighted ? 3 : 2;
        }
        
        /// <summary>
        /// Applies rotation transform to the building visual.
        /// </summary>
        private void ApplyRotation()
        {
            if (Building.Rotation != 0)
            {
                // Calculate center point for rotation
                double centerX = Building.Type.WidthMeters * GridSize / 2;
                double centerY = Building.Type.HeightMeters * GridSize / 2;
                
                // Apply rotation transform
                RotateTransform rotateTransform = new RotateTransform(Building.Rotation, centerX, centerY);
                this.RenderTransform = rotateTransform;
            }
            else
            {
                this.RenderTransform = null;
            }
        }
        
        /// <summary>
        /// Updates the visual to reflect changes in the building (e.g., rotation).
        /// </summary>
        public void UpdateVisual()
        {
            this.Children.Clear();
            CreateVisual();
            ApplyRotation();
        }
        
        private void PortCircle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.Tag is IOPort port)
            {
                // Raise the PortClicked event
                PortClicked?.Invoke(this, new PortClickedEventArgs(Building, port));
                e.Handled = true; // Prevent event from bubbling up
            }
        }
    }

/// <summary>
/// Event arguments for port click events
/// </summary>
public class PortClickedEventArgs : EventArgs
{
    public Building Building { get; }
    public IOPort Port { get; }
    
    public PortClickedEventArgs(Building building, IOPort port)
    {
        Building = building;
        Port = port;
    }
}