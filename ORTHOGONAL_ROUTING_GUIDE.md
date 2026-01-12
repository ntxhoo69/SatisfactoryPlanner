# Orthogonal Belt Routing - Implementation Guide

## Summary of Changes

This implementation adds "straight-mode" orthogonal routing for conveyor belts, similar to Satisfactory's in-game belt behavior.

### 1. Data Model Changes

#### New Dir Enum (IOPort.cs)
- `Dir.Up`: Port faces upward (negative Y direction)
- `Dir.Right`: Port faces right (positive X direction)
- `Dir.Down`: Port faces downward (positive Y direction)
- `Dir.Left`: Port faces left (negative X direction)

#### IOPort Class Updates
- Added `Facing` property (type: `Dir`)
- Constructor now accepts `facing` parameter with default value `Dir.Right`

#### Building Class Updates
- Added `GetPortFacing(IOPort port)` method
- Currently returns port's facing directly for rotation=0
- Designed to support rotation transformation in the future

### 2. Automatic Port Facing Calculation (BuildingTypes.cs)

The `ComputePortFacing` method automatically determines which direction a port should face based on its position on the building footprint:

**Algorithm:**
1. Calculate distance from port position to each edge (left, right, top, bottom)
2. Find the minimum distance
3. Assign facing direction based on closest edge:
   - Left edge (x ≈ 0) → `Dir.Left`
   - Right edge (x ≈ width) → `Dir.Right`
   - Top edge (y ≈ 0) → `Dir.Up`
   - Bottom edge (y ≈ height) → `Dir.Down`

**Example: Smelter (6m × 9m)**
- Input at (0, 4.5): distance to left = 0 → faces `Left`
- Input at (3, 0): distance to top = 0 → faces `Up`
- Output at (6, 4.5): distance to right = 0 → faces `Right`

### 3. Routing Algorithm (BeltRouter.cs)

The `BeltRouter.ComputeOrthogonalRoute` method creates orthogonal paths with the following steps:

**Input Parameters:**
- `startPx`: Start point in pixels
- `endPx`: End point in pixels
- `startFacing`: Direction belt exits source port
- `endFacing`: Direction belt enters target port
- `gridSize`: Pixels per meter (20)

**Algorithm Steps:**
1. **Grid Snapping**: Round coordinates to nearest grid unit for alignment
2. **Lead Points**: 
   - S1 = start + facing_vector * leadDistance (1 grid unit from source)
   - E1 = end + facing_vector * leadDistance (1 grid unit behind target)
3. **Orthogonal Path**: Build zig-zag path between S1 and E1
   - Horizontal-first if source faces Left/Right
   - Vertical-first if source faces Up/Down
4. **Assembly**: Combine points → [start, S1, corners, E1, end]
5. **Cleanup**: Remove duplicates and collinear points

**Path Structure Example:**
```
Source (Right-facing) → Target (Left-facing)

start → S1 → corner → E1 → end
  └────→    ↓        ←────┘
          └──────────┘
```

### 4. Visual Rendering (ConveyorBeltVisual.cs)

**Major Changes:**
- Changed from `Polyline` to `Canvas` containing a `Path`
- Uses `PathGeometry` with `LineSegment` instead of bezier curves
- Maintains same styling: colors, thickness, caps, opacity

**Rendering Process:**
1. Get port positions in meters from buildings
2. Get port facing directions from `Building.GetPortFacing()`
3. Convert positions to pixels
4. Call `BeltRouter.ComputeOrthogonalRoute()` to get path points
5. Create `PathGeometry` with `PathFigure` and `LineSegment`s
6. Apply styling based on resource type

### 5. Testing Instructions (Windows Only)

Since this is a WPF application, it must be tested on Windows:

**Build:**
```bash
dotnet build SatisfactoryPlanner.sln
```

**Run:**
```bash
dotnet run --project SatisfactoryPlanner/SatisfactoryPlanner.csproj
```

**Test Scenarios:**

1. **Same-Side Ports** (e.g., Storage Container)
   - Input on left (0, 2) facing Left
   - Output on right (4, 2) facing Right
   - Expected: Straight horizontal line with small lead segments

2. **Adjacent-Side Ports** (e.g., Smelter to Constructor)
   - Smelter output on right (6, 4.5) facing Right
   - Constructor input on left (0, 4) facing Left
   - Expected: L-shaped path with one 90° turn

3. **Opposite-Side Ports** (e.g., Miner to Refinery)
   - Miner output on right facing Right
   - Refinery input on left facing Left
   - Expected: Z-shaped path with two 90° turns

4. **Top/Bottom Ports**
   - Building with top port (facing Up)
   - Building with bottom port (facing Down)
   - Expected: Vertical segments with appropriate turns

**Visual Verification:**
- [ ] Belts should have no diagonal segments
- [ ] All turns should be 90° angles
- [ ] First segment should align with source port facing
- [ ] Last segment should align with target port facing
- [ ] Belt color should match resource type (Yellow=Solid, Cyan=Fluid)
- [ ] Path should have rounded caps and joins

### 6. Extensibility for Future Enhancements

**Building Rotation Support:**
- `Building.GetPortFacing()` has placeholder for rotation transformation
- To implement: rotate Dir enum by building rotation angle
  - 90° CW: Up→Right, Right→Down, Down→Left, Left→Up
  - 180°: Up→Down, Right→Left, Down→Up, Left→Right
  - 270° CW: Up→Left, Right→Up, Down→Right, Left→Down

**Collision Avoidance:**
- `BeltRouter.BuildOrthogonalPath()` can be extended to accept:
  - Occupancy grid: `bool[,]` indicating blocked cells
  - Callback: `Func<Point, bool>` to check if position is valid
- Current implementation uses simple heuristic (horizontal-first vs vertical-first)
- Can be replaced with A* or other pathfinding for obstacle avoidance

**Multiple Routing Options:**
- Store multiple valid routes and let user select
- Preview route before confirming connection
- Allow manual adjustment of corner positions

## Files Modified

1. `Models/IOPort.cs` - Added Dir enum and Facing property
2. `Models/Building.cs` - Added GetPortFacing method
3. `Models/BeltRouter.cs` - NEW: Routing algorithm utility class
4. `Data/BuildingTypes.cs` - Added ComputePortFacing logic
5. `Controls/ConveyorBeltVisual.cs` - Changed from Polyline to Canvas+Path with orthogonal routing

## No Breaking Changes

All changes are backward compatible:
- IOPort constructor has default `facing` parameter
- Existing building data (buildings.json) works without modification
- Port facing is computed automatically from position
- ConveyorBelt and Building classes maintain same public API
