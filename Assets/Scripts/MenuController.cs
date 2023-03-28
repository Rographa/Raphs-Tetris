using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
public class MenuController : MonoBehaviour
{    
    [SerializeField] private Transform title;
    [SerializeField] private Transform play;
    [SerializeField] private Transform exit;
    [SerializeField] private Transform credits;
    [SerializeField] private Transform highScore;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI[] topPlayers;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Image fade;
    [SerializeField] private AudioClip btnHighlight;
    [SerializeField] private AudioClip btnPressed;
    [SerializeField] private AudioSource source;
    [SerializeField] private Animator leaderboardsAnim;

    private static string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player");
        set => PlayerPrefs.SetString("PlayerName", value);
    }
    private void Awake()
    {
        SaveLoad.Load();
        playerNameInput.text = PlayerName;
        score.SetText(PlayerInfo.Data.HighScore.ToString());      
    }
    private void OnEnable()
    {
        LeaderboardsManager.OnScoresLoaded += SetLeaderboards;
    }
    private void OnDisable()
    {
        LeaderboardsManager.OnScoresLoaded -= SetLeaderboards;
    }
    private void Start()
    {
        LeaderboardsManager.Instance.DownloadHighscores();
        StartCoroutine(StartAnimation());        
    }

    private IEnumerator StartAnimation()
    {
        var startYTitle = title.position.y;
        var startYExit = exit.position.y;
        var startYPlay = play.position.y;
        var startYCredits = credits.position.y;
        var startYHighscore = highScore.position.y;
        var startYLeaderboards = leaderboardsAnim.transform.position.y;
        leaderboardsAnim.enabled = false;
        exit.DOMoveY(1500, 0, true);
        play.DOMoveY(1500, 0, true);
        title.DOMoveY(1500, 0, true);
        highScore.DOMoveY(1500, 0, true);
        credits.DOMoveY(-1250, 0, true);
        leaderboardsAnim.transform.DOMoveY(-1250, 0, true);
        yield return new WaitForEndOfFrame();
        exit.DOMoveY(startYExit, 1.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        play.DOMoveY(startYPlay, 1.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        highScore.DOMoveY(startYHighscore, 1.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        title.DOMoveY(startYTitle, 1.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        leaderboardsAnim.transform.DOMoveY(startYLeaderboards, 1.25f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        credits.DOMoveY(startYCredits, 1.25f).SetEase(Ease.OutCubic);

        leaderboardsAnim.enabled = true;
    }
    public void ChangeName(string newName)
    {
        PlayerName = newName;
    }
    public void RemoveSpaces()
    {
        playerNameInput.text = playerNameInput.text.Replace(" ", "");
    }
    public void PlayButton()
    {
        PressedSfx();
        StartCoroutine(LoadScene());
    }
    public void ExitButton()
    {
        PressedSfx();
        Application.Quit();
    }
    public void HighlightSfx()
    {
        source.PlayOneShot(btnHighlight);
    }

    private void PressedSfx()
    {
        source.PlayOneShot(btnPressed);
    }

    private IEnumerator LoadScene()
    {
        var async = SceneManager.LoadSceneAsync(1);
        async.allowSceneActivation = false;
        Fade();
        yield return new WaitForSeconds(1.1f);
        while (!async.isDone)
        {            
            if (async.progress >= 0.9f)
            {
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    private void Fade()
    {
        var color = fade.color;
        color.a = 1;
        fade.color = color;
        fade.CrossFadeAlpha(0f, 0f, true);
        fade.CrossFadeAlpha(1, 1f, true);
    }
    #region Highscores

    public void LeaderboardsClicked()
    {
        leaderboardsAnim.SetTrigger("OnClick");
    }

    private void SetLeaderboards()
    {
        var highscoresList = LeaderboardsManager.Instance.HighscoresList;
        for (var i = 0; i < topPlayers.Length; i++)
        {
            if (highscoresList.Length <= i) break;
            var hs = highscoresList[i];
            var text = $"{(i + 1).ToString()}. {hs.Username} - {hs.Score.ToString()}";
            topPlayers[i].SetText(text);
        }
    }
}


#endregion
