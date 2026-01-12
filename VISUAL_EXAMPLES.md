# Orthogonal Belt Routing - Visual Examples

This document provides visual ASCII examples of how the orthogonal belt routing works.

## Example 1: Horizontal Belt (Storage to Storage)

```
Building A (Storage Container 4m×4m)          Building B (Storage Container 4m×4m)
Position: (10, 10)                            Position: (20, 10)
┌─────────────────┐                           ┌─────────────────┐
│                 │                           │                 │
│   Storage       │                           │   Storage       │
│                 │                           │                 │
│             [O]─┼────────────────────────→┼→[I]              │
│   Out (4, 2)    │                           │  In (0, 2)      │
│   Facing: Right │                           │  Facing: Left   │
│                 │                           │                 │
└─────────────────┘                           └─────────────────┘

Path Points (in pixels, GridSize=20):
1. Start: (240, 220)      - Output port at (12, 11) = (12*20, 11*20)
2. S1:    (260, 220)      - Lead point 1 grid unit Right
3. E1:    (380, 220)      - Lead point 1 grid unit Left of input (20*20 - 20)
4. End:   (400, 220)      - Input port at (20, 11) = (20*20, 11*20)

Result: Straight horizontal line
```

## Example 2: L-Shaped Belt (Smelter to Constructor)

```
Building A (Smelter 6m×9m)                    
Position: (10, 10)                            
┌───────────────────┐                         
│                   │                         
│                   │                         
│     Smelter       │                         
│                   │                         
│               [O]─┼──→                      
│  Out (6, 4.5)     │   │                     Building B (Constructor 8m×8m)
│  Facing: Right    │   │                     Position: (20, 20)
│                   │   │                     ┌───────────────────┐
└───────────────────┘   │                     │                   │
                        │                     │   Constructor     │
                        │                     │                   │
                        └────────────────────→┼→[I]              │
                                              │  In (0, 4)        │
                                              │  Facing: Left     │
                                              │                   │
                                              └───────────────────┘

Path Points (in pixels):
1. Start: (220, 290)      - Smelter output at (16, 14.5)
2. S1:    (240, 290)      - Lead point Right
3. C1:    (240, 480)      - Corner: horizontal to (20, 14.5), vertical to (20, 24)
4. E1:    (380, 480)      - Lead point Left of Constructor input
5. End:   (400, 480)      - Constructor input at (20, 24)

Result: L-shape with one 90° turn
```

## Example 3: Z-Shaped Belt (Opposite Facing Ports)

```
Building A (Miner 6m×6m)
Position: (10, 10)
┌─────────────────┐
│                 │
│     Miner       │
│                 │
│             [O]─┼──→
│ Out (6, 3)      │   │
│ Facing: Right   │   │
└─────────────────┘   │
                      │
                      └────────→  (horizontal segment 1)
                                │
                                ↓  (vertical segment down)
                                │
                      ┌─────────┘
                      │
                      └────────────────────────→  (horizontal segment 2)
                                                │
Building B (Assembler 10m×10m)                 │
Position: (25, 20)                              │
┌────────────────────────┐                      │
│                        │                      │
│      Assembler         │                      │
│                        │                      │
┼←[I]                    │←─────────────────────┘
│ In (0, 3)              │
│ Facing: Left           │
│                        │
└────────────────────────┘

Path Points:
1. Start: (220, 260)      - Miner output
2. S1:    (240, 260)      - Lead Right
3. C1:    (240, 460)      - Corner 1 (turn down)
4. C2:    (480, 460)      - Corner 2 (turn right)
5. E1:    (480, 460)      - Lead Left (happens to align with C2)
6. End:   (500, 460)      - Assembler input

Result: Z-shape or N-shape with two 90° turns
```

## Example 4: Vertical Belt (Top Port to Side Port)

```
Building A (Constructor 8m×8m)
Position: (10, 5)
┌───────────────────┐
│       [O]         │  ← Out (4, 0), Facing: Up
│        ↑          │
│        │          │
│   Constructor     │
│                   │
│                   │
│                   │
└───────────────────┘
        │
        │  (vertical segment up)
        │
        └─────→  (turn right)
              │
              └─────────────→  (horizontal segment)
                            │
Building B (Smelter 6m×9m)  │
Position: (20, 10)           │
┌───────────────────┐        │
┼←[I]               │←───────┘
│ In (0, 4.5)       │
│ Facing: Left      │
│                   │
│     Smelter       │
│                   │
│                   │
└───────────────────┘

Path Points:
1. Start: (180, 100)      - Constructor top output
2. S1:    (180, 80)       - Lead Up
3. C1:    (380, 80)       - Corner (turn right)
4. E1:    (380, 290)      - Lead before Smelter input
5. End:   (400, 290)      - Smelter input

Result: Inverted L-shape with vertical segment
```

