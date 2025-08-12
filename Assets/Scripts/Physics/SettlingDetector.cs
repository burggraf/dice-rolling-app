using UnityEngine;
using UnityEngine.Events;
using DiceGame.Physics;

namespace DiceGame.Physics
{
    /// <summary>
    /// Advanced settling detection for dice with multiple criteria validation
    /// Handles edge cases like dice on edges, corners, or unstable positions
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SettlingDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private DicePhysicsConfig physicsConfig;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private bool enableEdgeCaseDetection = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Stability Validation")]
        [SerializeField] private int stabilityCheckSamples = 5;
        [SerializeField] private float positionStabilityThreshold = 0.01f;
        [SerializeField] private float rotationStabilityThreshold = 1f;
        
        [Header("Events")]
        public UnityEvent OnSettlingDetected = new UnityEvent();
        public UnityEvent OnUnstableDetected = new UnityEvent();
        public UnityEvent OnEdgeCaseDetected = new UnityEvent();
        
        private Rigidbody rb;
        private Collider col;
        
        private bool isSettled = false;
        private float settlingTimer = 0f;
        private float edgeCaseTimer = 0f;
        
        // Stability tracking
        private Vector3[] positionHistory;
        private Vector3[] rotationHistory;
        private int historyIndex = 0;
        private bool historyFilled = false;
        
