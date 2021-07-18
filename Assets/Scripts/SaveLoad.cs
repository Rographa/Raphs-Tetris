using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static PlayerInfo playerInfo = new PlayerInfo();

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/tetris.raph"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/tetris.raph", FileMode.Open);
            playerInfo = (PlayerInfo)bf.Deserialize(file);
            file.Close();
            PlayerInfo.playerInfo = playerInfo;
        }
        else
        {
            Save();
        }
    }

    public static void Save()
    {
        if (ReferenceEquals(PlayerInfo.playerInfo, null))
            PlayerInfo.playerInfo = new PlayerInfo();
        playerInfo = PlayerInfo.playerInfo;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/tetris.raph");
        bf.Serialize(file, playerInfo);
        file.Close();
    }
}
