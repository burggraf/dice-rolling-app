# Dice Game Score System Setup Guide

## Overview
This score system adds a score display at the top right of the screen that shows the total of each dice roll. If either die leans against a wall or rolls off the board, it shows "Roll Again" instead of a score.

## Key Components

### 1. ScoreManager (`Assets/Scripts/UI/ScoreManager.cs`)
- Displays score at top-right of screen
- Handles "Roll Again" message for invalid rolls
- Automatically listens to GameManager events

### 2. Enhanced GameManager (`Assets/Scripts/Managers/GameManager.cs`)
- Added dice validation for wall contact and board boundaries
- Sends -1 result for invalid rolls
- Configurable board boundaries with visual gizmos

### 3. UIManager (`Assets/Scripts/UI/UIManager.cs`)
- Coordinates all UI elements
- Sets up Screen Space overlay canvas
- Manages ScoreManager lifecycle

### 4. ScoreSystemTestHelper (`Assets/Scripts/Utils/ScoreSystemTestHelper.cs`)
- Testing utility for the score system
- Keyboard shortcuts for testing valid/invalid rolls
- GUI buttons for easy testing

## Setup Instructions

### 1. Scene Setup
1. Create a Canvas GameObject in your scene
2. Set Canvas Render Mode to "Screen Space - Overlay"
3. Add UIManager component to the Canvas
4. The ScoreManager will be created automatically

### 2. GameManager Configuration
1. On your GameManager GameObject:
   - Set "Board Center" to your game board's center Transform
   - Configure "Board Size" to match your play area
   - Enable "Debug Bounds" to visualize boundaries in Scene view

### 3. Wall Tagging
1. Tag all wall objects with "Wall" tag
2. Ensure walls have Colliders for detection

### 4. Testing
1. Add ScoreSystemTestHelper to any GameObject for testing
2. Use keyboard shortcuts:
   - `V` - Simulate valid roll
   - `I` - Simulate invalid roll
   - `Space` - Trigger normal dice roll
3. Or use the Context Menu options in the Inspector

## Configuration Options

### GameManager Settings
- **Board Center**: Transform representing the center of the play area
- **Board Size**: Vector3 defining the valid play area dimensions
- **Debug Bounds**: Shows board boundaries as green wireframe in Scene view

### ScoreManager Settings
- **Score Prefix**: Text before the score number (default: "Score: ")
- **Roll Again Text**: Message for invalid rolls (default: "Roll Again")
- **Normal Score Color**: Color for valid scores (default: White)
- **Invalid Roll Color**: Color for invalid roll message (default: Red)
- **Top Right Offset**: Position offset from top-right corner

## How It Works

1. **Dice Roll**: Player triggers dice roll
2. **Settling**: Dice physics settle and stop moving
3. **Validation**: GameManager checks each die:
   - Is it touching a wall? (Raycast detection)
   - Is it outside board boundaries?
4. **Result**:
   - **Valid**: Calculate total score and display
   - **Invalid**: Show "Roll Again" message
5. **Display**: 
   - Score appears at top-right of screen
   - "Roll Again" message also appears on floor (ResultDisplayUI)

## Events Flow

```
GameManager.OnResultCalculated(int score)
    ↓
ScoreManager.HandleGameResult(int score)
    ↓
if score == -1:
    ShowInvalidRoll() → "Roll Again"
else:
    ShowValidScore(score) → "Score: X"
```

## Customization

### Changing Colors
```csharp
scoreManager.SetScoreColors(Color.green, Color.yellow);
```

### Custom Board Boundaries
Set the Board Center and Board Size in GameManager inspector, or via code:
```csharp
gameManager.boardCenter = myBoardTransform;
gameManager.boardSize = new Vector3(12f, 3f, 12f);
```

### Custom Text
Modify the ScoreManager's "Score Prefix" and "Roll Again Text" fields in the inspector.

## Troubleshooting

### Score Not Appearing
1. Check that UIManager is on a Canvas with Screen Space - Overlay
2. Ensure GameManager has OnResultCalculated event listeners
3. Verify ScoreManager is created and active

### "Roll Again" Not Triggering
1. Ensure walls are tagged with "Wall"
2. Check Board Center and Board Size are properly configured
3. Enable Debug Bounds to visualize the play area
4. Check console for validation debug messages

### Physics Issues
1. Ensure dice have Colliders
2. Verify wall Colliders are properly set up
3. Check that dice settling detection is working (DiceController.IsSettled)

## Dependencies
- Unity 2022.3+ LTS
- TextMeshPro (for UI text display)
- Unity Physics (for collision detection)
- UnityEngine.Events (for event system)