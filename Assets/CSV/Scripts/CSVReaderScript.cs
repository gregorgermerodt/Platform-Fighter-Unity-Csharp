using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct PlayerValues
{
    public string name;
    public float scale;
    public float xpos;
    public float ypos;
    public float zpos;
}

public class CSVReaderScript : MonoBehaviour
{
    private string csvFilePath = "Assets/CSV/CSV-Files/stats.csv";
    private string completePath;
    private static List<PlayerValues> playerValues = new List<PlayerValues>();

    private void Start()
    {
        completePath = Path.Combine(Application.persistentDataPath, csvFilePath);
        ReadCSVFile();
    }

    private void ReadCSVFile()
    {
        if(!File.Exists(completePath))
        {
            Debug.Log("Datapath not found");
            return;
        }
        string[] lines = File.ReadAllLines(completePath);

        // i = 1, first row contains captions
        for(int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');

            PlayerValues currPlaVal;
            currPlaVal.name = values[0];
            currPlaVal.scale = float.Parse(values[1]);
            currPlaVal.xpos = float.Parse(values[2]);
            currPlaVal.ypos = float.Parse(values[3]);
            currPlaVal.zpos = float.Parse(values[4]);

            Debug.Log(currPlaVal.name + " "
                + currPlaVal.scale + " "
                + currPlaVal.xpos + " "
                + currPlaVal.ypos + " "
                + currPlaVal.zpos + " ");

            playerValues.Add(currPlaVal);
        }
    }

    public static PlayerValues getPlayerValues(string playerName)
    {
        foreach(PlayerValues playerValue in playerValues)
        {
            if(playerValue.name == playerName)
            {
                return playerValue;
            }
        }
        Debug.Log("No Player with specified name given");
        return new PlayerValues();
    }
}
