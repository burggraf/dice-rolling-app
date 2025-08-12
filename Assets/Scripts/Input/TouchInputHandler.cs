using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace DiceGame.Input
{
    /// <summary>
    /// Handles touch input for dice rolling using Unity Input System
    /// Supports tap, swipe, and multi-touch gestures optimized for iOS
    /// </summary>
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("Touch Settings")]
        [SerializeField] private float tapThreshold = 0.1f;
        [SerializeField] private float swipeThreshold = 100f;
        [SerializeField] private float longPressThreshold = 1f;
        [SerializeField] private bool enableHapticFeedback = true;
        
        [Header("Safe Area Settings")]
        [SerializeField] private float edgeSafeZone = 50f;
        [SerializeField] private bool respectNotchAreas = true;
        
        [Header("Input Events")]
        public UnityEvent OnTap = new UnityEvent();
        public UnityEvent OnDoubleTap = new UnityEvent();
        public UnityEvent OnLongPress = new UnityEvent();
        public UnityEvent<Vector2> OnSwipe = new UnityEvent<Vector2>();
        public UnityEvent OnTouchStart = new UnityEvent();
        public UnityEvent OnTouchEnd = new UnityEvent();
        
        private InputAction touchPressAction;
        private InputAction touchPositionAction;
        private InputAction touchDeltaAction;
        
        private bool isTouching = false;
        private float touchStartTime;
        private Vector2 touchStartPosition;
        private Vector2 currentTouchPosition;
        private bool hasMovedBeyondThreshold = false;
        
        // Double tap detection
        private float lastTapTime = 0f;
        private float doubleTapTimeWindow = 0.3f;
        
        // Safe area cache
        private Rect safeArea;
        private bool safeAreaCached = false;
        
        private void Awake()
        {
            SetupInputActions();
            CacheSafeArea();
        }
        
        private void OnEnable()
        {
            EnableInputActions();
        }
        
        private void OnDisable()
        {
            DisableInputActions();
        }
        
        private void SetupInputActions()
        {
            // Create input actions for touch handling
            touchPressAction = new InputAction("TouchPress", InputActionType.Button, "<Touchscreen>/primaryTouch/press");
            touchPositionAction = new InputAction("TouchPosition", InputActionType.Value, "<Touchscreen>/primaryTouch/position");
            touchDeltaAction = new InputAction("TouchDelta", InputActionType.Value, "<Touchscreen>/primaryTouch/delta");
            
            // Bind callbacks
            touchPressAction.performed += OnTouchPress;
            touchPressAction.canceled += OnTouchRelease;
        }
        
        private void EnableInputActions()
        {
            touchPressAction?.Enable();
            touchPositionAction?.Enable();
            touchDeltaAction?.Enable();
        }
        
        private void DisableInputActions()
        {
            touchPressAction?.Disable();
            touchPositionAction?.Disable();
            touchDeltaAction?.Disable();
        }
        
        private void CacheSafeArea()
        {
            safeArea = Screen.safeArea;
            safeAreaCached = true;
            
            if (respectNotchAreas)
            {
                Debug.Log($"Safe Area: {safeArea}, Screen: {Screen.width}x{Screen.height}");
            }
        }
        
        private void Update()
        {
            if (isTouching)
            {
                UpdateTouchPosition();
                CheckLongPress();
            }
            
            // Recache safe area if screen orientation changes
            if (safeAreaCached && safeArea != Screen.safeArea)
            {
                CacheSafeArea();
            }
        }
        
        private void OnTouchPress(InputAction.CallbackContext context)
        {
            if (!IsValidTouchPosition()) return;
            
            isTouching = true;
            touchStartTime = Time.time;
            touchStartPosition = touchPositionAction.ReadValue<Vector2>();
            currentTouchPosition = touchStartPosition;
            hasMovedBeyondThreshold = false;
            
            OnTouchStart?.Invoke();
            
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticFeedbackType.Light);
            }
        }
        
        private void OnTouchRelease(InputAction.CallbackContext context)
        {
            if (!isTouching) return;
            
            float touchDuration = Time.time - touchStartTime;
            Vector2 touchEndPosition = touchPositionAction.ReadValue<Vector2>();
            Vector2 swipeVector = touchEndPosition - touchStartPosition;
            float swipeDistance = swipeVector.magnitude;
            
            isTouching = false;
            
            OnTouchEnd?.Invoke();
            
            // Determine gesture type
            if (swipeDistance >= swipeThreshold)
            {
                HandleSwipe(swipeVector.normalized);
            }
            else if (touchDuration < tapThreshold)
            {
                HandleTap();
            }
            
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticFeedbackType.Medium);
            }
        }
        
        private void UpdateTouchPosition()
        {
            Vector2 newPosition = touchPositionAction.ReadValue<Vector2>();
            Vector2 deltaPosition = newPosition - currentTouchPosition;
            
            currentTouchPosition = newPosition;
            
            if (!hasMovedBeyondThreshold)
            {
                float totalMovement = Vector2.Distance(currentTouchPosition, touchStartPosition);
                if (totalMovement > tapThreshold * Screen.dpi)
                {
                    hasMovedBeyondThreshold = true;
                }
            }
        }
        
        private void CheckLongPress()
        {
            float touchDuration = Time.time - touchStartTime;
            
            if (touchDuration >= longPressThreshold && !hasMovedBeyondThreshold)
            {
                HandleLongPress();
                isTouching = false; // Prevent multiple long press events
            }
        }
        
        private void HandleTap()
        {
            float currentTime = Time.time;
            
            // Check for double tap
            if (currentTime - lastTapTime <= doubleTapTimeWindow)
            {
                OnDoubleTap?.Invoke();
                lastTapTime = 0f; // Reset to prevent triple tap
                
                if (enableHapticFeedback)
                {
                    TriggerHapticFeedback(HapticFeedbackType.Heavy);
                }
            }
            else
            {
                OnTap?.Invoke();
                lastTapTime = currentTime;
            }
        }
        
        private void HandleSwipe(Vector2 swipeDirection)
        {
            OnSwipe?.Invoke(swipeDirection);
            
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticFeedbackType.Medium);
            }
        }
        
        private void HandleLongPress()
        {
            OnLongPress?.Invoke();
            
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback(HapticFeedbackType.Heavy);
            }
        }
        
        private bool IsValidTouchPosition()
        {
            Vector2 touchPos = touchPositionAction.ReadValue<Vector2>();
            
            // Check if touch is within safe area
            if (respectNotchAreas && safeAreaCached)
            {
                if (!safeArea.Contains(touchPos))
                {
                    return false;
                }
            }
            
            // Check edge safe zones
            if (touchPos.x < edgeSafeZone || touchPos.x > Screen.width - edgeSafeZone ||
                touchPos.y < edgeSafeZone || touchPos.y > Screen.height - edgeSafeZone)
            {
                return false;
            }
            
            return true;
        }
        
        private void TriggerHapticFeedback(HapticFeedbackType feedbackType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            switch (feedbackType)
            {
                case HapticFeedbackType.Light:
                    Handheld.Vibrate();
                    break;
                case HapticFeedbackType.Medium:
                    Handheld.Vibrate();
                    break;
                case HapticFeedbackType.Heavy:
                    Handheld.Vibrate();
                    break;
            }
#endif
        }
        
        public void SetHapticFeedback(bool enabled)
        {
            enableHapticFeedback = enabled;
        }
        
        public void SetEdgeSafeZone(float safeZone)
        {
            edgeSafeZone = Mathf.Max(0f, safeZone);
        }
        
        public Vector2 GetCurrentTouchPosition()
        {
            return currentTouchPosition;
        }
        
        public bool IsTouching()
        {
            return isTouching;
        }
        
        public Vector2 GetTouchPositionInWorld(Camera camera)
        {
            if (camera == null) return Vector2.zero;
            
            return camera.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, currentTouchPosition.y, camera.nearClipPlane));
        }
        
        private void OnValidate()
        {
            tapThreshold = Mathf.Max(0.01f, tapThreshold);
            swipeThreshold = Mathf.Max(10f, swipeThreshold);
            longPressThreshold = Mathf.Max(0.1f, longPressThreshold);
            edgeSafeZone = Mathf.Max(0f, edgeSafeZone);
            doubleTapTimeWindow = Mathf.Clamp(doubleTapTimeWindow, 0.1f, 1f);
        }
        
        private void OnDestroy()
        {
            DisableInputActions();
            
            touchPressAction?.Dispose();
            touchPositionAction?.Dispose();
            touchDeltaAction?.Dispose();
        }
        
        public enum HapticFeedbackType
        {
            Light,
            Medium,
            Heavy
        }
    }
}