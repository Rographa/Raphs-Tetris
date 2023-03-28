using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int scorePerLine;
    [SerializeField] private float animationDuration = 0.5f;
    private Coroutine _scoreAnimation;
    private int _score;
    private int _animScore;
    private int _previousScore;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    public void Score(float multiplier = 1)
    {
        _previousScore = _score;
        _score += Mathf.RoundToInt(scorePerLine * multiplier);
        if (_scoreAnimation != null) StopCoroutine(_scoreAnimation);
        _scoreAnimation = StartCoroutine(ScoreAnimation());
    }
    private IEnumerator ScoreAnimation()
    {
        if (_animScore != 0) _previousScore = _animScore;
        for (float i = 0; i < 1; i += Time.deltaTime / animationDuration)
        {
            _animScore = Mathf.RoundToInt(Mathf.Lerp(_previousScore, _score, i));
            scoreText.SetText(_animScore.ToString());
            yield return new WaitForEndOfFrame();
        }
        _animScore = _score;
        scoreText.SetText(_animScore.ToString());
        _scoreAnimation = null;
    }
    public int GetScore()
    {
        return _score;
    }
}
