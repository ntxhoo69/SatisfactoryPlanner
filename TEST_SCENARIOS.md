# Manual Test Scenarios for Conveyor Belt System

This document provides step-by-step test scenarios to validate the conveyor belt implementation on a Windows machine.

## Prerequisites
- Windows machine with .NET 9 SDK installed
- Visual Studio 2022 or Rider IDE
- Clone and build the SatisfactoryPlanner project

## Test Scenario 1: Basic Conveyor Belt Creation

### Steps:
1. Launch the application
2. Place a "Miner Mk. 1" building on the grid (has 1 output port)
3. Place a "Smelter" building on the grid (has 1 input port)
4. Click the "Place Conveyor Belt" button in the toolbar
5. Verify window title shows: "Conveyor Placement: Click on an OUTPUT port to start"
6. Click on the green output port of the Miner
7. Verify window title changes to: "Conveyor Placement: Now click on an INPUT port..."
8. Click on the red input port of the Smelter
9. Verify a yellow curved line appears connecting the two ports
10. Verify the window title indicates success

### Expected Results:
✅ Yellow conveyor belt rendered with smooth curve
✅ Belt connects exactly at port positions
✅ Belt visible behind buildings (proper z-order)
✅ Status messages guide user correctly

---

## Test Scenario 2: Port Type Validation - Output First

### Steps:
1. Click "Place Conveyor Belt" button
2. Try clicking on an INPUT port (red circle) first
3. Verify MessageBox appears: "First port must be an OUTPUT port!"
4. Click OK on the MessageBox
5. Verify you're still in placement mode
6. Click on an OUTPUT port (green circle)
7. Verify placement continues to second step

### Expected Results:
✅ Error message shown for invalid first port
✅ Placement mode continues after error
✅ Can successfully complete after fixing mistake

---

## Test Scenario 3: Port Type Validation - Input Second

### Steps:
1. Click "Place Conveyor Belt" button
2. Click on an OUTPUT port (green circle)
3. Try clicking on another OUTPUT port instead of INPUT
4. Verify MessageBox appears: "Second port must be an INPUT port!"
5. Click OK
6. Verify placement resets and asks for output port again

### Expected Results:
✅ Error message shown for invalid second port
✅ Placement resets to initial state
✅ User must start over with output port selection

---

## Test Scenario 4: Same Port Validation

### Steps:
1. Place a "Storage Container" (has both input and output ports)
2. Click "Place Conveyor Belt" button
3. Click on the output port of the Storage Container
4. Try clicking on the same Storage Container (but the input port)
5. Actually, try clicking the SAME port twice if possible
6. Verify appropriate error handling

### Expected Results:
✅ Cannot create self-loops
✅ Clear error message shown
✅ Placement resets appropriately

---

## Test Scenario 5: Multiple Conveyor Belts

### Steps:
1. Place 3 buildings in a row: Miner → Smelter → Constructor
2. Click "Place Conveyor Belt" button
3. Create belt from Miner output to Smelter input
4. Without exiting placement mode, create second belt from Smelter output to Constructor input
5. Create third belt from Constructor output to another building
6. Verify all belts render correctly

### Expected Results:
✅ Multiple belts can be placed in sequence
✅ Placement mode stays active after each belt
✅ All belts render with correct layering
✅ No visual overlap issues

---

## Test Scenario 6: Resource Type Colors

### Steps:
1. Create belts connecting various building types
2. Observe belt colors for different resource types
3. Verify color coding:
   - Yellow: Solid resources
   - Cyan: Fluid resources (if available in building data)

### Expected Results:
✅ Belts colored based on source port resource type
✅ Colors clearly distinguishable
✅ Colors match the specification

---

## Test Scenario 7: Cancel Placement with ESC

### Steps:
1. Click "Place Conveyor Belt" button
2. Verify placement mode is active
3. Press ESC key
4. Verify placement mode exits
5. Verify cursor returns to normal
6. Verify window title returns to "Satisfactory Planner - Ready"

### Expected Results:
✅ ESC cancels placement immediately
✅ No partial state left over
✅ UI returns to normal state
✅ Can start new placement after cancel

---

## Test Scenario 8: Mixed Placement Modes

### Steps:
1. Click "Place Conveyor Belt" button to enter conveyor mode
2. Click a building type button to place a building
3. Verify conveyor mode is cancelled
4. Place the building
5. Now enter building placement mode
6. Press ESC
7. Enter conveyor mode
8. Press ESC

### Expected Results:
✅ Building and conveyor modes are mutually exclusive
✅ Starting one mode cancels the other
✅ ESC works correctly for both modes
✅ No state conflicts between modes

---

## Test Scenario 9: Visual Quality at Different Zoom Levels

### Steps:
1. Create several conveyor belts
2. Zoom in to 200% using mouse wheel
3. Verify belts render clearly
4. Zoom out to 50%
5. Verify belts still visible and clear
6. Pan around to different areas

### Expected Results:
✅ Belts scale properly with zoom
✅ Stroke thickness remains visible at all zoom levels
✅ Curves remain smooth at all zoom levels
✅ No visual artifacts or clipping

---

## Test Scenario 10: Complex Factory Layout

### Steps:
1. Create a complex layout with 10+ buildings
2. Connect them with 15+ conveyor belts
3. Verify all belts render correctly
4. Zoom and pan to inspect different areas
5. Try creating additional belts in the crowded layout

### Expected Results:
✅ Application remains responsive
✅ All belts visible and distinguishable
✅ No performance degradation
✅ Belts don't obscure port selection

---

## Regression Tests

### Test that existing functionality still works:

1. **Building Placement**
   - Can still place buildings normally
   - Preview works correctly
   - Snap to grid still functions

2. **Pan and Zoom**
   - Right-click pan still works
   - Mouse wheel zoom still works
   - Zoom centers on mouse position

3. **Port Visualization**
   - Ports still render on buildings
   - Port colors correct (red for input, green for output)
   - Port positions match building definition

---

## Known Limitations (Not Bugs)

These are expected behaviors per the requirements:

1. ✅ **ResourceType mixing allowed**: You can connect Solid output to Fluid input - this is intentional
2. ✅ **No belt deletion**: Right-click deletion not implemented in this phase
3. ✅ **No belt editing**: Cannot change belt endpoints after creation
4. ✅ **Static belts**: Belts don't update if buildings are moved (movement not implemented)

---

## Reporting Issues

If you find any issues during testing, please report:
- Exact steps to reproduce
- Expected vs actual behavior
- Screenshots if visual issue
- Any error messages or exceptions
