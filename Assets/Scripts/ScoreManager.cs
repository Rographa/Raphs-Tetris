using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] int scorePerLine;
    [SerializeField] float animationDuration = 0.5f;
    Coroutine scoreAnimation;
    int score;
    int animScore;
    int previousScore;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    public void Score(float multiplier = 1)
    {
        previousScore = score;
        score += Mathf.RoundToInt(scorePerLine * multiplier);
        if (scoreAnimation != null) StopCoroutine(scoreAnimation);
        scoreAnimation = StartCoroutine(ScoreAnimation());
    }
    IEnumerator ScoreAnimation()
    {
        if (animScore != 0) previousScore = animScore;
        for (float i = 0; i < 1; i += Time.deltaTime / animationDuration)
        {
            animScore = Mathf.RoundToInt(Mathf.Lerp(previousScore, score, i));
            scoreText.SetText(animScore.ToString());
            yield return new WaitForEndOfFrame();
        }
        animScore = score;
        scoreText.SetText(animScore.ToString());
        scoreAnimation = null;
    }
    public int GetScore()
    {
        return score;
    }
}
