using UnityEngine;
using DiceGame.Managers;
using DiceGame.UI;

namespace DiceGame.Utils
{
    /// <summary>
    /// Helper script for testing the score system functionality
    /// Can be attached to any GameObject in the scene for testing
    /// </summary>
    public class ScoreSystemTestHelper : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private KeyCode testValidRollKey = KeyCode.V;
        [SerializeField] private KeyCode testInvalidRollKey = KeyCode.I;
        [SerializeField] private KeyCode rollDiceKey = KeyCode.Space;
        
        [Header("Test Values")]
        [SerializeField] private int testScoreMin = 2;
        [SerializeField] private int testScoreMax = 12;
        
        private GameManager gameManager;
        private UIManager uiManager;
        private bool isTestMode = false;
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            uiManager = UIManager.Instance;
            
            if (enableTestMode)
            {
                isTestMode = true;
                Debug.Log("Score System Test Helper enabled:");
                Debug.Log($"Press {testValidRollKey} to simulate valid roll");
                Debug.Log($"Press {testInvalidRollKey} to simulate invalid roll");
                Debug.Log($"Press {rollDiceKey} to trigger normal dice roll");
            }
        }
        
        private void Update()
        {
            if (!isTestMode) return;
            
            HandleTestInput();
        }
        
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(testValidRollKey))
            {
                SimulateValidRoll();
            }
            else if (Input.GetKeyDown(testInvalidRollKey))
            {
                SimulateInvalidRoll();
            }
            else if (Input.GetKeyDown(rollDiceKey))
            {
                TriggerNormalRoll();
            }
        }
        
        private void SimulateValidRoll()
        {
            int testScore = Random.Range(testScoreMin, testScoreMax + 1);
            Debug.Log($"Simulating valid roll with score: {testScore}");
            
            // Trigger the result event directly
            if (gameManager != null)
            {
                gameManager.OnResultCalculated?.Invoke(testScore);
            }
        }
        
        private void SimulateInvalidRoll()
        {
            Debug.Log("Simulating invalid roll");
            
            // Trigger the result event with -1 to indicate invalid roll
            if (gameManager != null)
            {
                gameManager.OnResultCalculated?.Invoke(-1);
            }
        }
        
        private void TriggerNormalRoll()
        {
            if (gameManager != null)
            {
                Debug.Log("Triggering normal dice roll");
                gameManager.RollDice();
            }
            else
            {
                Debug.LogWarning("GameManager not found for normal roll test");
            }
        }
        
        public void TestScoreDisplay()
        {
            Debug.Log("Testing score display components...");
            
            // Check UIManager
            if (uiManager == null)
            {
                Debug.LogWarning("UIManager not found in scene");
            }
            else
            {
                Debug.Log("UIManager found");
                
                if (uiManager.ScoreManager == null)
                {
                    Debug.LogWarning("ScoreManager not found in UIManager");
                }
                else
                {
                    Debug.Log("ScoreManager found and connected");
                }
            }
            
            // Check GameManager
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found in scene");
            }
            else
            {
                Debug.Log("GameManager found");
            }
            
            // Test score display
            SimulateValidRoll();
            Invoke(nameof(SimulateInvalidRoll), 2f);
        }
        
        [ContextMenu("Test Score System")]
        public void TestScoreSystemFromMenu()
        {
            TestScoreDisplay();
        }
        
        [ContextMenu("Simulate Valid Roll")]
        public void SimulateValidRollFromMenu()
        {
            SimulateValidRoll();
        }
        
        [ContextMenu("Simulate Invalid Roll")]
        public void SimulateInvalidRollFromMenu()
        {
            SimulateInvalidRoll();
        }
        
        private void OnValidate()
        {
            testScoreMin = Mathf.Clamp(testScoreMin, 1, 12);
            testScoreMax = Mathf.Clamp(testScoreMax, testScoreMin, 12);
        }
        
        private void OnGUI()
        {
            if (!isTestMode) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Score System Test Helper", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            
            if (GUILayout.Button($"Valid Roll ({testValidRollKey})"))
            {
                SimulateValidRoll();
            }
            
            if (GUILayout.Button($"Invalid Roll ({testInvalidRollKey})"))
            {
                SimulateInvalidRoll();
            }
            
            if (GUILayout.Button($"Normal Roll ({rollDiceKey})"))
            {
                TriggerNormalRoll();
            }
            
            if (GUILayout.Button("Test All Systems"))
            {
                TestScoreDisplay();
            }
            
            GUILayout.EndArea();
        }
    }
}