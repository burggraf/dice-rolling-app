using UnityEngine;
  using DiceGame.Managers;

  public class SimpleInputHandler : MonoBehaviour
  {
      void Update()
      {
          // Check for mouse click or touch
          if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
          {
              Debug.Log("Touch/Click detected!");

              // Find GameManager automatically
              GameManager gameManager = FindObjectOfType<GameManager>();
              if (gameManager != null)
              {
                  gameManager.RollDice();
              }
          }
      }
  }
