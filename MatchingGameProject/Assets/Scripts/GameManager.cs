using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;  // Reference to the BoardManager
    public Text scoreText;  // UI Text for score display
    public Text levelText;  // UI Text for level display
    private int score = 0;  // Current score
    private int level = 1;  // Current level

    void Start()
    {
        UpdateUI();
    }

    // Update the score and level UI
    public void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        levelText.text = "Level: " + level;
    }

    // Method to increase the score
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    // Method to handle level progress
    public void LevelUp()
    {
        level++;
        boardManager.GenerateBoard();  // Generate a new board for the next level
        UpdateUI();
    }

}