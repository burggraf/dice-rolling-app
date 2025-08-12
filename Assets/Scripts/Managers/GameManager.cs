using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DiceGame.Physics;

namespace DiceGame.Managers
{
    /// <summary>
    /// Main game manager handling overall game state and dice roll coordination
    /// Singleton pattern for global access
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private int numberOfDice = 2;
        [SerializeField] private bool autoRollEnabled = false;
        [SerializeField] private float autoRollInterval = 5f;
        
        [Header("Game Objects")]
        [SerializeField] private DiceController[] diceControllers;
        [SerializeField] private Transform rollStartPosition;
        
        [Header("Score Display")]
        [SerializeField] private bool createScoreDisplay = true;
        
        [Header("Board Boundaries")]
        [SerializeField] private Transform boardCenter;
        [SerializeField] private Vector3 boardSize = new Vector3(10f, 2f, 10f);
        [SerializeField] private bool debugBounds = false;
        
        [Header("Events")]
        public UnityEvent OnGameStart = new UnityEvent();
        public UnityEvent OnRollStart = new UnityEvent();
        public UnityEvent OnRollComplete = new UnityEvent();
        public UnityEvent<int> OnResultCalculated = new UnityEvent<int>();
        
        private GameState currentState = GameState.Ready;
        private float autoRollTimer;
        
        // Score display components
        private Canvas scoreCanvas;
        private TextMeshProUGUI scoreText;
        
        public enum GameState
        {
            Ready,
            Rolling,
            Settling,
            DisplayingResult,
            Paused
        }
        
        public GameState CurrentState => currentState;
        public int NumberOfDice => numberOfDice;
        
        private void Awake()
        {
            SetupSingleton();
            InitializeGame();
        }
        
        private void Start()
        {
            if (createScoreDisplay)
            {
                CreateScoreUI();
            }
            OnGameStart?.Invoke();
        }
        
        private void Update()
        {
            HandleAutoRoll();
            UpdateGameState();
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
        
        private void InitializeGame()
        {
            if (diceControllers == null || diceControllers.Length == 0)
            {
                diceControllers = FindObjectsOfType<DiceController>();
            }
            
            foreach (var dice in diceControllers)
            {
                dice.OnDiceSettled.AddListener(CheckAllDiceSettled);
            }
        }
        
        private void CreateScoreUI()
        {
            // Create Canvas
            GameObject canvasGO = new GameObject("Score Canvas");
            scoreCanvas = canvasGO.AddComponent<Canvas>();
            scoreCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scoreCanvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create Score Text
            GameObject scoreGO = new GameObject("Score Text");
            scoreGO.transform.SetParent(scoreCanvas.transform, false);
            
            scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = 36;
            scoreText.color = Color.white;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.alignment = TextAlignmentOptions.TopRight;
            
            // Position at top right
            RectTransform rt = scoreText.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-50f, -50f);
            rt.sizeDelta = new Vector2(300f, 80f);
            
            Debug.Log("Score UI created successfully!");
        }
        
        public void RollDice()
        {
            if (currentState != GameState.Ready) return;
            
            Debug.Log("Rolling dice...");
            currentState = GameState.Rolling;
            OnRollStart?.Invoke();
            
            foreach (var dice in diceControllers)
            {
                dice.ApplyRollForce();
            }
        }
        
        public void ResetGame()
        {
            Debug.Log("Resetting game...");
            currentState = GameState.Ready;
            
            foreach (var dice in diceControllers)
            {
                dice.ResetDicePosition();
            }
        }
        
        private void HandleAutoRoll()
        {
            if (!autoRollEnabled || currentState != GameState.Ready) return;
            
            autoRollTimer += Time.deltaTime;
            if (autoRollTimer >= autoRollInterval)
            {
                RollDice();
                autoRollTimer = 0f;
            }
        }
        
        private void UpdateGameState()
        {
            switch (currentState)
            {
                case GameState.Rolling:
                    if (AllDiceSettled())
                    {
                        currentState = GameState.Settling;
                    }
                    break;
                    
                case GameState.Settling:
                    currentState = GameState.DisplayingResult;
                    CalculateAndDisplayResult();
                    break;
                    
                case GameState.DisplayingResult:
                    // Wait for UI display time, then return to ready
                    Invoke(nameof(ReturnToReady), 3f);
                    break;
            }
        }
        
        private void CheckAllDiceSettled()
        {
            if (currentState == GameState.Rolling && AllDiceSettled())
            {
                OnRollComplete?.Invoke();
            }
        }
        
        private bool AllDiceSettled()
        {
            foreach (var dice in diceControllers)
            {
                if (!dice.IsSettled)
                    return false;
            }
            return true;
        }
        
