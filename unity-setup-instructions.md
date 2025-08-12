# Unity Project Setup Instructions

## Creating the Unity Project

Since Unity projects can't be created via command line, follow these steps:

### 1. Unity Hub Setup
1. Open Unity Hub
2. Click "New Project"
3. Select **Unity 2022.3.50f1** (LTS)
4. Choose **3D (URP)** template for mobile optimization
5. Set Project Name: **Dice Rolling App**
6. Set Location: `/Users/markb/dev/dice`
7. Click "Create Project"

### 2. Required Packages Installation

Once Unity opens, install these packages via Window → Package Manager:

#### Essential Packages:
- **Input System** (com.unity.inputsystem) - For touch controls
- **iOS Build Support** - For iPhone deployment
- **Universal RP** (already included with URP template)

#### Optional Packages:
- **Unity Remote 5** - For testing touch input in editor
- **Recorder** - For creating promotional videos
- **Analytics** - For usage tracking

### 3. iOS Build Configuration

Navigate to **File → Build Settings**:
1. Select **iOS** platform
2. Click "Switch Platform"
3. Click "Player Settings"

#### iOS Player Settings:
- **Company Name**: Your company name
- **Product Name**: Dice Rolling App
- **Bundle Identifier**: com.yourcompany.dicerollingapp
- **Version**: 0.1.0
- **Minimum iOS Version**: 13.0
- **Target Device Family**: iPhone & iPad
- **Behavior in Background**: Exit (recommended for physics apps)
- **Architecture**: ARM64

#### Graphics Settings:
- **Color Space**: Linear (for better visual quality)
- **Graphics APIs**: Metal (iOS default)
- **Rendering Path**: Universal Render Pipeline

### 4. Project Structure Setup

The folder structure has been created automatically. After Unity generates the project:

1. Copy all files from `Assets/Scripts/` folders into your Unity project
2. The scripts are organized in namespaces and should compile without errors
3. Create Physics Materials in `Assets/Materials/Physics/`
4. Set up scene hierarchy as described below

### 5. Scene Setup

#### Create Main Scene:
1. Create new scene: **DiceGame.unity**
2. Save in `Assets/Scenes/`

#### Scene Hierarchy:
```
DiceGame
├── Main Camera
├── Directional Light
├── GameManager (Empty GameObject)
│   └── GameManager.cs script
├── DiceRoom (Empty GameObject)
│   ├── Floor (Cube, scaled and positioned)
│   ├── WallNorth (Cube, scaled and positioned)
│   ├── WallSouth (Cube, scaled and positioned) 
│   ├── WallEast (Cube, scaled and positioned)
│   └── WallWest (Cube, scaled and positioned)
├── Dice (Empty GameObject)
│   ├── Die1 (Cube with DiceController.cs)
│   └── Die2 (Cube with DiceController.cs)
├── InputManager (Empty GameObject)
│   └── TouchInputHandler.cs script
└── UI (Empty GameObject)
    └── Canvas (World Space)
        └── ResultText (Text - TextMeshPro)
```

### 6. Component Setup

#### GameManager Setup:
1. Add `GameManager.cs` to GameManager GameObject
2. Assign dice controllers in the inspector
3. Configure roll settings

#### DiceController Setup:
1. Create cube GameObjects for dice
2. Add `DiceController.cs` script
3. Add Rigidbody component
4. Add Box Collider component
5. Create and assign Physics Material

#### Room Colliders Setup:
1. Create cube GameObjects for walls and floor
2. Scale appropriately (Floor: 10x0.1x10, Walls: 0.1x5x10)
3. Position to form enclosed room
4. Add Box Colliders (no Rigidbody needed for static objects)
5. Create and assign wall Physics Materials

### 7. Physics Materials Creation

Create these Physics Materials in `Assets/Materials/Physics/`:

#### DicePhysicsMaterial:
- Dynamic Friction: 0.6
- Static Friction: 0.6
- Bounciness: 0.3
- Friction Combine: Average
- Bounce Combine: Average

#### WallPhysicsMaterial:
- Dynamic Friction: 0.4
- Static Friction: 0.4
- Bounciness: 0.3
- Friction Combine: Average
- Bounce Combine: Average

#### FloorPhysicsMaterial:
- Dynamic Friction: 0.6
- Static Friction: 0.6
- Bounciness: 0.2
- Friction Combine: Average
- Bounce Combine: Average

### 8. Input System Setup

1. Create Input Action Asset in `Assets/Scripts/Input/`
2. Name it: **DiceGameInputActions**
3. Add Touch action map with:
   - Touch (Button): `<Touchscreen>/primaryTouch/press`
   - Position (Vector2): `<Touchscreen>/primaryTouch/position`
   - Delta (Vector2): `<Touchscreen>/primaryTouch/delta`

### 9. Testing Setup

#### In Unity Editor:
1. Enable "Simulate Touch Input from Mouse or Pen" in Input System settings
2. Use Unity Remote 5 for real device testing
3. Test physics by clicking to roll dice

#### iOS Device Testing:
1. Connect iPhone via USB
2. Build and Run (File → Build Settings → Build and Run)
3. Test touch controls and physics behavior

### 10. Performance Testing

Monitor these metrics during development:
- **Frame Rate**: Target 60fps on iPhone 8+
- **Memory Usage**: Keep under 100MB
- **Physics Performance**: Watch Profiler during dice rolls

## Next Steps

After completing this setup:
1. Test basic dice rolling functionality
2. Tune physics materials for realistic behavior
3. Implement UI for result display
4. Add audio feedback
5. Optimize for target devices

## Troubleshooting

### Common Issues:
- **Scripts not compiling**: Check namespace imports
- **Physics not working**: Verify Rigidbody and Collider setup
- **Touch not responding**: Check Input System package installation
- **iOS build failing**: Verify iOS Build Support module installed

### Performance Issues:
- **Low frame rate**: Reduce physics quality in Project Settings
- **High memory usage**: Check for memory leaks in Profiler
- **Touch lag**: Optimize UI update frequency

## Development Tools

Recommended Unity Editor windows to keep open:
- **Hierarchy**: Scene organization
- **Inspector**: Component configuration  
- **Console**: Error tracking
- **Profiler**: Performance monitoring
- **Physics Debugger**: Physics troubleshooting