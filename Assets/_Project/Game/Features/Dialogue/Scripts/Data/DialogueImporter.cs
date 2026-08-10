using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueImporter
{
    public string nameJson = "replicas.JSON";
    public string pathToJson = "_Project/Game/Features/Dialogue/Scripts/Data";
    public List<DialogueNode> Import()
    {
        List<DialogueNode> nodes = new List<DialogueNode>();
        string jsonData = Load(nameJson, pathToJson);
        nodes = Parse(jsonData);


        return nodes;
    }

    public string Load(string nameJson, string pathToJson)
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

    public List<DialogueNode> Parse(string JsonData)
    {
        List<DialogueNode> nodes = new List<DialogueNode>();
        JArray array = JArray.Parse(JsonData);
        
        foreach (JObject obj in array)
        {

            DialogueNode node = new DialogueNode
            {
                id = obj["id"].Value<string>(),
                speaker = obj["speaker"].Value<string>(),
                text = obj["text"].Value<string>(),
                nextId = obj["next_id"].Value<string>(),
            };

            nodes.Add(node);
        }

        Validate(nodes);

        return nodes;
    }

    public bool Validate(List<DialogueNode> nodes)
    {
        if (nodes == null)
        {
            Debug.LogError("Dialogue node is null");
            return false;
        }

        foreach (DialogueNode node in nodes)
        {

            if (string.IsNullOrEmpty(node.id))
            {
                Debug.LogError("ID is null");
                return false;
            }

            if (string.IsNullOrEmpty(node.text))
            {
                Debug.LogError("Node " + node.id + " has null text");
                return false;
            }

            if (string.IsNullOrEmpty(node.speaker))
            {
                Debug.LogError("Node " + node.id + " has null speaker");
                return false;
            }

            if (!nodes.Any(n => n.id == node.nextId) && !string.IsNullOrEmpty(node.nextId))
            {
                Debug.LogError("Node " + node.id + " has non-existent nextId");
                return false;
            }

        }

        return true;

    }
}