        private void CalculateAndDisplayResult()
        {
            // Check if all dice are valid (not touching walls, within bounds)
            bool allDiceValid = ValidateAllDice();
            
            if (allDiceValid)
            {
                int totalResult = 0;
                
                foreach (var dice in diceControllers)
                {
                    totalResult += dice.GetDiceValue();
                }
                
                Debug.Log($"Valid dice roll result: {totalResult}");
                OnResultCalculated?.Invoke(totalResult);
                UpdateScoreDisplay(totalResult);
            }
            else
            {
                Debug.Log("Invalid dice roll - dice touching wall or out of bounds");
                OnResultCalculated?.Invoke(-1); // Use -1 to indicate invalid roll
                UpdateScoreDisplay(-1);
            }
        }
        
        private bool ValidateAllDice()
        {
            foreach (var dice in diceControllers)
            {
                if (!ValidateDicePosition(dice))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool ValidateDicePosition(DiceController dice)
        {
            // Check if dice is touching walls
            if (IsDiceTouchingWall(dice))
            {
                return false;
            }
            
            // Check if dice is within board boundaries
            if (IsDiceOutsideBounds(dice))
            {
                return false;
            }
            
            return true;
        }
        
        private bool IsDiceTouchingWall(DiceController dice)
        {
            Collider diceCollider = dice.GetComponent<Collider>();
            if (diceCollider == null) return false;

            // Check for wall collisions using raycast in all directions
            Vector3 center = diceCollider.bounds.center;
            float checkDistance = diceCollider.bounds.size.magnitude * 0.6f;
            
            Vector3[] directions = {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right
            };
            
            foreach (Vector3 direction in directions)
            {
                if (Physics.Raycast(center, direction, checkDistance))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(center, direction, out hit, checkDistance))
                    {
                        if (hit.collider.CompareTag("Wall"))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        private bool IsDiceOutsideBounds(DiceController dice)
        {
            if (boardCenter == null)
            {
                // Fallback boundaries if no board center is set
                Vector3 boardMin = new Vector3(-5f, -1f, -5f);
                Vector3 boardMax = new Vector3(5f, 5f, 5f);
                Vector3 dicePosition = dice.transform.position;
                return dicePosition.x < boardMin.x || dicePosition.x > boardMax.x ||
                       dicePosition.z < boardMin.z || dicePosition.z > boardMax.z ||
                       dicePosition.y < boardMin.y;
            }
            
            Vector3 dicePos = dice.transform.position;
            Vector3 boardMin = boardCenter.position - boardSize * 0.5f;
            Vector3 boardMax = boardCenter.position + boardSize * 0.5f;
            
            bool outsideBounds = dicePos.x < boardMin.x || dicePos.x > boardMax.x ||
                               dicePos.z < boardMin.z || dicePos.z > boardMax.z ||
                               dicePos.y < boardMin.y - 1f; // Allow some margin below board
            
            if (outsideBounds && debugBounds)
            {
                Debug.Log($"Dice {dice.name} is outside bounds. Position: {dicePos}, Bounds: {boardMin} to {boardMax}");
            }
            
            return outsideBounds;
        }
        
        private void UpdateScoreDisplay(int score)
        {
            if (scoreText == null) return;
            
            if (score == -1)
            {
                scoreText.text = "Roll Again";
                scoreText.color = Color.red;
            }
            else
            {
                scoreText.text = "Score: " + score;
                scoreText.color = Color.white;
            }
        }
        
        // Test methods for debugging
        [ContextMenu("Test Valid Score")]
        public void TestValidScore()
        {
            int testScore = Random.Range(2, 13);
            Debug.Log($"Testing valid score: {testScore}");
            UpdateScoreDisplay(testScore);
        }
        
        [ContextMenu("Test Invalid Score")]
        public void TestInvalidScore()
        {
            Debug.Log("Testing invalid score");
            UpdateScoreDisplay(-1);
        }
        
        private void ReturnToReady()
        {
            currentState = GameState.Ready;
            autoRollTimer = 0f;
        }
        
        public void SetAutoRoll(bool enabled)
        {
            autoRollEnabled = enabled;
        }
        
        public void PauseGame()
        {
            currentState = GameState.Paused;
        }
        
        public void ResumeGame()
        {
            currentState = GameState.Ready;
        }
        
        private void OnValidate()
        {
            numberOfDice = Mathf.Clamp(numberOfDice, 1, 6);
            autoRollInterval = Mathf.Max(autoRollInterval, 1f);
            
            if (boardSize.x <= 0) boardSize.x = 10f;
            if (boardSize.y <= 0) boardSize.y = 2f;
            if (boardSize.z <= 0) boardSize.z = 10f;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (boardCenter != null && debugBounds)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(boardCenter.position, boardSize);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(boardCenter.position, 0.3f);
            }
        }
    }
}