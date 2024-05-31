using System.Collections.Generic;
using UnityEngine;

//public struct PlayerValues
//{
//    public string name;
//    public float scale;
//    public float xpos;
//    public float ypos;
//    public float zpos;
//}

public class CSVReaderScript : MonoBehaviour
{
    //private List<PlayerValues> playerValues = new List<PlayerValues>();
    //private TextAsset csvFile;

    //private void Start()
    //{
    //    csvFile = Resources.Load<TextAsset>("CSV-Files/stats");
    //    ReadCSVFile();
    //}

    //private void ReadCSVFile()
    //{
    //    if (csvFile == null)
    //    {
    //        Debug.LogError("CSV file not loaded");
    //        return;
    //    }

    //    string[] lines = csvFile.text.Split('\n');
    //    // first line is filled with the names of the values
    //    for (int i = 1; i < lines.Length; i++)
    //    {
    //        // if there are empty lines in the file (e.g. at the end) they
    //        // will be ignored
    //        if (string.IsNullOrWhiteSpace(lines[i])) continue;

    //        string[] values = lines[i].Split(';');

    //        PlayerValues currPlaVal;
    //        currPlaVal.name = values[0];
    //        currPlaVal.scale = float.Parse(values[1]);
    //        currPlaVal.xpos = float.Parse(values[2]);
    //        currPlaVal.ypos = float.Parse(values[3]);
    //        currPlaVal.zpos = float.Parse(values[4]);

    //        playerValues.Add(currPlaVal);
    //    }
    //}

    //public PlayerValues GetPlayerValues(string playerName)
    //{
    //    foreach (PlayerValues playerValue in playerValues)
    //    {
    //        if (playerValue.name == playerName)
    //        {
    //            return playerValue;
    //        }
    //    }
    //    Debug.LogError("No player with the specified name found");
    //    return new PlayerValues("default", 0, 0, 0, 0);
    //}

    //public List<PlayerValues> GetAllPlayerValues()
    //{
    //    return playerValues;
    //}

    //public static void WriteCSVFile()
    //{

    //}
}
