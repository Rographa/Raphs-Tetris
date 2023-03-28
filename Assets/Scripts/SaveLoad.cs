using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    private static PlayerInfo _playerInfo = new PlayerInfo();

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/tetris.raph"))
        {
            var bf = new BinaryFormatter();
            var file = File.Open(Application.persistentDataPath + "/tetris.raph", FileMode.Open);
            _playerInfo = (PlayerInfo)bf.Deserialize(file);
            file.Close();
            PlayerInfo.Data = _playerInfo;
        }
        else
        {
            Save();
        }
    }

    public static void Save()
    {
        PlayerInfo.Data ??= new PlayerInfo();
        _playerInfo = PlayerInfo.Data;
        var bf = new BinaryFormatter();
        var file = File.Create(Application.persistentDataPath + "/tetris.raph");
        bf.Serialize(file, _playerInfo);
        file.Close();
    }
}
