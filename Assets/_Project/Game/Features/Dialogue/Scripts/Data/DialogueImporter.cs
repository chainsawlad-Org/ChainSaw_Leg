using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public static bool Validate(Dictionary<string, DialogueNode> nodesDatabase)
    {
        if (nodesDatabase == null)
        {
            Debug.LogError("Dialogue database is null");
            return false;
        }

        foreach (KeyValuePair<string, DialogueNode> dict in nodesDatabase)
        {
            if (string.IsNullOrEmpty(dict.Key))
            {
                Debug.LogError("Key is null");
                return false;
            }

            if (string.IsNullOrEmpty(dict.Value.text))
            {
                Debug.LogError("Node " + dict.Key + " has null text");
                return false;
            }

            if (string.IsNullOrEmpty(dict.Value.speaker))
            {
                Debug.LogError("Node " + dict.Key + " has null speaker");
                return false;
            }

            if (!nodesDatabase.ContainsKey(dict.Value.nextId) && dict.Value.nextId != " ")
            {
                Debug.LogError("Node " + dict.Key + " has non-existent nextId");
                return false;
            }

        }

        return true;

    }
}
