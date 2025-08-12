using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.Managers;

namespace DiceGame.UI
{
    /// <summary>
    /// Main UI manager that coordinates all UI elements for the dice game
    /// Handles Screen Space overlay canvas setup and UI component initialization
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Canvas")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private CanvasScaler canvasScaler;
        
        [Header("Score UI")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private GameObject scoreUIPrefab;
        
        [Header("Other UI")]
        [SerializeField] private ResultDisplayUI resultDisplay;
        
        public static UIManager Instance { get; private set; }
        public ScoreManager ScoreManager => scoreManager;
        public ResultDisplayUI ResultDisplay => resultDisplay;
        
        private void Awake()
        {
            SetupSingleton();
            InitializeCanvas();
            SetupUIComponents();
        }
        
        private void Start()
        {
            // Connect to game manager events if needed
            if (GameManager.Instance != null)
            {
                // Additional event connections can go here
            }
        }
        
        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void InitializeCanvas()
        {
            if (mainCanvas == null)
            {
                mainCanvas = GetComponent<Canvas>();
                if (mainCanvas == null)
                {
                    mainCanvas = gameObject.AddComponent<Canvas>();
                }
            }
            
            // Set up Screen Space - Overlay canvas for UI
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;
            
            // Set up canvas scaler for responsive UI
            if (canvasScaler == null)
            {
                canvasScaler = GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = gameObject.AddComponent<CanvasScaler>();
                }
            }
            
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            
            // Add GraphicRaycaster if missing
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        private void SetupUIComponents()
        {
            // Set up score manager
            if (scoreManager == null)
            {
                scoreManager = GetComponentInChildren<ScoreManager>();
                if (scoreManager == null)
                {
                    CreateScoreUI();
                }
            }
            
            // Find result display if not assigned
            if (resultDisplay == null)
            {
                resultDisplay = FindObjectOfType<ResultDisplayUI>();
            }
        }
        
        private void CreateScoreUI()
        {
            // Create score UI programmatically if prefab is available
            if (scoreUIPrefab != null)
            {
                GameObject scoreUIObject = Instantiate(scoreUIPrefab, transform);
                scoreManager = scoreUIObject.GetComponent<ScoreManager>();
            }
            else
            {
                // Create score UI from scratch
                GameObject scoreUIObject = new GameObject("ScoreUI");
                scoreUIObject.transform.SetParent(transform, false);
                
                // Add TextMeshPro component
                TextMeshProUGUI scoreText = scoreUIObject.AddComponent<TextMeshProUGUI>();
                scoreText.text = "Score: 0";
                scoreText.fontSize = 24f;
                scoreText.color = Color.white;
                scoreText.alignment = TextAlignmentOptions.TopRight;
                
                // Position at top right
                RectTransform rectTransform = scoreText.rectTransform;
                rectTransform.anchorMin = new Vector2(1f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(1f, 1f);
                rectTransform.anchoredPosition = new Vector2(-20f, -20f);
                rectTransform.sizeDelta = new Vector2(200f, 50f);
                
                // Add ScoreManager component
                scoreManager = scoreUIObject.AddComponent<ScoreManager>();
            }
        }
        
        public void ShowScore(int score)
        {
            if (scoreManager != null)
            {
                // This will be handled automatically by the ScoreManager through events
            }
        }
        
        public void ShowRollAgain()
        {
            if (scoreManager != null)
            {
                // This will be handled automatically by the ScoreManager through events
            }
        }
        
        public void SetScoreColors(Color normalColor, Color invalidColor)
        {
            if (scoreManager != null)
            {
                scoreManager.SetScoreColors(normalColor, invalidColor);
            }
        }
        
        public void DisplayResult(int result)
        {
            if (resultDisplay != null)
            {
                if (result == -1)
                {
                    resultDisplay.DisplayFailure("Roll Again!");
                }
                else
                {
                    resultDisplay.DisplayResult(result);
                }
            }
        }
        
        private void OnValidate()
        {
            // Ensure canvas is properly configured
            if (mainCanvas != null && mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
    }
}