# Quick Unity Setup Checklist

## ✅ What's Already Done For You:
- 📁 Complete project folder structure
- 🎯 All C# scripts with full implementation
- ⚡ Physics materials (dice, walls, floor)
- 🎮 Input Action asset configured
- 📱 iOS project settings optimized
- 🎬 Basic scene file with camera and lighting

## 🔧 What You Need to Do (5 minutes):

### 1. Create Unity Project
1. Open Unity Hub
2. Click "New Project" 
3. Select **Unity 2022.3.50f1 LTS**
4. Choose **3D (URP)** template
5. Set location to: `/Users/markb/dev/dice`
6. Click "Create Project"

### 2. Install Required Packages
In Unity: Window → Package Manager → Install:
- ✅ **Input System** (for touch controls)
- ✅ **iOS Build Support** (for iPhone builds)

### 3. Quick Scene Setup (2 minutes)
1. **Open DiceGame scene**: `Assets/Scenes/DiceGame.unity`

2. **Create Room Objects**:
   ```
   Create → 3D Object → Cube (name: "Floor")
   - Scale: (10, 0.1, 10)
   - Position: (0, 0, 0)
   - Add Box Collider (already has one)
   - Assign FloorPhysicsMaterial to collider
   ```

3. **Create 4 Walls**:
   ```
   Create → 3D Object → Cube (name: "WallNorth")
   - Scale: (10, 5, 0.1) 
   - Position: (0, 2.5, 5)
   - Assign WallPhysicsMaterial
   
   Duplicate for South (-5), East (5, 0), West (-5, 0)
   ```

4. **Create Dice**:
   ```
   Create → 3D Object → Cube (name: "Die1")
   - Position: (0, 3, 0)
   - Add Rigidbody component
   - Add DiceController script
   - Assign DicePhysicsMaterial to collider
   
   Duplicate for Die2 at (1, 3, 0)
   ```

5. **Add GameManager**:
   ```
   Create Empty GameObject (name: "GameManager")
   - Add GameManager script
   - Drag Die1 and Die2 to diceControllers array
   ```

6. **Add Input Handler**:
   ```
   Create Empty GameObject (name: "InputManager")  
   - Add TouchInputHandler script
   - Connect OnTap event to GameManager.RollDice()
   ```

### 4. Test It!
- ✅ Press Play
- ✅ Click anywhere to roll dice
- ✅ Watch physics simulation
- ✅ Check Console for results

## 🎯 Expected Results:
- Dice respond to clicks/taps
- Realistic bouncing off walls
- Settling detection works
- Results appear in Console
- 60fps performance

## 📱 iOS Build Test:
1. File → Build Settings → iOS
2. Switch Platform  
3. Build and Run (with connected iPhone)

## 🔧 Troubleshooting:
- **Scripts not compiling**: Check Input System package installed
- **No physics**: Verify Rigidbody components added
- **No input**: Check TouchInputHandler events connected
- **Performance issues**: Enable Performance Monitor debug UI

Total setup time: **~5 minutes** for a fully functional dice game!