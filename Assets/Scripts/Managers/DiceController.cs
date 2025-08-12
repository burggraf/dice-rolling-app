using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DiceGame.Physics;

namespace DiceGame.Managers
{
    /// <summary>
    /// Controls individual dice physics, settling detection, and result calculation
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class DiceController : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float rollForceRange = 10f;
        [SerializeField] private float torqueRange = 100f;
        [SerializeField] private Vector3 rollDirection = Vector3.forward;
        
        [Header("Dice Configuration")]
        [SerializeField] private Transform[] diceFaces = new Transform[6];
        [SerializeField] private int[] faceValues = {1, 2, 3, 4, 5, 6};
        
        [Header("Settling Detection")]
        [SerializeField] private float settlingCheckInterval = 0.1f;
        [SerializeField] private bool debugSettling = false;
        
        [Header("Score Display Test")]
        [SerializeField] private bool createScoreUI = true;
        
        [Header("Events")]
        public UnityEvent OnRollStarted = new UnityEvent();
        public UnityEvent OnDiceSettled = new UnityEvent();
        public UnityEvent<int> OnValueChanged = new UnityEvent<int>();
        
        private Rigidbody rb;
        private Collider col;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        
        private bool isSettled = false;
        private float settlingTimer = 0f;
        private int currentValue = 1;
        
        public bool IsSettled => isSettled;
        public int CurrentValue => currentValue;
        
        private void Awake()
        {
            InitializeComponents();
            SetupPhysics();
            CacheInitialTransform();
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(CheckSettling), 0f, settlingCheckInterval);
            
