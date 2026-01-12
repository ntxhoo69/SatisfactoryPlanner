using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows. Shapes;
using SatisfactoryPlanner.Data;
using System.Collections.Generic;
using SatisfactoryPlanner.Controls;
using SatisfactoryPlanner.Models;

namespace SatisfactoryPlanner
{
    public partial class MainWindow : Window
    {
        private int GridSeperatorSize = 8;
        private const double GridSize = 20; // 20 pixels = 1 meter at 100% zoom
        private const double MinZoom = 0.1;
        private const double MaxZoom = 5.0;
        private const double ZoomSpeed = 0.1;
        
        private double currentZoom = 1.0;
        private Point lastMousePosition;
        private bool isPanning = false;
        
        // Grid rendering
        private int gridWidth = 200;  // 200 meters
        private int gridHeight = 200; // 200 meters
        
        private List<Building> buildings = new List<Building>();
        private BuildingType selectedBuildingType = null;
        private BuildingVisual previewBuilding = null;
        private bool isPlacingMode = false;

        public MainWindow()
        {
            InitializeComponent();
            
            ScaleTransform.ScaleX = currentZoom;
            ScaleTransform.ScaleY = currentZoom;
            
            DrawGrid();
            CenterView();
            PopulateBuildingToolbar();
            
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.Focus();
        }

        private void DrawGrid()
        {
            MainCanvas.Children.Clear();
            
            double totalWidth = gridWidth * GridSize;
            double totalHeight = gridHeight * GridSize;
            
            // Set canvas size
            MainCanvas.Width = totalWidth;
            MainCanvas. Height = totalHeight;
            
            // Draw vertical lines
            for (int x = 0; x <= gridWidth; x++)
            {
                Line line = new Line
                {
                    X1 = x * GridSize,
                    Y1 = 0,
                    X2 = x * GridSize,
                    Y2 = totalHeight,
                    Stroke = GetGridLineBrush(x),
                    StrokeThickness = GetGridLineThickness(x)
                };
                
                MainCanvas.Children.Add(line);
            }
            
            // Draw horizontal lines
            for (int y = 0; y <= gridHeight; y++)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = y * GridSize,
                    X2 = totalWidth,
                    Y2 = y * GridSize,
                    Stroke = GetGridLineBrush(y),
                    StrokeThickness = GetGridLineThickness(y)
                };
                
                MainCanvas.Children.Add(line);
            }
            
            // Draw coordinate labels every 10 meters
            for (int x = 0; x <= gridWidth; x += GridSeperatorSize)
            {
                TextBlock label = new TextBlock
                {
                    Text = $"{x}m",
                    Foreground = Brushes.LightGray,
                    FontSize = 10
                };
                Canvas.SetLeft(label, x * GridSize + 2);
                Canvas.SetTop(label, 2);
                MainCanvas.Children. Add(label);
            }
            
