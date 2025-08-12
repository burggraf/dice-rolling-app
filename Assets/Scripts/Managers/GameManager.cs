using UnityEngine;
using UnityEngine.Events;
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
        
        [Header("Events")]
        public UnityEvent OnGameStart = new UnityEvent();
        public UnityEvent OnRollStart = new UnityEvent();
        public UnityEvent OnRollComplete = new UnityEvent();
        public UnityEvent<int> OnResultCalculated = new UnityEvent<int>();
        
        private GameState currentState = GameState.Ready;
        private float autoRollTimer;
        
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
            int totalResult = 0;
            
            foreach (var dice in diceControllers)
            {
                totalResult += dice.GetDiceValue();
            }
            
            Debug.Log($"Dice roll result: {totalResult}");
            OnResultCalculated?.Invoke(totalResult);
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
        }
    }
}