## Port Facing Calculation Examples

### Building: Smelter (6m width × 9m height)

```
      0    1    2    3    4    5    6  (X in meters)
    0 ┌────────────┬────────────────┐
      │            │[I] Power       │  Port at (3, 0): dist_top=0 → Facing: Up
      │            │Facing: Up      │
    1 │            │                │
      │            │                │
    2 │            │                │
      │            │                │
    3 │            │                │
      │            │                │
    4 │[I] Ore In  │                │  Port at (0, 4.5): dist_left=0 → Facing: Left
      │Facing:Left │                │
    5 │            │                │[O] Ingot  Port at (6, 4.5): dist_right=0 → Facing: Right
      │            │                │Facing:Right
    6 │            │                │
      │            │                │
    7 │            │                │
      │            │                │
    8 │            │                │
      │            │                │
    9 └────────────┴────────────────┘
```

**Calculation for each port:**
1. **Power (3, 0):**
   - dist_left = |3 - 0| = 3
   - dist_right = |3 - 6| = 3
   - dist_top = |0 - 0| = 0 ← minimum
   - dist_bottom = |0 - 9| = 9
   - **Result: Dir.Up**

2. **Ore In (0, 4.5):**
   - dist_left = |0 - 0| = 0 ← minimum
   - dist_right = |0 - 6| = 6
   - dist_top = |4.5 - 0| = 4.5
   - dist_bottom = |4.5 - 9| = 4.5
   - **Result: Dir.Left**

3. **Ingot Out (6, 4.5):**
   - dist_left = |6 - 0| = 6
   - dist_right = |6 - 6| = 0 ← minimum
   - dist_top = |4.5 - 0| = 4.5
   - dist_bottom = |4.5 - 9| = 4.5
   - **Result: Dir.Right**

## Routing Algorithm Flow

```
INPUT:
  startPx: (220, 290)  - pixel position of source port
  endPx:   (400, 480)  - pixel position of target port
  startFacing: Dir.Right
  endFacing:   Dir.Left
  gridSize: 20

STEP 1: Snap to grid (optional, helps alignment)
  start = (220, 280)  [rounded to nearest 20]
  end   = (400, 480)

STEP 2: Create lead points
  leadDistance = 20 * 1.0 = 20 pixels
  
  FacingVector(Dir.Right) = (1, 0)
  S1 = (220, 280) + (1, 0) * 20 = (240, 280)
  
  FacingVector(Dir.Left) = (-1, 0)
  E1 = (400, 480) + (-1, 0) * 20 = (380, 480)

STEP 3: Build orthogonal path between S1 and E1
  startFacing = Right → use horizontal-first
  
  Corner1 = (E1.X, S1.Y) = (380, 280)
  
  Path: S1(240,280) → C1(380,280) → E1(380,480)

STEP 4: Assemble final path
  [start, S1, C1, E1, end]
  = [(220,280), (240,280), (380,280), (380,480), (400,480)]

STEP 5: Clean up
  - Remove duplicates: None
  - Remove collinear: None (all corners are actual turns)
  
FINAL PATH: 5 points creating L-shape
```

## Coordinate Systems

```
METERS (World Space):          PIXELS (Canvas Space):
                                  
0   10  20  30  40  (meters)   0    200  400  600  800  (pixels)
┌────┬───┬───┬───┬───         ┌─────┬────┬────┬────┬────
│    │   │   │   │            │     │    │    │    │
│    │   │   │   │            │     │    │    │    │
10   │   │   │   │            200   │    │    │    │
├────┼───┼───┼───┼───         ├─────┼────┼────┼────┼────
│    │ B │   │   │            │     │ B  │    │    │
│    │   │   │   │            │     │    │    │    │
20   │   │   │   │            400   │    │    │    │
├────┼───┼───┼───┼───         ├─────┼────┼────┼────┼────

Conversion: pixels = meters * GridSize
           meters = pixels / GridSize
           GridSize = 20 pixels/meter

Example:
  Building at (10, 10) meters
  → Canvas position: (200, 200) pixels
  
  Port at relative (0, 2) meters
  → Absolute (10, 12) meters
  → Canvas (200, 240) pixels
```

## Legend

```
Symbols Used:
┌─┐  Building outline
│ │
└─┘

[O]  Output port
[I]  Input port

─→   Belt segment going right
←─   Belt segment going left
│    Belt segment going down
↑    Belt segment going up
│
└─   90° turn (corner)
```

## Summary

The orthogonal routing system ensures:
1. ✅ Belts exit ports in the correct facing direction
2. ✅ All segments are horizontal or vertical (no diagonals)
3. ✅ All turns are exactly 90°
4. ✅ Belts enter target ports from the correct direction
5. ✅ Clean, efficient paths with minimal unnecessary points
6. ✅ Grid-aligned for a professional, structured appearance
