# Conveyor Belt System Implementation

## Overview
This document describes the implementation of the conveyor belt system for the Satisfactory Planner application. The system allows users to create visual connections between output ports of one building and input ports of another building.

## Architecture

### 1. Model Layer

#### ConveyorBelt.cs (`Models/ConveyorBelt.cs`)
The core model class representing a conveyor belt connection.

**Key Features:**
- Stores references to source and target buildings and their ports
- Validates that source port is an Output and target port is an Input
- Provides computed properties for start and end positions in absolute coordinates
- Does NOT validate ResourceType compatibility (as per requirements)

**Properties:**
```csharp
- Id: string - Unique identifier
- SourceBuilding: Building - The building where the belt starts
- SourcePort: IOPort - The output port on the source building
- TargetBuilding: Building - The building where the belt ends
- TargetPort: IOPort - The input port on the target building
- StartPosition: Point - Computed absolute position of source port (in meters)
- EndPosition: Point - Computed absolute position of target port (in meters)
```

**Validation:**
- Constructor throws ArgumentException if source port is not PortType.Output
- Constructor throws ArgumentException if target port is not PortType.Input
- No validation on ResourceType compatibility (Solid/Fluid can be mixed)

### 2. Visual Layer

#### ConveyorBeltVisual.cs (`Controls/ConveyorBeltVisual.cs`)
Visual representation of a conveyor belt on the canvas.

**Key Features:**
- Extends WPF Polyline for smooth curved rendering
- Applies GridSize scaling (20 pixels = 1 meter)
- Creates a 3-point curve for visual appeal
- Color-coded based on source port's ResourceType:
  - Yellow: Solid resources
  - Cyan: Fluid resources
  - Orange: Other/unknown types

**Visual Properties:**
- StrokeThickness: 3 pixels
- Opacity: 0.8 (for better layering)
- Rounded caps and joins for smooth appearance
- Perpendicular curve offset (5% of line length)

#### BuildingVisual.cs Enhancement (`Controls/BuildingVisual.cs`)
Enhanced to support port interaction.

**Key Additions:**
- `PortClicked` event: Raised when a port is clicked
- `PortClickedEventArgs`: Event arguments containing Building and IOPort references
- Port shapes now have:
  - MouseLeftButtonDown event handlers
  - Cursor set to Hand for better UX
  - IOPort stored in Tag property for event handling

### 3. Application Layer

#### MainWindow.xaml.cs Enhancements
Main application window with conveyor belt management.

**New Fields:**
```csharp
- conveyorBelts: List<ConveyorBelt> - Collection of all conveyor belts
- isPlacingConveyor: bool - Flag indicating if in conveyor placement mode
- conveyorSourceBuilding: Building? - Temporary storage for selected source building
- conveyorSourcePort: IOPort? - Temporary storage for selected source port
```

**New Methods:**

1. `PlaceConveyor_Click()`: Enters conveyor placement mode
2. `BuildingVisual_PortClicked()`: Handles port click events
3. `StartConveyorPlacementFromPort()`: Validates and stores source port selection
4. `CompleteConveyorPlacementToPort()`: Validates target port and creates conveyor belt
5. `DrawConveyorBelt()`: Adds conveyor belt visual to canvas
6. `ResetConveyorPlacement()`: Resets placement state
7. `UpdateStatusText()`: Updates window title with status messages

**Validation Logic:**
- First click must be on an Output port
- Second click must be on an Input port
- Cannot connect a port to itself
- Shows MessageBox for invalid selections
- Continues in placement mode after successful creation (allows multiple belts)

**UI Integration:**
- "Place Conveyor Belt" button added to BuildingToolBar
- Status messages shown in window title
- ESC key cancels conveyor placement mode
- Port click events automatically wired when buildings are placed

## User Workflow

### Placing a Conveyor Belt

1. Click "Place Conveyor Belt" button in the toolbar
2. Window title changes to: "Conveyor Placement: Click on an OUTPUT port to start"
3. Click on any output port (green circle) on any building
4. Window title changes to: "Conveyor Placement: Now click on an INPUT port to complete the connection"
5. Click on any input port (red circle) on any building
6. Conveyor belt is created and rendered on canvas
7. Mode stays active for placing more belts
8. Press ESC to exit placement mode

### Visual Feedback

- Output ports: Green stroke, hand cursor on hover
- Input ports: Red/OrangeRed stroke, hand cursor on hover
- Conveyor belts: Colored line with curve (yellow for solids, cyan for fluids)
- Status messages in window title guide the user through the process
- Error messages via MessageBox for invalid actions

### Error Handling

**Scenario 1: Click on Input port first**
- Action: User clicks input port when selecting source
- Result: MessageBox "First port must be an OUTPUT port!"
- State: Placement mode continues, waiting for output port

**Scenario 2: Click on Output port second**
- Action: User clicks output port when selecting target
- Result: MessageBox "Second port must be an INPUT port!"
- State: Placement resets, waiting for new output port

**Scenario 3: Click same port twice**
- Action: User clicks same port as source and target
- Result: MessageBox "Cannot connect a port to itself!"
- State: Placement resets, waiting for new output port

## Technical Notes

### Coordinate System
- All positions in the model are in meters
- Visual rendering multiplies by GridSize (20 pixels/meter)
- Port positions are relative to building position
- GetPortAbsolutePosition() combines building and port positions

### Z-Order
- Conveyor belts are inserted before building visuals on the canvas
- This ensures belts appear behind buildings for better visual clarity
- Insertion index is found by searching for first BuildingVisual

### Resource Type Handling
- Conveyor color is based on source port's ResourceType
- No validation that source and target ResourceTypes match
- This is intentional per requirements (allows mixed connections)

### Event Propagation
- Port click events set e.Handled = true to prevent bubbling
- This ensures only port clicks are processed, not canvas clicks
- Building placement and conveyor placement are mutually exclusive modes

## Future Enhancements (Not Implemented)

The following features are not part of the current implementation but could be added:

1. **Belt Deletion**: Right-click or select and delete belts
2. **Belt Highlighting**: Hover over belts to highlight them
3. **Building Movement**: Update belt visuals when buildings are moved
4. **Throughput Display**: Show item flow rate on belts
5. **Belt Validation**: Optional warning when connecting incompatible resources
6. **Multiple Belt Styles**: Different visuals for Mk.1, Mk.2, etc.
7. **Persistence**: Save/load conveyor belts with the factory layout

## Testing Recommendations

Since this is a WPF application that cannot be built on Linux, testing should be performed on a Windows machine:

1. **Basic Functionality**
   - Create belts between various building types
   - Verify visual rendering and colors
   - Test port click detection

2. **Validation**
   - Try to connect Input→Input (should fail)
   - Try to connect Output→Output (should fail)
   - Connect ports with different ResourceTypes (should succeed)
   - Try to connect port to itself (should fail)

3. **UI Flow**
   - Enter/exit placement mode with button and ESC
   - Create multiple belts in sequence
   - Switch between building and conveyor placement modes

4. **Visual Quality**
   - Verify belt curves render smoothly
   - Check colors match resource types
   - Ensure proper layering (belts behind buildings)
   - Test with various zoom levels

## Code Quality

- All new code follows C# naming conventions
- XML documentation comments on public classes and methods
- Clear separation of concerns (Model/View/Application layers)
- Consistent with existing codebase style
- No hardcoded "magic numbers" (GridSize constant used consistently)
