using UnityEngine;

namespace DiceGame.Physics
{
    /// <summary>
    /// ScriptableObject for configurable physics properties
    /// Allows for easy tuning and different dice variations
    /// </summary>
    [CreateAssetMenu(fileName = "DicePhysicsConfig", menuName = "Dice Game/Physics Config")]
    public class DicePhysicsConfig : ScriptableObject
    {
        [Header("Physics Material Properties")]
        [Range(0f, 1f)] public float bounciness = 0.3f;
        [Range(0f, 1f)] public float friction = 0.6f;
        [Range(0f, 1f)] public float frictionCombine = 0.5f;
        [Range(0f, 1f)] public float bounceCombine = 0.5f;
        
        [Header("Physics Material References")]
        public PhysicMaterial physicsMaterial;
        
        [Header("Dice Mass Properties")]
        [Range(0.1f, 10f)] public float mass = 1f;
        [Range(0f, 10f)] public float drag = 0.5f;
        [Range(0f, 10f)] public float angularDrag = 5f;
        
        [Header("Roll Force Settings")]
        [Range(1f, 50f)] public float minRollForce = 5f;
        [Range(1f, 50f)] public float maxRollForce = 15f;
        [Range(10f, 500f)] public float minTorque = 50f;
        [Range(10f, 500f)] public float maxTorque = 150f;
        
        [Header("Settling Detection")]
        [Range(0.01f, 1f)] public float velocityThreshold = 0.1f;
        [Range(0.01f, 5f)] public float angularVelocityThreshold = 0.1f;
        [Range(0.1f, 5f)] public float settlingTime = 1f;
        
        [Header("Edge Case Handling")]
        [Range(10f, 80f)] public float edgeDetectionAngle = 45f;
        [Range(1f, 20f)] public float unstuckForce = 5f;
        [Range(0.01f, 0.5f)] public float stackDetectionDistance = 0.1f;
        
        private void OnValidate()
        {
            // Ensure logical constraints
            maxRollForce = Mathf.Max(maxRollForce, minRollForce);
            maxTorque = Mathf.Max(maxTorque, minTorque);
            
            // Update physics material if it exists
            if (physicsMaterial != null)
            {
                physicsMaterial.bounciness = bounciness;
                physicsMaterial.dynamicFriction = friction;
                physicsMaterial.staticFriction = friction;
                physicsMaterial.frictionCombine = (PhysicMaterialCombine)Mathf.RoundToInt(frictionCombine * 3);
                physicsMaterial.bounceCombine = (PhysicMaterialCombine)Mathf.RoundToInt(bounceCombine * 3);
            }
        }
        
        /// <summary>
        /// Apply this configuration to a Rigidbody component
        /// </summary>
        public void ApplyToRigidbody(Rigidbody rigidbody)
        {
            if (rigidbody == null) return;
            
            rigidbody.mass = mass;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;
        }
        
        /// <summary>
        /// Apply this configuration to a Collider component
        /// </summary>
        public void ApplyToCollider(Collider collider)
        {
            if (collider == null || physicsMaterial == null) return;
            
            collider.material = physicsMaterial;
        }
        
        /// <summary>
        /// Get random roll force within configured range
        /// </summary>
        public float GetRandomRollForce()
        {
            return Random.Range(minRollForce, maxRollForce);
        }
        
        /// <summary>
        /// Get random torque within configured range
        /// </summary>
        public Vector3 GetRandomTorque()
        {
            return new Vector3(
                Random.Range(minTorque, maxTorque),
                Random.Range(minTorque, maxTorque),
                Random.Range(minTorque, maxTorque)
            );
        }
        
        /// <summary>
        /// Check if velocity indicates settling
        /// </summary>
        public bool IsVelocitySettled(Vector3 velocity, Vector3 angularVelocity)
        {
            return velocity.magnitude <= velocityThreshold && 
                   angularVelocity.magnitude <= angularVelocityThreshold;
        }
        
        /// <summary>
        /// Create a physics material with current settings
        /// </summary>
        [ContextMenu("Create Physics Material")]
        public PhysicMaterial CreatePhysicsMaterial()
        {
            PhysicMaterial material = new PhysicMaterial($"{name}_PhysicsMaterial");
            material.bounciness = bounciness;
            material.dynamicFriction = friction;
            material.staticFriction = friction;
            material.frictionCombine = (PhysicMaterialCombine)Mathf.RoundToInt(frictionCombine * 3);
            material.bounceCombine = (PhysicMaterialCombine)Mathf.RoundToInt(bounceCombine * 3);
            
            physicsMaterial = material;
            return material;
        }
    }
}