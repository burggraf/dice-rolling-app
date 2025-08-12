using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.UI;
using DiceGame.Managers;

namespace DiceGame.Utils
{
    /// <summary>
    /// Utility script to automatically set up the score UI in the scene
    /// Run this once to create all necessary UI components
    /// </summary>
    public class ScoreUISetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createEventSystem = true;
        
        [Header("UI Styling")]
        [SerializeField] private Font uiFont;
        [SerializeField] private int scoreFontSize = 32;
        [SerializeField] private Color scoreColor = Color.white;
        [SerializeField] private Color invalidColor = Color.red;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupScoreUI();
            }
        }
        
        [ContextMenu("Setup Score UI")]
        public void SetupScoreUI()
        {
            Debug.Log("Setting up Score UI...");
            
            // 1. Create Event System if needed
            if (createEventSystem && FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                CreateEventSystem();
            }
            
            // 2. Find or create main canvas
            Canvas mainCanvas = FindOrCreateMainCanvas();
            
            // 3. Create UIManager
            UIManager uiManager = CreateUIManager(mainCanvas);
            
            // 4. Create Score UI
            CreateScoreDisplay(mainCanvas);
            
            // 5. Connect to GameManager
            ConnectToGameManager();
            
            Debug.Log("Score UI setup complete!");
        }
        
        private void CreateEventSystem()
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }
        
        private Canvas FindOrCreateMainCanvas()
        {
            // Look for existing canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log($"Found existing canvas: {canvas.name}");
                    return canvas;
                }
            }
            
            // Create new canvas
            GameObject canvasGO = new GameObject("UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            Debug.Log("Created new UI Canvas");
            return canvas;
        }
        
        private UIManager CreateUIManager(Canvas canvas)
        {
            UIManager existing = canvas.GetComponent<UIManager>();
            if (existing != null)
            {
                Debug.Log("UIManager already exists");
                return existing;
            }
            
            UIManager uiManager = canvas.gameObject.AddComponent<UIManager>();
            Debug.Log("Created UIManager");
            return uiManager;
        }
        
        private void CreateScoreDisplay(Canvas canvas)
        {
            // Check if score display already exists
            ScoreManager existingScore = FindObjectOfType<ScoreManager>();
            if (existingScore != null)
            {
                Debug.Log("ScoreManager already exists");
                return;
            }
            
            // Create score display GameObject
            GameObject scoreGO = new GameObject("Score Display");
            scoreGO.transform.SetParent(canvas.transform, false);
            
            // Add TextMeshPro component
            TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = scoreFontSize;
            scoreText.color = scoreColor;
            scoreText.alignment = TextAlignmentOptions.TopRight;
            scoreText.fontStyle = FontStyles.Bold;
            
            // Position at top right
            RectTransform rectTransform = scoreText.rectTransform;
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-30f, -30f);
            rectTransform.sizeDelta = new Vector2(300f, 60f);
            
            // Add ScoreManager component
            ScoreManager scoreManager = scoreGO.AddComponent<ScoreManager>();
            
            Debug.Log("Created Score Display");
        }
        
        private void ConnectToGameManager()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found in scene. Make sure GameManager exists.");
                return;
            }
            
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogWarning("ScoreManager not found after setup.");
                return;
            }
            
            Debug.Log("Score system connected to GameManager");
        }
        
        [ContextMenu("Test Score Display")]
        public void TestScoreDisplay()
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogError("No ScoreManager found. Run Setup Score UI first.");
                return;
            }
            
            // Test valid score
            Debug.Log("Testing valid score display...");
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnResultCalculated?.Invoke(7); // Test score of 7
                
                // Test invalid roll after 2 seconds
                Invoke(nameof(TestInvalidRoll), 2f);
            }
        }
        
        private void TestInvalidRoll()
        {
            Debug.Log("Testing invalid roll display...");
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnResultCalculated?.Invoke(-1); // Invalid roll
            }
        }
        
        [ContextMenu("Remove Score UI")]
        public void RemoveScoreUI()
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                DestroyImmediate(scoreManager.gameObject);
                Debug.Log("Removed Score UI");
            }
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                DestroyImmediate(uiManager);
                Debug.Log("Removed UIManager");
            }
        }
    }
}