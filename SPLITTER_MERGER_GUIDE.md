# Splitter and Merger Guide

## Overview
This guide explains how to use the new Splitter and Merger buildings in the Satisfactory Planner.

## New Buildings

### Splitter
- **Size**: 4x4 meters
- **Color**: Teal
- **Ports**: 1 input, 3 outputs
- **Function**: Divides one conveyor belt input into up to 3 outputs

**How it works:**
- Takes input from one or more input belts (sums them if multiple)
- Divides the total flow equally among all connected output belts
- If only 1 output is connected: 100% of input goes to that output
- If 2 outputs are connected: 50% of input goes to each output
- If 3 outputs are connected: 33.3% of input goes to each output
- The item type is passed through from input to all outputs
- **Validation**: If multiple input belts have different item types, outputs are marked invalid (red)

**Example:**
```
Source (Iron Ore: 60/min) → Splitter → Output 1 (Iron Ore: 30/min)
                                     → Output 2 (Iron Ore: 30/min)
```

### Merger
- **Size**: 4x4 meters
- **Color**: DarkCyan
- **Ports**: 3 inputs, 1 output
- **Function**: Merges up to 3 conveyor belt inputs into one output

**How it works:**
- Takes input from 1-3 input belts
- Sums all inputs into a single output
- The item type from the first input is used for the output
- **Validation**: All input belts must carry the same item type
  - If different item types are detected, output is marked invalid (red)
  - Invalid connections will show red coloring on the output belt

**Example:**
```
Source 1 (Iron Ore: 30/min) → Merger → Output (Iron Ore: 60/min)
Source 2 (Iron Ore: 30/min) →
```

## Usage Instructions

### Placing a Splitter
1. Click "Splitter" in the building toolbar
2. Place it on the grid
3. Connect input belt(s) to the input port (left side)
4. Connect output belts to the output ports (right side, top/middle/bottom)
5. The splitter automatically calculates and displays flow rates

### Placing a Merger
1. Click "Merger" in the building toolbar
2. Place it on the grid
3. Connect input belts to the input ports (left side, top/middle/bottom)
4. Connect output belt to the output port (right side)
5. The merger automatically calculates and displays flow rates

### Configuration
- Splitters and Mergers **do not require configuration**
- They work automatically based on connected belts
- Double-clicking shows an informational message

### Visual Feedback
- **Yellow belts**: Valid solid material connections
- **Cyan belts**: Valid fluid connections
- **Red belts**: Invalid connections (mixed item types)
- **Traffic labels**: Show item name and flow rate (e.g., "Iron Ore: 30.0/min")

## Example Production Chains

### Example 1: Splitting Iron Ore for Two Smelters
```
Miner (Iron Ore: 60/min) → Splitter → Smelter 1 (30/min) → Iron Ingot (15/min)
                                    → Smelter 2 (30/min) → Iron Ingot (15/min)
```

### Example 2: Merging Multiple Sources
```
Miner 1 (Iron Ore: 60/min) → Merger → Smelter (90/min) → Iron Ingot (45/min)
Miner 2 (Iron Ore: 30/min) →
```

### Example 3: Complex Distribution Network
```
Source (Iron Ore: 120/min) → Splitter 1 → Splitter 2 → Output 1 (30/min)
                                        |            → Output 2 (30/min)
                                        |
                                        → Merger → Output 3 (60/min)
                                               ↑
                                   Source 2 (Iron Ore: 0/min)
```

## Validation and Error Handling

### Valid Scenarios
✅ Splitter with single input, multiple outputs (same item)
✅ Splitter with multiple inputs of same item type
✅ Merger with multiple inputs of same item type
✅ Merger with single input
✅ Empty input belts (don't affect validation)

### Invalid Scenarios (Red Belts)
❌ Splitter with inputs of different item types
❌ Merger with inputs of different item types

Example of invalid merger:
```
Source 1 (Iron Ore: 60/min) → Merger → Output (INVALID - RED)
Source 2 (Copper Ore: 30/min) →
```
The output belt will be marked red because Iron Ore and Copper Ore cannot be merged.

## Tips and Best Practices

1. **Use Splitters for Load Balancing**
   - Split high-volume inputs across multiple production buildings
   - Ensure each building gets equal share of resources

2. **Use Mergers for Consolidation**
   - Combine outputs from multiple sources
   - Simplify belt routing by merging parallel lines

3. **Watch for Red Belts**
   - Red belts indicate invalid connections
   - Check that all inputs to a merger are the same item type
   - Check that all inputs to a splitter are the same item type

4. **Production Scaling**
   - Remember that existing production buildings already scale their output based on available inputs
   - A smelter with 50% of required input will produce 50% of normal output

5. **Flow Visualization**
   - Traffic labels show exactly how much is flowing through each belt
   - Use this to identify bottlenecks and balance your production

## Technical Details

### Splitter Algorithm
1. Sum all input belt flow rates (only same item type)
2. Check if all inputs have the same item type
3. Divide total by number of connected outputs
4. Set each output belt to carry the divided amount
5. Mark outputs as invalid if mixed item types detected

### Merger Algorithm
1. Identify the item type from first non-null input
2. Sum all input belts with matching item type
3. Check if any inputs have different item types
4. Set output to carry the total sum
5. Mark output as invalid if mixed item types detected

### Integration with Existing System
- Splitters and Mergers work seamlessly with the existing production calculation system
- They respect the same validation rules as other buildings
- They integrate with the conveyor belt traffic visualization
- They are included in the overall production recalculation when any change is made
