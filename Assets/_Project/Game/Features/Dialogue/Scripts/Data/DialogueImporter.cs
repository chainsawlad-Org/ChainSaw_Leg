using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

public static class DialogueImporter
{
    public static string Load(string nameJson, string pathToJson)
    {
        string pathToFile = Path.Combine(Application.dataPath, pathToJson, nameJson);
        string jsonData = null;
        
        if (File.Exists(pathToFile))
        {
            jsonData = File.ReadAllText(pathToFile);
        }
        else
        {
            Debug.LogError(nameJson + " does not exist");
        }

        return jsonData;
    }

    public static JArray Parse(string JsonData)
    {
        JArray array = JArray.Parse(JsonData);
        return array;
    }

    public static void Validate()
    {

    }

}
