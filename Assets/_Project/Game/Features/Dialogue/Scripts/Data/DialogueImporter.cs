using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class DialogueImporter
{
    public string nameJson = "replicas.JSON";
    public string pathToJson = "_Project/Game/Features/Dialogue/Scripts/Data/";
    public string Load()
    {
        string pathToFile = Path.Combine(Application.dataPath, pathToJson, nameJson);
        string jsonData = null;
        
        if (File.Exists(pathToFile))
        {
            jsonData = File.ReadAllText(pathToFile);
        }
        else
        {
            Debug.LogError(nameJson + "does not exist");
        }

        return jsonData;
    }

    public void Parse()
    {

    }

    public void Validate()
    {

    }

}