            for (int y = 10; y <= gridHeight; y += GridSeperatorSize)
            {
                TextBlock label = new TextBlock
                {
                    Text = $"{y}m",
                    Foreground = Brushes. LightGray,
                    FontSize = 10
                };
                Canvas.SetLeft(label, 2);
                Canvas.SetTop(label, y * GridSize + 2);
                MainCanvas.Children.Add(label);
            }
        }
        
        private Brush GetGridLineBrush(int index)
        {
            // Every 10m line is brighter
            if (index % GridSeperatorSize == 0)
                return new SolidColorBrush(Color.FromRgb(80, 80, 80));
            else
                return new SolidColorBrush(Color.FromRgb(50, 50, 50));
        }
        
        private double GetGridLineThickness(int index)
        {
            return index % GridSeperatorSize == 0 ?  1.0 : 0.5;
        }

        private void CenterView()
        {
            double canvasWidth = MainCanvas.Width * currentZoom;
            double canvasHeight = MainCanvas.Height * currentZoom;
            
            TranslateTransform.X = (CanvasContainer.ActualWidth - canvasWidth) / 2;
            TranslateTransform.Y = (CanvasContainer. ActualHeight - canvasHeight) / 2;
        }

        // Zoom functionality
        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomDelta = e.Delta > 0 ? ZoomSpeed : -ZoomSpeed;
            double newZoom = Math.Clamp(currentZoom + zoomDelta, MinZoom, MaxZoom);
            
            if (newZoom != currentZoom)
            {
                Point mousePos = e.GetPosition(CanvasContainer);
                
                // Calculate point under mouse in canvas coordinates
                double canvasX = (mousePos.X - TranslateTransform.X) / currentZoom;
                double canvasY = (mousePos.Y - TranslateTransform.Y) / currentZoom;
                
                // Apply new zoom
                currentZoom = newZoom;
                ScaleTransform.ScaleX = currentZoom;
                ScaleTransform.ScaleY = currentZoom;
                
                // Adjust translation to keep point under mouse
                TranslateTransform.X = mousePos.X - canvasX * currentZoom;
                TranslateTransform. Y = mousePos.Y - canvasY * currentZoom;
                
                UpdateZoomDisplay();
                UpdateCoordinateDisplay(e.GetPosition(MainCanvas));
            }
            
            e.Handled = true;
        }

        // Pan functionality
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState. Pressed)
            {
                isPanning = true;
                lastMousePosition = e.GetPosition(CanvasContainer);
                MainCanvas.Cursor = Cursors.Hand;
                MainCanvas. CaptureMouse();
                e.Handled = true;
            }
            
            if (e. LeftButton == MouseButtonState. Pressed && isPlacingMode && selectedBuildingType != null)
            {
                Point clickPosition = e.GetPosition(MainCanvas);
                Point gridPosition = SnapToGrid(clickPosition);
        
                // Create and place building
                Building newBuilding = new Building(selectedBuildingType, gridPosition);
                buildings.Add(newBuilding);
        
                BuildingVisual visual = new BuildingVisual(newBuilding);
                Canvas.SetLeft(visual, gridPosition.X * GridSize);
                Canvas.SetTop(visual, gridPosition.Y * GridSize);
                MainCanvas.Children.Add(visual);
        
                e.Handled = true;
                return;
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point canvasPos = e.GetPosition(MainCanvas);
            UpdateCoordinateDisplay(canvasPos);
            
            if (isPlacingMode)
            {
                UpdateBuildingPreview(canvasPos);
            }
            
            if (isPanning)
            {
                Point currentPosition = e.GetPosition(CanvasContainer);
                Vector delta = currentPosition - lastMousePosition;
                
                TranslateTransform.X += delta.X;
                TranslateTransform.Y += delta. Y;
                
                lastMousePosition = currentPosition;
                e.Handled = true;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                MainCanvas. Cursor = Cursors.Arrow;
                MainCanvas.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                MainCanvas.ReleaseMouseCapture();
                MainCanvas.Cursor = Cursors.Arrow;
            }
        }

        private void UpdateCoordinateDisplay(Point canvasPosition)
        {
            double meterX = canvasPosition.X / GridSize;
            double meterY = canvasPosition.Y / GridSize;
            CoordinatesText.Text = $"X: {meterX:F1}m, Y: {meterY:F1}m";
        }

        private void UpdateZoomDisplay()
        {
            ZoomText.Text = $"Zoom: {currentZoom * 100:F0}%";
        }

        // Helper method to add objects to the grid (for future use)
        public void AddBuildingToGrid(double xMeters, double yMeters, double widthMeters, double heightMeters, Brush color)
        {
            Rectangle building = new Rectangle
            {
                Width = widthMeters * GridSize,
                Height = heightMeters * GridSize,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            
            Canvas.SetLeft(building, xMeters * GridSize);
            Canvas.SetTop(building, yMeters * GridSize);
            
            MainCanvas.Children.Add(building);
        }
        
        private void EnterPlacementMode(BuildingType buildingType)
        {
            selectedBuildingType = buildingType;
            isPlacingMode = true;
            MainCanvas. Cursor = Cursors.Cross;
        }
        
        private Point SnapToGrid(Point position)
        {
            double x = Math.Round(position.X / GridSize);
            double y = Math.Round(position.Y / GridSize);
            return new Point(x, y);
        }
        
        private void UpdateBuildingPreview(Point mousePosition)
        {
            if (!isPlacingMode || selectedBuildingType == null) return;
    
            Point gridPosition = SnapToGrid(mousePosition);
    
            if (previewBuilding == null)
            {
                Building previewBuildingModel = new Building(selectedBuildingType, gridPosition);
                previewBuilding = new BuildingVisual(previewBuildingModel);
                previewBuilding.Opacity = 0.5;
                MainCanvas.Children.Add(previewBuilding);
            }
    
            Canvas.SetLeft(previewBuilding, gridPosition.X * GridSize);
            Canvas.SetTop(previewBuilding, gridPosition. Y * GridSize);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && isPlacingMode)
            {
                isPlacingMode = false;
                selectedBuildingType = null;
                if (previewBuilding != null)
                {
                    MainCanvas.Children.Remove(previewBuilding);
                    previewBuilding = null;
                }

                MainCanvas.Cursor = Cursors.Arrow;
            }

            //if (e.Key == Key.LeftAlt && e.Key == Key.LeftShift && e.Key == Key.R && e.Key == Key.T && e.Key == Key.N);
        }
        
        private void SelectBuilding_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string buildingId)
            {
                BuildingType type = BuildingTypes.GetById(buildingId);
                if (type != null)
                {
                    EnterPlacementMode(type);
                }
            }
        }

        private void CancelPlacement_Click(object sender, RoutedEventArgs e)
        {
            isPlacingMode = false;
            selectedBuildingType = null;
            if (previewBuilding != null)
            {
                MainCanvas.Children.Remove(previewBuilding);
                previewBuilding = null;
            }
            MainCanvas.Cursor = Cursors. Arrow;
        }
        
        private void PopulateBuildingToolbar()
        {
            BuildingToolBar.Items.Clear();
            
            Console.WriteLine(BuildingTypes.GetAll().Count);
            // Add button for each building type
            foreach (var buildingType in BuildingTypes.GetAll())
            {
                
                Console.WriteLine(buildingType.Name);
                Button btn = new Button
                {
                    Content = buildingType. Name,
                    Tag = buildingType. Id,
                    Padding = new Thickness(5),
                    Margin = new Thickness(2),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    MinWidth = 150
                };
                
                btn.Click += SelectBuilding_Click;
                BuildingToolBar.Items.Add(btn);
            }
        }
    }
}