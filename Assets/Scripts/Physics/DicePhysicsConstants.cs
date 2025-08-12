using UnityEngine;

namespace DiceGame.Physics
{
    /// <summary>
    /// Physics constants for dice simulation based on testing and optimization
    /// </summary>
    public static class DicePhysicsConstants
    {
        // Dice Physics
        public const float DICE_MASS = 1f;
        public const float DICE_DRAG = 0.5f;
        public const float DICE_ANGULAR_DRAG = 5f;
        
        // Wall Physics
        public const float WALL_BOUNCINESS = 0.3f;
        public const float WALL_FRICTION = 0.4f;
        
        // Floor Physics
        public const float FLOOR_BOUNCINESS = 0.2f;
        public const float FLOOR_FRICTION = 0.6f;
        
        // Settling Detection
        public const float SETTLING_VELOCITY_THRESHOLD = 0.1f;
        public const float SETTLING_ANGULAR_VELOCITY_THRESHOLD = 0.1f;
        public const float SETTLING_TIME_REQUIREMENT = 1f;
        
        // Roll Force
        public const float MIN_ROLL_FORCE = 5f;
        public const float MAX_ROLL_FORCE = 15f;
        public const float MIN_TORQUE = 50f;
        public const float MAX_TORQUE = 150f;
        
        // Edge Case Detection
        public const float EDGE_DETECTION_ANGLE = 45f;
        public const float UNSTUCK_FORCE = 5f;
        public const float STACK_DETECTION_DISTANCE = 0.1f;
    }
}