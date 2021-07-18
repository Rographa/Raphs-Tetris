using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
public class MenuController : MonoBehaviour
{    
    [SerializeField] Transform title;
    [SerializeField] Transform play;
    [SerializeField] Transform exit;
    [SerializeField] Transform credits;
    [SerializeField] Transform highScore;
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI[] topPlayers;
    [SerializeField] TMP_InputField playerNameInput;
    [SerializeField] Image fade;
    [SerializeField] AudioClip btnHighlight;
    [SerializeField] AudioClip btnPressed;
    [SerializeField] AudioSource source;
    [SerializeField] Animator leaderboardsAnim;

    public string playerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player");
        set => PlayerPrefs.SetString("PlayerName", value);
    }
    private void Awake()
    {
        SaveLoad.Load();
        playerNameInput.text = playerName;
        score.SetText(PlayerInfo.playerInfo.HighScore.ToString());      
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
        LeaderboardsManager.instance.DownloadHighscores();
        StartCoroutine(StartAnimation());        
    }
    IEnumerator StartAnimation()
    {
        float startYTitle = title.position.y;
        float startYExit = exit.position.y;
        float startYPlay = play.position.y;
        float startYCredits = credits.position.y;
        float startYHighscore = highScore.position.y;
        float startYLeaderboards = leaderboardsAnim.transform.position.y;
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
    public void ChangeName(string _name)
    {
        playerName = _name;
    }
    public void RemoveSpaces()
    {
        playerNameInput.text = playerNameInput.text.Replace(" ", "");
    }
    public void PlayButton()
    {
        PressedSFX();
        StartCoroutine(LoadScene());
    }
    public void ExitButton()
    {
        PressedSFX();
        Application.Quit();
    }
    public void HighlightSFX()
    {
        source.PlayOneShot(btnHighlight);
    }
    public void PressedSFX()
    {
        source.PlayOneShot(btnPressed);
    }
    IEnumerator LoadScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
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
    void Fade()
    {
        Color color = fade.color;
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
    public void SetLeaderboards()
    {
        Highscore[] highscoresList = LeaderboardsManager.instance.highscoresList;
        for (int i = 0; i < topPlayers.Length; i++)
        {
            if (highscoresList.Length <= i) break;
            Highscore hs = highscoresList[i];
            topPlayers[i].SetText($"{i + 1}. {hs.username} - {hs.score}");
        }
    }
}


#endregion
