using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PlayerInfo
{
    public static PlayerInfo playerInfo;
    public int HighScore
    {
        get;
        private set;
    }
    public PlayerInfo()
    {
        HighScore = 0;
    }
    public void CheckAndSetHighscore(int currentScore)
    {
        if (currentScore > HighScore)
        {
            HighScore = currentScore;
            SaveLoad.Save();
        }
    }
}
