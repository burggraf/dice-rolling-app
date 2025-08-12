using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.Managers;

namespace DiceGame.UI
{
    /// <summary>
    /// Manages the score display at the top right of the screen
    /// Shows dice roll totals and handles invalid roll cases
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private string scorePrefix = "Score: ";
        [SerializeField] private string rollAgainText = "Roll Again";
        [SerializeField] private Color normalScoreColor = Color.white;
        [SerializeField] private Color invalidRollColor = Color.red;
        
        [Header("UI Positioning")]
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private Vector2 topRightOffset = new Vector2(-20f, -20f);
        
        
        private Canvas canvas;
        private GameManager gameManager;
        private int currentScore = 0;
        private bool hasInvalidDice = false;

        public int CurrentScore => currentScore;
        public bool HasInvalidDice => hasInvalidDice;

        private void Awake()
        {
            InitializeComponents();
            SetupUI();
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnResultCalculated.AddListener(HandleGameResult);
                gameManager.OnRollStart.AddListener(ResetScore);
            }
            
            UpdateScoreDisplay();
        }

        private void InitializeComponents()
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("ScoreManager must be child of a Canvas component");
            }

            if (canvasRectTransform == null)
            {
                canvasRectTransform = canvas?.GetComponent<RectTransform>();
            }

            if (scoreText == null)
            {
                scoreText = GetComponent<TextMeshProUGUI>();
                if (scoreText == null)
                {
                    scoreText = GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }

        private void SetupUI()
        {
            if (scoreText == null) return;

            // Position at top right of screen
            RectTransform rectTransform = scoreText.rectTransform;
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = topRightOffset;

            // Set text properties
            scoreText.color = normalScoreColor;
            scoreText.fontSize = 24f;
            scoreText.alignment = TextAlignmentOptions.TopRight;
        }

        private void HandleGameResult(int totalResult)
        {
            // GameManager sends -1 for invalid rolls
            if (totalResult == -1)
            {
                ShowInvalidRoll();
            }
            else
            {
                ShowValidScore(totalResult);
            }
        }


        private void ShowValidScore(int score)
        {
            currentScore = score;
            hasInvalidDice = false;
            
            if (scoreText != null)
            {
                scoreText.text = scorePrefix + score.ToString();
                scoreText.color = normalScoreColor;
            }

        }

        private void ShowInvalidRoll()
        {
            currentScore = 0;
            hasInvalidDice = true;
            
            if (scoreText != null)
            {
                scoreText.text = rollAgainText;
                scoreText.color = invalidRollColor;
            }

        }

        private void ResetScore()
        {
            currentScore = 0;
            hasInvalidDice = false;
            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = scorePrefix + currentScore.ToString();
                scoreText.color = normalScoreColor;
            }
        }


        public void SetScoreColors(Color normalColor, Color invalidColor)
        {
            normalScoreColor = normalColor;
            invalidRollColor = invalidColor;
            
            if (scoreText != null && !hasInvalidDice)
            {
                scoreText.color = normalColor;
            }
        }


        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnResultCalculated.RemoveListener(HandleGameResult);
                gameManager.OnRollStart.RemoveListener(ResetScore);
            }
        }
    }
}