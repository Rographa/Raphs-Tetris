using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardsManager : MonoBehaviour
{
    public static LeaderboardsManager Instance;
    private const string PrivateCode = "c4FS0YmN30Cn2Y8GBorAnwSR87bdxTUkeaM8g5auYOaA";
    private const string PublicCode = "60f3be958f40bb8ea037a93b";
    private const string WebURL = "http://dreamlo.com/lb/";
    public Highscore[] HighscoresList;
    public delegate void LeaderboardsEvents();
    public static event LeaderboardsEvents OnScoresLoaded;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }   
    public void AddNewHighscore(string username, int score)
    {
        StartCoroutine(UploadNewHighscore(username, score));
    }

    private IEnumerator UploadNewHighscore(string username, int score)
    {
        var www = new WWW(WebURL + PrivateCode + "/add/" + WWW.EscapeURL(username) + "/" + score);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
            print("Upload Successful");
        else
        {
            print("Error uploading: " + www.error);
        }
        DownloadHighscores();
    }
    public void DownloadHighscores()
    {
        StartCoroutine(DownloadHighscoresFromDatabase());
    }

    private IEnumerator DownloadHighscoresFromDatabase()
    {
        var www = new WWW(WebURL + PublicCode + "/pipe/");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
            FormatHighscores(www.text);
        else
        {
            print("Error Downloading: " + www.error);
        }
    }

    private void FormatHighscores(string textStream)
    {
        var entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        HighscoresList = new Highscore[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            var entryInfo = entries[i].Split(new char[] { '|' });
            var username = entryInfo[0];
            var score = int.Parse(entryInfo[1]);
            HighscoresList[i] = new Highscore(username, score);
            print(HighscoresList[i].Username + ": " + HighscoresList[i].Score);
        }
        OnScoresLoaded?.Invoke();
    }

}

public struct Highscore
{
    public readonly string Username;
    public readonly int Score;

    public Highscore(string username, int score)
    {
        Username = username;
        Score = score;
    }

}
