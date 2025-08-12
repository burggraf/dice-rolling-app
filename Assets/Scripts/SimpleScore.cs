using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleScore : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SimpleScore starting...");
        
        // Create Canvas
        GameObject canvasGO = new GameObject("ScoreCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // Create Text
        GameObject textGO = new GameObject("ScoreText");
        textGO.transform.SetParent(canvasGO.transform, false);
        
        Text scoreText = textGO.AddComponent<Text>();
        scoreText.text = "SCORE: 42";
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        // Position at top right
        RectTransform rt = scoreText.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-50f, -50f);
        rt.sizeDelta = new Vector2(200f, 50f);
        
        Debug.Log("Score created!");
    }
}