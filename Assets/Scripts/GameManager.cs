using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameObject[] tetrominos;
    [SerializeField] private GameObject[] tetrominosNext;
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    public const int ScreenLimitX = 13;
    public const int ScreenLimitY = 22;

    public delegate void LineCompletedEvent();
    public delegate void TetrominoSpawned();
    public static event LineCompletedEvent OnLineCompleted;
    public static event TetrominoSpawned OnTetrominoSpawned;
    private static Transform[,] _occupiedSpaces;

    private int _nextTetrominoIndex = -1;
    private bool _paused;
    private void Awake()
    {
        if (PlayerInfo.Data is null)
            SaveLoad.Load();
        _occupiedSpaces = new Transform[ScreenLimitX, ScreenLimitY];
        SpawnTetromino();
    }
    private void OnEnable()
    {        
        Tetromino.OnFinished += AddToGrid;
        InputManager.EscPressed += Pause;
    }
    private void OnDisable()
    {
        Tetromino.OnFinished -= AddToGrid;
        InputManager.EscPressed -= Pause;
    }    
    private void AddToGrid(Transform objParent)
    {
        for (var i = 0; i < objParent.childCount - 1; i++)
        {
            var obj = objParent.GetChild(i);
            var pos = obj.position;
            var x = Mathf.RoundToInt(pos.x);
            var y = Mathf.RoundToInt(pos.y);
            if (y >= ScreenLimitY - 1)
            {
                GameOver();
                return;
            }
            _occupiedSpaces[x, y] = obj;
        }
        StartCoroutine(CheckLines());
        SpawnTetromino();
    }
    private IEnumerator CheckLines()
    {
        var multiplier = 1f;
        
        for (var i = ScreenLimitY - 1; i >= 0; i--)
        {
            if (!LineExists(i)) continue;
            
            yield return DeleteElementsRoutine(i);
            AudioManager.Instance.PlayClip(AudioManager.Clips.LINE_FINISHED, true);
            scoreManager.Score(multiplier);
            multiplier += 0.5f;
            StartCoroutine(MoveDownRoutine(i));
            yield return new WaitForEndOfFrame();
        }
    }
    private static bool LineExists(int i)
    {
        for (var j = 0; j < ScreenLimitX; j++)
        {            
            if (_occupiedSpaces[j, i] == null)
                return false;
        }
        return true;
    }
    private IEnumerator DeleteElementsRoutine(int i)
    {
        var objs = new List<TetrominoPiece>();
        for (var j = 0; j < ScreenLimitX; j++)
        {            
            var obj = _occupiedSpaces[j, i];            
            objs.Add(obj.GetComponent<TetrominoPiece>());
            _occupiedSpaces[j, i] = null;
        }
        yield return new WaitForEndOfFrame();
        
        foreach (var piece in objs)
        {
            piece.BlinkAndFade();
        }
        yield return new WaitForSeconds(0.15f);
        OnLineCompleted?.Invoke();        
    }
    private IEnumerator MoveDownRoutine(int i)
    {
        var objs = new List<Transform>();
        for (var k = i; k < ScreenLimitY - 1; k++)
        {
            for (var j = 0; j < ScreenLimitX; j++)
            {
                if (_occupiedSpaces[j, k] == null) continue;
                
                _occupiedSpaces[j, k - 1] = _occupiedSpaces[j, k];
                _occupiedSpaces[j, k] = null;                    
                objs.Add(_occupiedSpaces[j, k - 1]);
            }
            if (objs.Count > 0)
            {
                objs.ForEach(t => t.GetComponent<TetrominoPiece>().MovePieceDown(k - 1));
                yield return new WaitForEndOfFrame();
            }
            objs.Clear();            
        }
        
        yield return new WaitForEndOfFrame();

    }
    private void SpawnTetromino()
    {
        if (_nextTetrominoIndex == -1)
        {
            Instantiate(tetrominos[Random.Range(0, tetrominos.Length)], spawnPos, Quaternion.identity);
            SetNextTetromino();
        }
        else
        {
            Instantiate(tetrominos[_nextTetrominoIndex], spawnPos, Quaternion.identity);
            SetNextTetromino();
        }
        OnTetrominoSpawned?.Invoke();
    }
    private void SetNextTetromino()
    {
        _nextTetrominoIndex = Random.Range(0, tetrominos.Length);
        foreach (var i in tetrominosNext)
        {
            if (i.activeInHierarchy) i.SetActive(false);
        }
        tetrominosNext[_nextTetrominoIndex].SetActive(true);
    }
    public static bool SpaceOccupied(int x, int y)
    {
        x = Mathf.Clamp(x, 0, ScreenLimitX - 1);
        y = Mathf.Clamp(y, 0, ScreenLimitY - 1);
        return !ReferenceEquals(_occupiedSpaces[x, y], null);
    }

    private void Pause()
    {
        _paused = !_paused;
        pauseCanvas.SetActive(_paused);
        Time.timeScale = _paused ? 0 : 1;
    }

    private void GameOver()
    {
        var score = ScoreManager.Instance.GetScore();
        scoreTxt.SetText(score.ToString());
        LeaderboardsManager.Instance.AddNewHighscore(PlayerPrefs.GetString("PlayerName", "Player"), score);
        PlayerInfo.Data.CheckAndSetHighscore(score);
        Time.timeScale = 0;
        gameOverCanvas.SetActive(true);
    }
    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);        
    }
}
