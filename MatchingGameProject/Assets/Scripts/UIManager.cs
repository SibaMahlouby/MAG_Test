using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    private int score = 0;

    public void AddScore(int value)
    {
        score += value;
        scoreText.text = "Score: " + score;
        scoreText.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo); // Score bounce effect
    }
}