        public bool IsSettled => isSettled;
        public bool IsOnEdge { get; private set; } = false;
        public bool IsStacked { get; private set; } = false;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            
            InitializeStabilityTracking();
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(CheckSettling), 0f, checkInterval);
            
            if (enableEdgeCaseDetection)
            {
                InvokeRepeating(nameof(CheckEdgeCases), 0.5f, 0.5f);
            }
        }
        
        private void InitializeStabilityTracking()
        {
            positionHistory = new Vector3[stabilityCheckSamples];
            rotationHistory = new Vector3[stabilityCheckSamples];
            
            for (int i = 0; i < stabilityCheckSamples; i++)
            {
                positionHistory[i] = transform.position;
                rotationHistory[i] = transform.eulerAngles;
            }
        }
        
        private void CheckSettling()
        {
            if (rb == null || isSettled) return;
            
            UpdateStabilityHistory();
            
            bool velocitySettled = IsVelocitySettled();
            bool positionStable = IsPositionStable();
            bool rotationStable = IsRotationStable();
            
            if (velocitySettled && positionStable && rotationStable)
            {
                settlingTimer += checkInterval;
                
                float requiredTime = physicsConfig != null ? physicsConfig.settlingTime : DicePhysicsConstants.SETTLING_TIME_REQUIREMENT;
                
                if (settlingTimer >= requiredTime)
                {
                    SetSettled();
                }
            }
            else
            {
                settlingTimer = 0f;
                
                if (isSettled)
                {
                    // Dice became unsettled
                    isSettled = false;
                    OnUnstableDetected?.Invoke();
                }
            }
            
            if (debugMode)
            {
                LogDebugInfo(velocitySettled, positionStable, rotationStable);
            }
        }
        
        private void UpdateStabilityHistory()
        {
            positionHistory[historyIndex] = transform.position;
            rotationHistory[historyIndex] = transform.eulerAngles;
            
            historyIndex = (historyIndex + 1) % stabilityCheckSamples;
            
            if (historyIndex == 0)
                historyFilled = true;
        }
        
        private bool IsVelocitySettled()
        {
            if (physicsConfig != null)
            {
                return physicsConfig.IsVelocitySettled(rb.velocity, rb.angularVelocity);
            }
            
            return rb.velocity.magnitude <= DicePhysicsConstants.SETTLING_VELOCITY_THRESHOLD &&
                   rb.angularVelocity.magnitude <= DicePhysicsConstants.SETTLING_ANGULAR_VELOCITY_THRESHOLD;
        }
        
        private bool IsPositionStable()
        {
            if (!historyFilled) return false;
            
            Vector3 avgPosition = Vector3.zero;
            for (int i = 0; i < stabilityCheckSamples; i++)
            {
                avgPosition += positionHistory[i];
            }
            avgPosition /= stabilityCheckSamples;
            
            float maxDeviation = 0f;
            for (int i = 0; i < stabilityCheckSamples; i++)
            {
                float deviation = Vector3.Distance(positionHistory[i], avgPosition);
                maxDeviation = Mathf.Max(maxDeviation, deviation);
            }
            
            return maxDeviation <= positionStabilityThreshold;
        }
        
        private bool IsRotationStable()
        {
            if (!historyFilled) return false;
            
            float maxAngleChange = 0f;
            for (int i = 1; i < stabilityCheckSamples; i++)
            {
                float angleChange = Vector3.Angle(rotationHistory[i], rotationHistory[i-1]);
                maxAngleChange = Mathf.Max(maxAngleChange, angleChange);
            }
            
            return maxAngleChange <= rotationStabilityThreshold;
        }
        
        private void CheckEdgeCases()
        {
            if (isSettled) return;
            
            CheckIfOnEdge();
            CheckIfStacked();
            
            if (IsOnEdge || IsStacked)
            {
                edgeCaseTimer += 0.5f;
                
                if (edgeCaseTimer >= 3f) // Edge case detected for 3 seconds
                {
                    OnEdgeCaseDetected?.Invoke();
                    HandleEdgeCase();
                }
            }
            else
            {
                edgeCaseTimer = 0f;
            }
        }
        
        private void CheckIfOnEdge()
        {
            // Raycast downward to check if dice is balanced on an edge
            Vector3 center = col.bounds.center;
            float rayDistance = col.bounds.size.y * 0.6f;
            
            int hitCount = 0;
            Vector3[] rayDirections = {
                Vector3.down,
                transform.right * 0.3f + Vector3.down,
                -transform.right * 0.3f + Vector3.down,
                transform.forward * 0.3f + Vector3.down,
                -transform.forward * 0.3f + Vector3.down
            };
            
            foreach (Vector3 direction in rayDirections)
            {
                if (UnityEngine.Physics.Raycast(center, direction.normalized, rayDistance, ~(1 << gameObject.layer)))
                {
                    hitCount++;
                }
                
                if (debugMode)
                {
                    Debug.DrawRay(center, direction.normalized * rayDistance, Color.yellow, 0.1f);
                }
            }
            
            IsOnEdge = hitCount <= 2; // Dice is on edge if only few rays hit ground
        }
        
        private void CheckIfStacked()
        {
            // Check if another dice is too close (stacking detection)
            Collider[] nearbyColliders = UnityEngine.Physics.OverlapSphere(
                transform.position, 
                DicePhysicsConstants.STACK_DETECTION_DISTANCE, 
                ~(1 << gameObject.layer)
            );
            
            IsStacked = false;
            foreach (var nearbyCol in nearbyColliders)
            {
                if (nearbyCol.gameObject != gameObject && nearbyCol.GetComponent<SettlingDetector>() != null)
                {
                    IsStacked = true;
                    break;
                }
            }
        }
        
        private void HandleEdgeCase()
        {
            if (debugMode)
            {
                Debug.Log($"Handling edge case for {gameObject.name} - OnEdge: {IsOnEdge}, Stacked: {IsStacked}");
            }
            
            // Apply small random force to resolve edge case
            Vector3 unstuckForce = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 2f),
                Random.Range(-1f, 1f)
            ).normalized * DicePhysicsConstants.UNSTUCK_FORCE;
            
            rb.AddForce(unstuckForce, ForceMode.Impulse);
            
            // Reset timers
            edgeCaseTimer = 0f;
            settlingTimer = 0f;
            isSettled = false;
        }
        
        private void SetSettled()
        {
            if (isSettled) return;
            
            isSettled = true;
            
            // Ensure complete stop
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            OnSettlingDetected?.Invoke();
            
            if (debugMode)
            {
                Debug.Log($"Dice {gameObject.name} settled after {settlingTimer:F2} seconds");
            }
        }
        
        public void ForceSettle()
        {
            settlingTimer = 0f;
            SetSettled();
        }
        
        public void ResetSettling()
        {
            isSettled = false;
            settlingTimer = 0f;
            edgeCaseTimer = 0f;
            IsOnEdge = false;
            IsStacked = false;
            
            InitializeStabilityTracking();
        }
        
        private void LogDebugInfo(bool velocitySettled, bool positionStable, bool rotationStable)
        {
            Debug.Log($"Settling Debug - Velocity: {velocitySettled}, Position: {positionStable}, " +
                     $"Rotation: {rotationStable}, Timer: {settlingTimer:F2}, OnEdge: {IsOnEdge}, Stacked: {IsStacked}");
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !debugMode) return;
            
            // Draw settling status
            Gizmos.color = isSettled ? Color.green : (IsOnEdge || IsStacked ? Color.red : Color.yellow);
            Gizmos.DrawWireSphere(transform.position, 0.15f);
            
            // Draw velocity vectors
            if (rb != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + rb.angularVelocity * 0.1f);
            }
            
            // Draw stack detection sphere
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, DicePhysicsConstants.STACK_DETECTION_DISTANCE);
        }
    }
}