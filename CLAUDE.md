# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an iPhone dice rolling game built with Unity 2022.3.50 LTS featuring physics-based dice simulation with realistic bouncing within a contained room environment. The app uses touch controls for rolling dice and displays results both on-screen and on the floor.

## Development Commands

### Unity Project Setup
- **Open Project**: Open "Dice Rolling App" folder in Unity 2022.3.50 LTS
- **iOS Build**: File → Build Settings → iOS → Build (requires iOS Build Support module)
- **Test Build**: File → Build Settings → iOS → Build and Run (requires connected iOS device)

### Key Unity Tools
- **Profiler**: Window → Analysis → Profiler (monitor FPS, memory, physics)
- **Physics Debugger**: Window → Analysis → Physics Debugger
- **Console**: Window → General → Console (for debugging dice settling and validation)

### Testing Commands
- **Play Mode**: Enter Play Mode to test dice physics
- **Test Shortcuts** (when ScoreSystemTestHelper is present):
  - `V` key: Simulate valid roll
  - `I` key: Simulate invalid roll  
  - `Space` key: Trigger normal dice roll

## Architecture Overview

### Core Manager System
The project follows a Manager-based architecture with event-driven communication:

- **GameManager**: Singleton managing game state, dice coordination, and result validation
- **DiceController**: Individual dice physics, settling detection, and value calculation
- **UIManager**: UI system coordination and canvas management
- **ScoreManager**: Screen-space score display with validation feedback

### Key Namespaces
```csharp
DiceGame.Managers     // Core game management classes
DiceGame.Physics      // Physics constants and configurations
DiceGame.UI           // UI components and managers
```

### Physics Implementation
- **Dice Physics**: Rigidbody + Collider with physics materials from `DicePhysicsConstants`
- **Settling Detection**: Multi-criteria validation (velocity, angular velocity, time)
- **Validation System**: Wall collision detection and boundary checking
- **Edge Cases**: Automatic detection for dice touching walls or outside bounds

### Event System
Uses UnityEvents for decoupled communication:
- `OnRollStart` / `OnRollComplete` - Dice rolling lifecycle
- `OnDiceSettled` - Individual dice settling
- `OnResultCalculated(int)` - Final result (use -1 for invalid rolls)

## Key Physics Constants

Located in `Assets/Scripts/Physics/DicePhysicsConstants.cs`:
```csharp
DICE_MASS = 1f
SETTLING_VELOCITY_THRESHOLD = 0.1f
SETTLING_TIME_REQUIREMENT = 1f
MIN_ROLL_FORCE = 5f / MAX_ROLL_FORCE = 15f
MIN_TORQUE = 50f / MAX_TORQUE = 150f
```

## Score System Architecture

### Core Components
1. **ScoreManager**: Top-right screen display, handles valid/invalid roll messaging
2. **GameManager Validation**: Checks wall contact and boundary violations
3. **ResultDisplayUI**: World-space floor display for results

### Validation Logic
- Dice touching walls (tagged "Wall") → Invalid roll
- Dice outside board boundaries → Invalid roll  
- Valid rolls show total score, invalid rolls show "Roll Again"

### Setup Requirements
- Canvas with UIManager component
- GameManager with configured Board Center and Board Size
- Wall objects tagged with "Wall" tag

## File Structure

```
Assets/Scripts/
├── Managers/           # GameManager, DiceController
├── Physics/           # DicePhysicsConstants, DicePhysicsConfig, SettlingDetector
├── Input/             # TouchInputHandler, input action assets
├── UI/                # UIManager, ScoreManager, ResultDisplayUI
├── Utils/             # PerformanceMonitor, test helpers
└── Audio/             # Audio management (future)

Assets/Materials/Physics/  # Physics materials for dice/walls/floor
Assets/Prefabs/           # Dice and environment prefabs
Assets/Scenes/            # DiceGame.unity main scene
```

## Common Development Patterns

### Adding New Dice Behavior
1. Modify `DiceController` for physics changes
2. Update `DicePhysicsConstants` for tuning values
3. Use existing event system for state communication

### UI Development
1. Use UIManager as the central coordinator
2. Screen-space UI should integrate with ScoreManager pattern
3. World-space UI follows ResultDisplayUI pattern

### Physics Tuning
1. Modify constants in `DicePhysicsConstants.cs`
2. Test with Physics Debugger and Profiler
3. Use debug flags in components for real-time feedback

### Testing New Features
1. Add test methods to ScoreSystemTestHelper pattern
2. Use Context Menu attributes for inspector testing
3. Implement keyboard shortcuts for rapid iteration

## Performance Targets

- **Target FPS**: 60fps on iPhone 8+, 30fps fallback
- **Memory**: < 100MB RAM usage
- **Touch Response**: < 16ms input latency
- **Load Time**: < 3 seconds to playable state

## Debugging Tips

### Physics Issues
- Enable `debugSettling` in DiceController inspector
- Use `debugBounds` in GameManager to visualize play area
- Check Physics Debugger for collision detection issues

### Score System Issues
- Verify wall objects have "Wall" tag
- Check Board Center and Board Size configuration in GameManager
- Use ScoreSystemTestHelper keyboard shortcuts to isolate problems

### Common Gotchas
- Dice settling requires all criteria: velocity, angular velocity, and time
- Invalid rolls return -1 from GameManager.OnResultCalculated
- Score display requires Canvas with UIManager component
- Wall detection uses raycast from dice center, not collision events

## Mobile-Specific Considerations

- **iOS Target**: iOS 13.0+ with portrait/landscape support
- **Touch Input**: Unity Input System with gesture recognition
- **Performance**: Continuous collision detection for fast-moving dice
- **UI Scaling**: CanvasScaler with reference resolution 1920x1080

## Implementation Status

Currently in Phase 1 (Core Physics) with working:
- ✅ Basic dice physics and settling detection
- ✅ Manager architecture and event system  
- ✅ Score validation and display system
- ✅ Touch input handling foundation
- 🔄 Mobile optimization and polish (ongoing)