            if (createScoreUI)
            {
                CreateScoreDisplay();
            }
        }
        
        private void CreateScoreDisplay()
        {
            Debug.Log("DiceController creating score display...");
            
            // Create Canvas
            GameObject canvasGO = new GameObject("SCORE_CANVAS_FROM_DICE");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create Score Text
            GameObject scoreGO = new GameObject("SCORE_TEXT_FROM_DICE");
            scoreGO.transform.SetParent(canvas.transform, false);
            
            TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreText.text = "DICE SCORE: 888";
            scoreText.fontSize = 50;
            scoreText.color = Color.cyan;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.alignment = TextAlignmentOptions.TopRight;
            
            // Position at top right
            RectTransform rt = scoreText.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(400f, 100f);
            
            Debug.Log("Score display created from DiceController!");
        }
        
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            
            if (rb == null)
            {
                Debug.LogError($"Rigidbody component missing on {gameObject.name}");
            }
            
            if (col == null)
            {
                Debug.LogError($"Collider component missing on {gameObject.name}");
            }
        }
        
        private void SetupPhysics()
        {
            if (rb == null) return;
            
            rb.mass = DicePhysicsConstants.DICE_MASS;
            rb.drag = DicePhysicsConstants.DICE_DRAG;
            rb.angularDrag = DicePhysicsConstants.DICE_ANGULAR_DRAG;
            
            // Ensure continuous collision detection for fast-moving dice
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        private void CacheInitialTransform()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
        
        public void ApplyRollForce()
        {
            if (rb == null) return;
            
            isSettled = false;
            settlingTimer = 0f;
            
            // Apply random force in roll direction with variation
            Vector3 force = rollDirection.normalized * Random.Range(DicePhysicsConstants.MIN_ROLL_FORCE, DicePhysicsConstants.MAX_ROLL_FORCE);
            force += new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(0f, 3f),
                Random.Range(-2f, 2f)
            );
            
            // Apply random torque on all axes
            Vector3 torque = new Vector3(
                Random.Range(DicePhysicsConstants.MIN_TORQUE, DicePhysicsConstants.MAX_TORQUE),
                Random.Range(DicePhysicsConstants.MIN_TORQUE, DicePhysicsConstants.MAX_TORQUE),
                Random.Range(DicePhysicsConstants.MIN_TORQUE, DicePhysicsConstants.MAX_TORQUE)
            );
            
            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(torque, ForceMode.Impulse);
            
            OnRollStarted?.Invoke();
            
            if (debugSettling)
            {
                Debug.Log($"Applied force: {force}, torque: {torque}");
            }
        }
        
        private void CheckSettling()
        {
            if (rb == null || isSettled) return;
            
            bool velocitySettled = rb.velocity.magnitude <= DicePhysicsConstants.SETTLING_VELOCITY_THRESHOLD;
            bool angularVelocitySettled = rb.angularVelocity.magnitude <= DicePhysicsConstants.SETTLING_ANGULAR_VELOCITY_THRESHOLD;
            
            if (velocitySettled && angularVelocitySettled)
            {
                settlingTimer += settlingCheckInterval;
                
                if (settlingTimer >= DicePhysicsConstants.SETTLING_TIME_REQUIREMENT)
                {
                    SetDiceSettled();
                }
            }
            else
            {
                settlingTimer = 0f;
            }
            
            if (debugSettling)
            {
                Debug.Log($"Velocity: {rb.velocity.magnitude:F3}, Angular: {rb.angularVelocity.magnitude:F3}, Timer: {settlingTimer:F2}");
            }
        }
        
        private void SetDiceSettled()
        {
            isSettled = true;
            
            // Stop all physics movement
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Calculate final dice value
            int newValue = CalculateDiceValue();
            if (newValue != currentValue)
            {
                currentValue = newValue;
                OnValueChanged?.Invoke(currentValue);
            }
            
            OnDiceSettled?.Invoke();
            
            if (debugSettling)
            {
                Debug.Log($"Dice settled with value: {currentValue}");
            }
        }
        
        public int GetDiceValue()
        {
            if (!isSettled)
            {
                return CalculateDiceValue();
            }
            return currentValue;
        }
        
        private int CalculateDiceValue()
        {
            if (diceFaces == null || diceFaces.Length != 6)
            {
                // Fallback: use transform rotation to determine face
                return CalculateValueFromRotation();
            }
            
            // Find which face is pointing up (highest Y position)
            float highestY = float.MinValue;
            int topFaceIndex = 0;
            
            for (int i = 0; i < diceFaces.Length; i++)
            {
                if (diceFaces[i] != null)
                {
                    float worldY = diceFaces[i].position.y;
                    if (worldY > highestY)
                    {
                        highestY = worldY;
                        topFaceIndex = i;
                    }
                }
            }
            
            return faceValues[topFaceIndex];
        }
        
        private int CalculateValueFromRotation()
        {
            // Simple rotation-based calculation for cube dice
            // This is a fallback method when face transforms aren't configured
            Vector3 up = transform.up;
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            // Determine which face is most aligned with world up
            float[] dots = {
                Vector3.Dot(up, Vector3.up),      // Top face
                Vector3.Dot(-up, Vector3.up),     // Bottom face  
                Vector3.Dot(forward, Vector3.up), // Forward face
                Vector3.Dot(-forward, Vector3.up),// Back face
                Vector3.Dot(right, Vector3.up),   // Right face
                Vector3.Dot(-right, Vector3.up)   // Left face
            };
            
            int maxIndex = 0;
            for (int i = 1; i < dots.Length; i++)
            {
                if (dots[i] > dots[maxIndex])
                {
                    maxIndex = i;
                }
            }
            
            return faceValues[maxIndex];
        }
        
        public void ResetDicePosition()
        {
            if (rb == null) return;
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            isSettled = false;
            settlingTimer = 0f;
            currentValue = 1;
        }
        
        public void AddRandomRotation()
        {
            // Add slight random rotation for variety
            transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Reset settling timer on collision
            settlingTimer = 0f;
        }
        
        private void OnValidate()
        {
            rollForceRange = Mathf.Max(rollForceRange, 0f);
            torqueRange = Mathf.Max(torqueRange, 0f);
            settlingCheckInterval = Mathf.Clamp(settlingCheckInterval, 0.05f, 1f);
            
            if (faceValues.Length != 6)
            {
                faceValues = new int[] {1, 2, 3, 4, 5, 6};
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (debugSettling && Application.isPlaying)
            {
                Gizmos.color = isSettled ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.1f);
                
                if (rb != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
                }
            }
        }
    }
}