using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Emergency score display - will force create score UI regardless of other scripts
/// </summary>
public class ForceScoreDisplay : MonoBehaviour
{
    void Start()
    {
        Debug.Log("ForceScoreDisplay starting...");
        CreateScoreNow();
    }
    
    void CreateScoreNow()
    {
        Debug.Log("Creating score display...");
        
        // Destroy any existing score canvas first
        Canvas[] existingCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in existingCanvases)
        {
            if (canvas.name.Contains("Score"))
            {
                DestroyImmediate(canvas.gameObject);
            }
        }
        
        // Create Canvas
        GameObject canvasGO = new GameObject("SCORE_CANVAS");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Very high to ensure it's on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create Score Text
        GameObject scoreGO = new GameObject("SCORE_TEXT");
        scoreGO.transform.SetParent(canvas.transform, false);
        
        TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreText.text = "SCORE: 999";
        scoreText.fontSize = 48;
        scoreText.color = Color.yellow;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.TopRight;
        
        // Position at top right with large offset so it's definitely visible
        RectTransform rt = scoreText.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-100f, -100f);
        rt.sizeDelta = new Vector2(400f, 100f);
        
        Debug.Log("Score display created! Should see SCORE: 999 in yellow at top-right");
        
        // Add a simple updater component
        scoreGO.AddComponent<SimpleScoreUpdater>();
    }
}

public class SimpleScoreUpdater : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private int currentScore = 999;
    
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        InvokeRepeating(nameof(UpdateScore), 1f, 2f); // Update every 2 seconds
    }
    
    void UpdateScore()
    {
        if (scoreText != null)
        {
            currentScore = Random.Range(2, 13);
            scoreText.text = "SCORE: " + currentScore;
            scoreText.color = (currentScore > 7) ? Color.green : Color.yellow;
            Debug.Log("Score updated to: " + currentScore);
        }
    }
    
    void Update()
    {
        // Test with keyboard
        if (Input.GetKeyDown(KeyCode.Space))
        {
            scoreText.text = "ROLL AGAIN";
            scoreText.color = Color.red;
            Debug.Log("Showing ROLL AGAIN");
        }
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateScore();
        }
    }
}