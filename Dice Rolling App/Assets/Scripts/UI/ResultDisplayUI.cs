using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DiceGame.UI
{
    /// <summary>
    /// Handles display of dice roll results on the floor using World Space Canvas
    /// Provides animated text display with fade in/out effects
    /// </summary>
    public class ResultDisplayUI : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("Text Formatting")]
        [SerializeField] private string resultPrefix = "Result: ";
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failColor = Color.red;
        [SerializeField] private float textSize = 48f;
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private Vector3 targetScale = Vector3.one;
        [SerializeField] private Vector3 startScale = Vector3.zero;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Positioning")]
        [SerializeField] private Transform floorCenter;
        [SerializeField] private float heightOffset = 0.1f;
        [SerializeField] private bool followCamera = true;
        
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Coroutine displayCoroutine;
        private Camera mainCamera;
        
        public bool IsDisplaying { get; private set; } = false;
        
        private void Awake()
        {
            InitializeComponents();
            SetupCanvas();
            SetupText();
        }
        
        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            
            HideDisplay();
        }
        
        private void LateUpdate()
        {
            if (followCamera && IsDisplaying && mainCamera != null)
            {
                UpdateCameraFacing();
            }
        }
        
        private void InitializeComponents()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            rectTransform = GetComponent<RectTransform>();
            
            if (resultText == null)
            {
                resultText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        
        private void SetupCanvas()
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Position canvas on floor
            if (floorCenter != null)
            {
                transform.position = floorCenter.position + Vector3.up * heightOffset;
            }
            
            // Scale canvas for world space
            transform.localScale = Vector3.one * 0.01f; // Adjust based on scene scale
            
            // Configure canvas sorting
            canvas.sortingOrder = 10;
        }
        
        private void SetupText()
        {
            if (resultText == null) return;
            
            resultText.text = "";
            resultText.fontSize = textSize;
            resultText.color = textColor;
            resultText.alignment = TextAlignmentOptions.Center;
            
            // Enable better mobile rendering
            resultText.enableWordWrapping = false;
            resultText.overflowMode = TextOverflowModes.Overflow;
        }
        
        public void DisplayResult(int diceValue)
        {
            DisplayResult(diceValue, textColor);
        }
        
        public void DisplayResult(int diceValue, Color color)
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            
            displayCoroutine = StartCoroutine(DisplayResultCoroutine(diceValue, color));
        }
        
        public void DisplayCustomText(string text)
        {
            DisplayCustomText(text, textColor);
        }
        
        public void DisplayCustomText(string text, Color color)
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            
            displayCoroutine = StartCoroutine(DisplayCustomTextCoroutine(text, color));
        }
        
        private IEnumerator DisplayResultCoroutine(int diceValue, Color color)
        {
            string displayText = resultPrefix + diceValue.ToString();
            yield return StartCoroutine(DisplayTextCoroutine(displayText, color));
        }
        
        private IEnumerator DisplayCustomTextCoroutine(string text, Color color)
        {
            yield return StartCoroutine(DisplayTextCoroutine(text, color));
        }
        
        private IEnumerator DisplayTextCoroutine(string text, Color color)
        {
            IsDisplaying = true;
            
            // Set up text
            if (resultText != null)
            {
                resultText.text = text;
                resultText.color = color;
            }
            
            // Position on floor center
            UpdateFloorPosition();
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
            // Display duration
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            IsDisplaying = false;
        }
        
        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            canvasGroup.alpha = 0f;
            
            // Set initial scale
            if (enableScaleAnimation)
            {
                transform.localScale = startScale * 0.01f;
            }
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInDuration;
                
                // Fade alpha
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                
                // Scale animation
                if (enableScaleAnimation)
                {
                    float scaleT = scaleCurve.Evaluate(t);
                    Vector3 currentScale = Vector3.Lerp(startScale, targetScale, scaleT);
                    transform.localScale = currentScale * 0.01f;
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            if (enableScaleAnimation)
            {
                transform.localScale = targetScale * 0.01f;
            }
        }
        
        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = transform.localScale;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                
                // Fade alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                
                // Scale animation
                if (enableScaleAnimation)
                {
                    float scaleT = scaleCurve.Evaluate(1f - t);
                    Vector3 currentScale = Vector3.Lerp(this.startScale, targetScale, scaleT);
                    transform.localScale = currentScale * 0.01f;
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        private void UpdateFloorPosition()
        {
            if (floorCenter != null)
            {
                Vector3 floorPosition = floorCenter.position + Vector3.up * heightOffset;
                transform.position = floorPosition;
            }
        }
        
        private void UpdateCameraFacing()
        {
            if (mainCamera == null) return;
            
            // Make canvas face camera
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 directionToCamera = (cameraPosition - transform.position).normalized;
            
            // Only rotate around Y axis to keep text upright
            directionToCamera.y = 0;
            
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
        
        public void HideDisplay()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            if (resultText != null)
            {
                resultText.text = "";
            }
            
            IsDisplaying = false;
        }
        
        public void ClearDisplay()
        {
            HideDisplay();
        }
        
        public void SetDisplayDuration(float duration)
        {
            displayDuration = Mathf.Max(0.1f, duration);
        }
        
        public void SetTextColor(Color color)
        {
            textColor = color;
            if (resultText != null)
            {
                resultText.color = color;
            }
        }
        
        public void SetTextSize(float size)
        {
            textSize = Mathf.Max(12f, size);
            if (resultText != null)
            {
                resultText.fontSize = textSize;
            }
        }
        
        public void SetFloorReference(Transform floor)
        {
            floorCenter = floor;
            if (!IsDisplaying)
            {
                UpdateFloorPosition();
            }
        }
        
        // Public methods for different result types
        public void DisplaySuccess(int value)
        {
            DisplayResult(value, successColor);
        }
        
        public void DisplayFailure(string message)
        {
            DisplayCustomText(message, failColor);
        }
        
        public void DisplayCriticalSuccess(int value)
        {
            DisplayCustomText($"CRITICAL! {resultPrefix}{value}", successColor);
        }
        
        private void OnValidate()
        {
            displayDuration = Mathf.Max(0.1f, displayDuration);
            fadeInDuration = Mathf.Max(0.1f, fadeInDuration);
            fadeOutDuration = Mathf.Max(0.1f, fadeOutDuration);
            textSize = Mathf.Max(12f, textSize);
            heightOffset = Mathf.Max(0f, heightOffset);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (floorCenter != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(floorCenter.position + Vector3.up * heightOffset, 0.5f);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(floorCenter.position, floorCenter.position + Vector3.up * heightOffset);
            }
        }
    }
}