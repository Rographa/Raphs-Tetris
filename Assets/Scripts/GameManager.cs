using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject[] tetrominos;
    [SerializeField] GameObject[] tetrominosNext;
    [SerializeField] Vector3 spawnPos;
    [SerializeField] GameObject pauseCanvas;
    [SerializeField] GameObject gameOverCanvas;
    [SerializeField] TextMeshProUGUI scoreTxt;
    public const int SCREEN_LIMIT_X = 13;
    public const int SCREEN_LIMIT_Y = 22;

    public delegate void LineCompletedEvent();
    public delegate void TetrominoSpawned();
    public static event LineCompletedEvent OnLineCompleted;
    public static event TetrominoSpawned OnTetrominoSpawned;
    public static Transform[,] occupiedSpaces;

    int nextTetrominoIndex = -1;
    bool paused;
    private void Awake()
    {
        if (PlayerInfo.playerInfo is null)
            SaveLoad.Load();
        occupiedSpaces = new Transform[SCREEN_LIMIT_X, SCREEN_LIMIT_Y];
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
    void AddToGrid(Transform objParent)
    {
        for (int i = 0; i < objParent.childCount - 1; i++)
        {
            Transform temp = objParent.GetChild(i);
            int _x = Mathf.RoundToInt(temp.position.x);
            int _y = Mathf.RoundToInt(temp.position.y);
            if (_y >= SCREEN_LIMIT_Y - 1)
            {
                GameOver();
                return;
            }
            occupiedSpaces[_x, _y] = temp;
        }
        StartCoroutine(CheckLines());
        SpawnTetromino();
    }
    IEnumerator CheckLines()
    {
        float multiplier = 1;
        
        for (int i = SCREEN_LIMIT_Y - 1; i >= 0; i--)
        {            
            if (LineExists(i))
            {                
                yield return DeleteElementsRoutine(i);
                AudioManager.instance.PlayClip(AudioManager.Clips.LINE_FINISHED, true);
                scoreManager.Score(multiplier);
                multiplier += 0.5f;
                StartCoroutine(MoveDownRoutine(i));
                yield return new WaitForEndOfFrame();
            }
        }
    }
    bool LineExists(int i)
    {
        for (int j = 0; j < SCREEN_LIMIT_X; j++)
        {            
            if (occupiedSpaces[j, i] is null)
                return false;
        }
        return true;
    }
    IEnumerator DeleteElementsRoutine(int i)
    {
        List<TetrominoPiece> objs = new List<TetrominoPiece>();
        for (int j = 0; j < SCREEN_LIMIT_X; j++)
        {            
            Transform go = occupiedSpaces[j, i];            
            objs.Add(go.GetComponent<TetrominoPiece>());
            occupiedSpaces[j, i] = null;
        }
        yield return new WaitForEndOfFrame();
        foreach(TetrominoPiece piece in objs)
        {
            piece.BlinkAndFade();
        }
        yield return new WaitForSeconds(0.15f);
        OnLineCompleted?.Invoke();        
    }
    IEnumerator MoveDownRoutine(int i)
    {
        List<Transform> objs = new List<Transform>();
        for (int k = i; k < SCREEN_LIMIT_Y - 1; k++)
        {
            for (int j = 0; j < SCREEN_LIMIT_X; j++)
            {                
                if (!ReferenceEquals(occupiedSpaces[j, k], null))
                {
                    occupiedSpaces[j, k - 1] = occupiedSpaces[j, k];
                    occupiedSpaces[j, k] = null;                    
                    objs.Add(occupiedSpaces[j, k - 1]);
                }
            }
            if (objs.Count > 0)
            {                                
                foreach (Transform obj in objs)
                {
                    obj.SendMessage("MovePieceDown", k-1);
                }
                yield return new WaitForEndOfFrame();
            }
            objs.Clear();            
        }
        
        yield return new WaitForEndOfFrame();

    }
    void SpawnTetromino()
    {
        if (nextTetrominoIndex == -1)
        {
            Instantiate(tetrominos[Random.Range(0, tetrominos.Length)], spawnPos, Quaternion.identity);
            SetNextTetromino();
        }
        else
        {
            Instantiate(tetrominos[nextTetrominoIndex], spawnPos, Quaternion.identity);
            SetNextTetromino();
        }
        OnTetrominoSpawned?.Invoke();
    }
    void SetNextTetromino()
    {
        nextTetrominoIndex = Random.Range(0, tetrominos.Length);
        foreach(GameObject i in tetrominosNext)
        {
            if (i.activeInHierarchy) i.SetActive(false);
        }
        tetrominosNext[nextTetrominoIndex].SetActive(true);
    }
    public static bool SpaceOccupied(int x, int y)
    {
        x = Mathf.Clamp(x, 0, SCREEN_LIMIT_X - 1);
        y = Mathf.Clamp(y, 0, SCREEN_LIMIT_Y - 1);
        return !ReferenceEquals(occupiedSpaces[x, y], null);
    }
    public void Pause()
    {
        paused = !paused;
        pauseCanvas.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
    }
    public void GameOver()
    {
        int _score = ScoreManager.instance.GetScore();
        scoreTxt.SetText(_score.ToString());
        LeaderboardsManager.instance.AddNewHighscore(PlayerPrefs.GetString("PlayerName", "Player"), _score);
        PlayerInfo.playerInfo.CheckAndSetHighscore(_score);
        Time.timeScale = 0;
        gameOverCanvas.SetActive(true);
    }
    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);        
    }
}
