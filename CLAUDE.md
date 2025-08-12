# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an iPhone dice rolling game built with Unity 2022.3.50 LTS. The app features physics-based dice that bounce realistically off 4 walls within a contained room environment, with touch controls for rolling and results displayed on the floor.

## Development Commands

### Unity Project Setup
- **Create Project**: Use Unity 2022.3.50 LTS with iOS Build Support module
- **iOS Build**: File → Build Settings → iOS → Build
- **Test Build**: File → Build Settings → iOS → Build and Run (requires connected iOS device)

### Performance Testing
- **Unity Profiler**: Window → Analysis → Profiler (monitor FPS, memory, physics)
- **iOS Performance**: Xcode Instruments for device-specific profiling
- **Memory Testing**: Unity Memory Profiler package for detailed analysis

### Physics Testing
- **Physics Debugger**: Window → Analysis → Physics Debugger
- **Collision Visualization**: Enable Physics.debugDraw in Physics settings

## Architecture Overview

### Core Systems
The app follows a Manager-based architecture with event-driven communication:

- **GameManager**: Singleton managing overall game state and dice roll coordination
- **DiceController**: Handles individual dice physics, settling detection, and result calculation
- **PhysicsManager**: Manages physics materials, performance monitoring, and edge case handling
- **TouchInputHandler**: Unity Input System integration for mobile touch controls
- **UIManager**: World Space Canvas for result display and touch feedback

### Physics Implementation
- **Dice Physics**: Rigidbody + Box Collider with custom Physics Materials
- **Room Boundaries**: Static Box Colliders for 4 walls and floor
- **Settling Detection**: Multi-criteria validation (velocity, angular velocity, time)
- **Edge Cases**: Automatic detection and resolution for dice on edges or stacking

### Key Constants
```csharp
// Physics Configuration (DicePhysicsConstants class)
DICE_MASS = 1f
SETTLING_VELOCITY_THRESHOLD = 0.1f
SETTLING_TIME_REQUIREMENT = 1f
```

## Development Phases

### Phase 1: Core Physics (Weeks 1-2)
- Unity project setup with iOS build support
- Basic room environment with collision boundaries
- Initial dice physics with Rigidbody and custom Physics Materials
- Basic settling detection system

### Phase 2: Architecture (Weeks 3-4)
- Manager class structure implementation
- Event system for dice state communication
- Custom 3D dice models integration
- ScriptableObject-based physics configuration system

### Phase 3: Mobile Integration (Weeks 5-6)
- Unity Input System for touch controls
- World Space Canvas for floor result display
- iOS-specific optimizations and safe area handling
- Mobile performance optimization

### Phase 4: Polish (Weeks 7-8)
- Physics edge case handling (stuck dice, stacking)
- Audio integration (rolling, bouncing, settling sounds)
- iOS build pipeline and device testing
- Performance optimization for target 60fps on iPhone 8+

## File Structure Conventions

```
Assets/
├── Scripts/Managers/     # Core singleton managers
├── Scripts/Physics/      # Physics-related components and configs
├── Scripts/Input/        # Touch input handling
├── Scripts/UI/          # UI components and managers
├── Scripts/Utils/       # Utility classes and helpers
├── Prefabs/Dice/        # Dice variants and configurations
├── Prefabs/Environment/ # Room and boundary prefabs
├── Materials/Physics/   # Physics Materials for dice/walls
└── Scenes/             # Game scenes
```

## Performance Requirements

- **Target**: 60fps on iPhone 8+, 30fps fallback for older devices
- **Memory**: < 100MB RAM usage
- **Load Time**: < 3 seconds to playable state
- **Touch Response**: < 16ms input latency

## iOS-Specific Considerations

- **Minimum iOS**: 13.0+
- **Orientation**: Portrait and landscape support
- **Safe Areas**: Proper handling for notched devices
- **Background Behavior**: Configure to Exit (recommended for physics apps)
- **Input Validation**: Prevent conflicts with iOS system gestures

## Physics Tuning Values

Based on testing, optimal physics material settings:
- **Dice Bounciness**: 0.3f
- **Wall Friction**: 0.4f
- **Settling Velocity**: 0.1f units/second
- **Settling Time**: 1.0f seconds minimum

## Common Development Patterns

### Dice Rolling Implementation
```csharp
// Apply random torque and initial velocity
rigidbody.AddTorque(Random.Range(-torqueRange, torqueRange), 
                   Random.Range(-torqueRange, torqueRange), 
                   Random.Range(-torqueRange, torqueRange));
```

### Settling Detection
Multi-criteria validation combining velocity, angular velocity, and time requirements to ensure accurate dice state detection.

### Event System
UnityEvents for dice state changes (OnRollStart, OnDiceSettled, OnResultCalculated) enabling decoupled system communication.

## Testing Strategy

- **Unit Tests**: Individual component functionality
- **Physics Tests**: Dice settling accuracy and edge cases
- **Performance Tests**: Frame rate and memory usage on target devices
- **Device Tests**: Multiple iOS device compatibility validation