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
using System.Linq;

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
        
        // Building selection
        private Building? selectedBuilding = null;
        private BuildingVisual? selectedBuildingVisual = null;
        
        // Conveyor belt management
        private List<ConveyorBelt> conveyorBelts = new List<ConveyorBelt>();
        private ConveyorBeltVisual? selectedConveyorVisual = null;
        private bool isPlacingConveyor = false;
        private Building? conveyorSourceBuilding = null;
        private IOPort? conveyorSourcePort = null;

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
        
                // Create and place building with current rotation
                Building newBuilding = new Building(selectedBuildingType, gridPosition);
                if (previewBuilding != null)
                {
                    newBuilding.Rotation = previewBuilding.Building.Rotation;
                }
                buildings.Add(newBuilding);
        
                BuildingVisual visual = new BuildingVisual(newBuilding);
                visual.PortClicked += BuildingVisual_PortClicked; // Subscribe to port click events
                visual.MouseLeftButtonDown += PlacedBuilding_MouseLeftButtonDown; // Subscribe to building click events
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
            Canvas.SetTop(previewBuilding, gridPosition.Y * GridSize);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                // Cancel building placement
                if (isPlacingMode)
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
                
                // Cancel conveyor placement
                if (isPlacingConveyor)
                {
                    isPlacingConveyor = false;
                    conveyorSourceBuilding = null;
                    conveyorSourcePort = null;
                    MainCanvas.Cursor = Cursors.Arrow;
                    UpdateStatusText("Ready");
                }
                
                // Deselect building/conveyor
                if (selectedBuilding != null || selectedConveyorVisual != null)
                {
                    DeselectAll();
                }
            }
            
            // Handle R key for rotation during placement
            if (e.Key == Key.R && isPlacingMode && previewBuilding != null)
            {
                previewBuilding.Building.Rotate();
                previewBuilding.UpdateVisual();
                e.Handled = true;
            }
            
            // Handle Backspace for deletion
            if (e.Key == Key.Back)
            {
                if (selectedBuilding != null && selectedBuildingVisual != null)
                {
                    DeleteSelectedBuilding();
                    e.Handled = true;
                }
                else if (selectedConveyorVisual != null)
                {
                    DeleteSelectedConveyor();
                    e.Handled = true;
                }
            }
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
            
            // Add separator
            BuildingToolBar.Items.Add(new Separator());
            
            // Add conveyor belt button
            Button conveyorBtn = new Button
            {
                Content = "Place Conveyor Belt",
                Padding = new Thickness(5),
                Margin = new Thickness(2),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                MinWidth = 150,
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 50))
            };
            conveyorBtn.Click += PlaceConveyor_Click;
            BuildingToolBar.Items.Add(conveyorBtn);
        }
        
        // ========== Conveyor Belt Management Methods ==========
        
        /// <summary>
        /// Enters conveyor placement mode
        /// </summary>
        private void PlaceConveyor_Click(object sender, RoutedEventArgs e)
        {
            // Cancel any building placement
            if (isPlacingMode)
            {
                CancelPlacement_Click(sender, e);
            }
            
            isPlacingConveyor = true;
            conveyorSourceBuilding = null;
            conveyorSourcePort = null;
            MainCanvas.Cursor = Cursors.Cross;
            UpdateStatusText("Conveyor Placement: Click on an OUTPUT port to start");
        }
        
        /// <summary>
        /// Handles port click events from BuildingVisual
        /// </summary>
        private void BuildingVisual_PortClicked(object? sender, PortClickedEventArgs e)
        {
            if (!isPlacingConveyor) return;
            
            // First click - select source port
            if (conveyorSourceBuilding == null)
            {
                StartConveyorPlacementFromPort(e.Building, e.Port);
            }
            // Second click - select target port
            else
            {
                CompleteConveyorPlacementToPort(e.Building, e.Port);
            }
        }
        
        /// <summary>
        /// Starts conveyor placement from a port
        /// </summary>
        private void StartConveyorPlacementFromPort(Building building, IOPort port)
        {
            // Validate that source port is an Output
            if (port.Type != PortType.Output)
            {
                MessageBox.Show("First port must be an OUTPUT port!", "Invalid Port Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            conveyorSourceBuilding = building;
            conveyorSourcePort = port;
            UpdateStatusText("Conveyor Placement: Now click on an INPUT port to complete the connection");
        }
        
        /// <summary>
        /// Completes conveyor placement to a target port
        /// </summary>
        private void CompleteConveyorPlacementToPort(Building building, IOPort port)
        {
            if (conveyorSourceBuilding == null || conveyorSourcePort == null) return;
            
            // Validate that target port is an Input
            if (port.Type != PortType.Input)
            {
                MessageBox.Show("Second port must be an INPUT port!", "Invalid Port Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetConveyorPlacement();
                return;
            }
            
            // Prevent connecting a port to itself
            if (building == conveyorSourceBuilding && port == conveyorSourcePort)
            {
                MessageBox.Show("Cannot connect a port to itself!", "Invalid Connection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetConveyorPlacement();
                return;
            }
            
            try
            {
                // Create the conveyor belt
                ConveyorBelt belt = new ConveyorBelt(
                    conveyorSourceBuilding, 
                    conveyorSourcePort, 
                    building, 
                    port
                );
                
                conveyorBelts.Add(belt);
                DrawConveyorBelt(belt);
                
                // Recalculate production after adding belt
                RecalculateProduction();
                
                UpdateStatusText($"Conveyor Belt created! Click another OUTPUT port to place more, or press ESC to exit.");
                
                // Reset for next conveyor (but stay in placement mode)
                conveyorSourceBuilding = null;
                conveyorSourcePort = null;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Failed to create conveyor: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ResetConveyorPlacement();
            }
        }
        
        /// <summary>
        /// Draws a conveyor belt on the canvas
        /// </summary>
        private void DrawConveyorBelt(ConveyorBelt belt)
        {
            ConveyorBeltVisual visual = new ConveyorBeltVisual(belt);
            visual.MouseLeftButtonDown += ConveyorBelt_MouseLeftButtonDown;
            
            // Add to canvas - conveyor belts should be drawn before buildings
            // Find the first building visual and insert before it
            int insertIndex = 0;
            for (int i = 0; i < MainCanvas.Children.Count; i++)
            {
                if (MainCanvas.Children[i] is BuildingVisual)
                {
                    insertIndex = i;
                    break;
                }
            }
            
            MainCanvas.Children.Insert(insertIndex, visual);
        }
        
        /// <summary>
        /// Resets the conveyor placement state
        /// </summary>
        private void ResetConveyorPlacement()
        {
            conveyorSourceBuilding = null;
            conveyorSourcePort = null;
            if (isPlacingConveyor)
            {
                UpdateStatusText("Conveyor Placement: Click on an OUTPUT port to start");
            }
        }
        
        /// <summary>
        /// Updates the status text display
        /// </summary>
        private void UpdateStatusText(string text)
        {
            this.Title = $"Satisfactory Planner - {text}";
        }
        
        // ========== Building Selection and Deletion Methods ==========
        
        /// <summary>
        /// Handles click events on conveyor belts for selection
        /// </summary>
        private void ConveyorBelt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Don't select if we're in placement or conveyor mode
            if (isPlacingMode || isPlacingConveyor) return;
            
            if (sender is ConveyorBeltVisual visual)
            {
                SelectConveyorBelt(visual);
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// Selects a conveyor belt and highlights it
        /// </summary>
        private void SelectConveyorBelt(ConveyorBeltVisual visual)
        {
            // Deselect previous selection
            DeselectAll();
            
            selectedConveyorVisual = visual;
            visual.SetHighlight(true);
            
            UpdateStatusText("Selected: Conveyor Belt - Press Backspace to delete");
        }
        
        /// <summary>
        /// Handles click events on placed buildings for selection
        /// </summary>
        private void PlacedBuilding_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Don't select if we're in placement or conveyor mode
            if (isPlacingMode || isPlacingConveyor) return;
            
            if (sender is BuildingVisual visual)
            {
                SelectBuilding(visual);
                
                // Double-click to configure
                if (e.ClickCount == 2)
                {
                    ConfigureBuilding(visual.Building, visual);
                }
                
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// Selects a building and highlights it
        /// </summary>
        private void SelectBuilding(BuildingVisual visual)
        {
            // Deselect previous selection
            DeselectAll();
            
            selectedBuilding = visual.Building;
            selectedBuildingVisual = visual;
            visual.SetHighlight(true);
            
            UpdateStatusText($"Selected: {visual.Building.Type.Name} - Press Backspace to delete");
        }
        
        /// <summary>
        /// Deselects all selected items
        /// </summary>
        private void DeselectAll()
        {
            if (selectedBuildingVisual != null)
            {
                selectedBuildingVisual.SetHighlight(false);
                selectedBuildingVisual = null;
            }
            
            if (selectedConveyorVisual != null)
            {
                selectedConveyorVisual.SetHighlight(false);
                selectedConveyorVisual = null;
            }
            
            selectedBuilding = null;
            UpdateStatusText("Ready");
        }
        
        /// <summary>
        /// Deletes the currently selected building
        /// </summary>
        private void DeleteSelectedBuilding()
        {
            if (selectedBuilding == null || selectedBuildingVisual == null) return;
            
            // Remove any conveyor belts connected to this building
            var connectedBelts = conveyorBelts
                .Where(belt => belt.SourceBuilding.Id == selectedBuilding.Id || belt.TargetBuilding.Id == selectedBuilding.Id)
                .ToList();
                
            foreach (var belt in connectedBelts)
            {
                // Find and remove the visual
                var beltVisual = MainCanvas.Children.OfType<ConveyorBeltVisual>()
                    .FirstOrDefault(v => v.ConveyorBelt == belt);
                if (beltVisual != null)
                {
                    MainCanvas.Children.Remove(beltVisual);
                }
                conveyorBelts.Remove(belt);
            }
            
            // Remove building from data and visual
            buildings.Remove(selectedBuilding);
            MainCanvas.Children.Remove(selectedBuildingVisual);
            
            selectedBuilding = null;
            selectedBuildingVisual = null;
            
            UpdateStatusText("Building deleted");
            
            // Recalculate production after deletion
            RecalculateProduction();
        }
        
        /// <summary>
        /// Deletes the currently selected conveyor belt
        /// </summary>
        private void DeleteSelectedConveyor()
        {
            if (selectedConveyorVisual == null) return;
            
            conveyorBelts.Remove(selectedConveyorVisual.ConveyorBelt);
            MainCanvas.Children.Remove(selectedConveyorVisual);
            
            selectedConveyorVisual = null;
            UpdateStatusText("Conveyor belt deleted");
            
            // Recalculate production after deletion
            RecalculateProduction();
        }
        
        /// <summary>
        /// Configures a building (recipe selection or source configuration)
        /// </summary>
        private void ConfigureBuilding(Building building, BuildingVisual visual)
        {
            if (building.IsSource())
            {
                // Configure source node
                var dialog = new SourceConfigDialog(building);
                if (dialog.ShowDialog() == true)
                {
                    building.SourceItemName = dialog.ItemName;
                    building.SourceItemRate = dialog.ItemRate;
                    visual.UpdateVisual();
                    RecalculateProduction();
                    UpdateStatusText($"Source configured: {building.SourceItemName} at {building.SourceItemRate:F1}/min");
                }
            }
            else if (building.IsSplitter() || building.IsMerger())
            {
                // Splitters and Mergers don't need configuration - they work automatically
                MessageBox.Show($"{building.Type.Name} automatically handles item flow. No configuration needed.", 
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Select recipe
                var recipes = RecipeTypes.GetRecipesForBuilding(building.Type.Id);
                if (recipes.Count == 0)
                {
                    MessageBox.Show($"No recipes available for {building.Type.Name}.", "No Recipes", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var dialog = new RecipeSelectionDialog(building.Type.Id);
                if (dialog.ShowDialog() == true && dialog.SelectedRecipe != null)
                {
                    building.SelectedRecipe = dialog.SelectedRecipe;
                    visual.UpdateVisual();
                    RecalculateProduction();
                    UpdateStatusText($"Recipe selected: {building.SelectedRecipe.Name}");
                }
            }
        }
        
        /// <summary>
        /// Recalculates production for all buildings and conveyor belts
        /// </summary>
        private void RecalculateProduction()
        {
            ProductionCalculator.RecalculateAll(buildings, conveyorBelts);
            
            // Update all conveyor belt visuals
            foreach (var visual in MainCanvas.Children.OfType<ConveyorBeltVisual>())
            {
                visual.UpdateVisual();
            }
        }
    }
}