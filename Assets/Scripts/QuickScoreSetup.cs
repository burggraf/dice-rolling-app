using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DiceGame.Managers;

/// <summary>
/// Quick and simple score setup - just add this to any GameObject and it will create the score display
/// </summary>
public class QuickScoreSetup : MonoBehaviour
{
    void Start()
    {
        CreateScoreUI();
    }
    
    void CreateScoreUI()
    {
        // Check if UI already exists
        if (FindObjectOfType<TextMeshProUGUI>() != null)
        {
            Debug.Log("UI already exists in scene");
            return;
        }
        
        Debug.Log("Creating score UI...");
        
        // Create Canvas
        GameObject canvasGO = new GameObject("Score Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create Score Text
        GameObject scoreGO = new GameObject("Score Text");
        scoreGO.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
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
        
        // Add the score logic
        SimpleScoreDisplay scoreDisplay = scoreGO.AddComponent<SimpleScoreDisplay>();
        
        Debug.Log("Score UI created successfully!");
    }
}

/// <summary>
/// Simple score display that listens to GameManager events
/// </summary>
public class SimpleScoreDisplay : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private GameManager gameManager;
    
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager != null)
        {
            gameManager.OnResultCalculated.AddListener(UpdateScore);
            Debug.Log("Connected to GameManager events");
        }
        else
        {
            Debug.LogWarning("GameManager not found - score won't update automatically");
        }
    }
    
    void UpdateScore(int result)
    {
        if (scoreText != null)
        {
            if (result == -1)
            {
                scoreText.text = "Roll Again";
                scoreText.color = Color.red;
            }
            else
            {
                scoreText.text = "Score: " + result;
                scoreText.color = Color.white;
            }
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnResultCalculated.RemoveListener(UpdateScore);
        }
    }
    
    // Test methods you can call from the inspector
    [ContextMenu("Test Valid Score")]
    void TestValidScore()
    {
        UpdateScore(Random.Range(2, 13));
    }
    
    [ContextMenu("Test Invalid Score")]
    void TestInvalidScore()
    {
        UpdateScore(-1);
    }
}