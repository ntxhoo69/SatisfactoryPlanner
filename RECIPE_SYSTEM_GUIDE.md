# Conveyor Belt Traffic & Recipe System - User Guide

## Overview
This implementation adds a complete production planning system to the Satisfactory Planner, including:
- Recipe selection for buildings
- Production rate calculation
- Conveyor belt traffic display
- Input/output validation
- Source nodes for material input

## Features Implemented

### 1. Recipe System
- **Recipe Database**: Added `recipes.json` with 14 common Satisfactory recipes
- **Recipe Selection**: Double-click any building to select a recipe
- **Visual Feedback**: Selected recipe name shown on building

### 2. Source Nodes
- **Item Source Building**: New special building type (light green)
- **Configuration**: Double-click to set item type and production rate
- **Default Rate**: 60 items/minute (configurable)

### 3. Production Calculation
- **Automatic Calculation**: Production rates calculated based on inputs
- **Input Validation**: Buildings only produce when inputs are sufficient
- **Proportional Output**: If inputs are 50% of required, output is 50% of normal

### 4. Conveyor Belt Traffic Display
- **Traffic Labels**: Show item name and flow rate on each belt
- **Format**: "Item Name: XX.X/min"
- **Position**: Displayed at belt midpoint
- **Auto-hide**: Hidden when no traffic

### 5. Visual Validation
- **Valid Belts**: Yellow (solid) or Cyan (fluid) coloring
- **Invalid Belts**: Red coloring when item type doesn't match recipe needs
- **Error Detection**: Highlights mismatched connections

## How to Use

### Setting Up a Production Chain

#### Step 1: Place a Source Node
1. Click "Item Source" in the toolbar
2. Place it on the grid
3. Double-click the source node
4. Enter item name (e.g., "Iron Ore")
5. Enter production rate (e.g., 60)
6. Click OK

#### Step 2: Place Production Buildings
1. Click desired building (e.g., "Smelter")
2. Place it on the grid
3. Double-click the building
4. Select a recipe from the list
5. Click OK

#### Step 3: Connect with Conveyor Belts
1. Click "Place Conveyor Belt"
2. Click on an OUTPUT port (green)
3. Click on an INPUT port (red)
4. Belt appears with traffic information

#### Step 4: Verify Production
- Check conveyor belt labels for item flow
- Red belts indicate mismatched connections
- Traffic shows actual production rates

### Example Production Chain

**Iron Plate Production:**
1. Source Node → outputs "Iron Ore" at 60/min
2. Connect to Smelter (Iron Ingot recipe)
   - Smelter outputs 30 Iron Ingot/min (60 ore ÷ 2 seconds × 60 = 30/min)
3. Connect Smelter to Constructor (Iron Plate recipe)
   - Constructor needs 30 Iron Ingot/min
   - Constructor outputs 20 Iron Plate/min

**Multi-Input Example (Reinforced Iron Plate):**
1. Source 1 → "Iron Plate" at 60/min
2. Source 2 → "Screw" at 120/min
3. Connect both to Assembler (Reinforced Iron Plate recipe)
   - Requires: 30 Iron Plate/min, 60 Screw/min
   - With 60 plate & 120 screw: Runs at 100% → 5 Reinforced Plate/min

**Insufficient Input Example:**
1. Source → "Iron Plate" at 30/min (half of required)
2. Connect to Assembler (needs 60/min)
   - Assembler runs at 50% capacity
   - Output: 2.5 Reinforced Plate/min instead of 5/min

## Available Recipes

### Smelter
- Iron Ingot: Iron Ore → Iron Ingot (30/min)
- Copper Ingot: Copper Ore → Copper Ingot (30/min)

### Constructor
- Iron Rod: Iron Ingot → Iron Rod (15/min)
- Iron Plate: Iron Ingot → Iron Plate (20/min)
- Screw: Iron Rod → Screw (40/min)
- Wire: Copper Ingot → Wire (30/min)
- Cable: Wire → Cable (30/min)
- Steel Beam: Steel Ingot → Steel Beam (15/min)
- Steel Pipe: Steel Ingot → Steel Pipe (20/min)
- Concrete: Limestone → Concrete (15/min)

### Assembler
- Reinforced Iron Plate: Iron Plate + Screw → Reinforced Iron Plate (5/min)
- Rotor: Iron Rod + Screw → Rotor (4/min)
- Modular Frame: Reinforced Iron Plate + Iron Rod → Modular Frame (2/min)

### Foundry
- Steel Ingot: Iron Ore + Coal → Steel Ingot (45/min)

## Controls

### Building Management
- **Place Building**: Click building in toolbar, click on grid
- **Rotate Building**: Press R during placement
- **Select Building**: Left-click placed building
- **Configure Building**: Double-click placed building
- **Delete Building**: Select building, press Backspace

### Conveyor Belt Management
- **Place Belt**: Click "Place Conveyor Belt", click output port, click input port
- **Select Belt**: Left-click belt
- **Delete Belt**: Select belt, press Backspace
- **Cancel Placement**: Press Escape

### Navigation
- **Pan**: Right-click and drag
- **Zoom**: Mouse wheel
- **Center View**: (Feature available at startup)

## Visual Indicators

### Building Colors
- **Light Green**: Source node
- **Orange/Red**: Smelter/Foundry
- **Gray**: Constructor
- **Slate Gray**: Assembler
- **Yellow Highlight**: Selected building

### Conveyor Belt Colors
- **Yellow**: Solid materials (valid)
- **Cyan**: Fluids (valid)
- **Red**: Invalid connection (wrong item type)

### Text Labels
- **Building Name**: White text (center of building)
- **Recipe Name**: Light green text (below name)
- **Traffic Info**: White text on black background (belt midpoint)

## Technical Details

### Production Calculation
The system calculates production using these formulas:

1. **Recipe Rate** = (Quantity ÷ Crafting Time) × 60
   - Example: Iron Plate = (2 ÷ 6) × 60 = 20/min

2. **Production Multiplier** = MIN(Available Input ÷ Required Input) for all inputs
   - Example: If need 60/min but have 30/min → multiplier = 0.5

3. **Actual Output** = Recipe Output × Production Multiplier
   - Example: 20/min × 0.5 = 10/min actual output

### Validation Rules
- Output port can only connect to Input port
- Source buildings don't require recipes (configured directly)
- Production buildings require recipe selection
- Belts turn red when item type doesn't match recipe inputs
- Buildings with no recipe produce no output

## Troubleshooting

### "No traffic" on belt
- Check if source building is configured
- Check if production building has recipe selected
- Verify sufficient inputs for production

### Red colored belt
- Item type doesn't match recipe requirements
- Check recipe inputs vs. connected outputs
- Reconfigure source or change recipe

### No output from building
- Building may not have recipe selected
- Insufficient input materials
- Check input belt traffic values

### Dialog doesn't appear
- Make sure to double-click (not single click)
- Check that building is selected (yellow highlight)

## Future Enhancements
Potential additions for future versions:
- Save/load production layouts
- Production statistics panel
- Alternative recipes
- Overclocking support
- Belt speed limits (Mk.1-5)
- Power consumption tracking
- Building efficiency indicators
