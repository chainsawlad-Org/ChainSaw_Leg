using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

public class DialogueDatabase
{
    public string nameJson = "replicas.JSON";
    public string pathToJson = "_Project/Game/Features/Dialogue/Scripts/Data";

    public List<DialogueNode> nodes = new List<DialogueNode>();
    public Dictionary<string, DialogueNode> nodesDatabase = new Dictionary<string, DialogueNode>();

    public bool isBuilt = false;

    public void Build()
    {
        string jsonData = DialogueImporter.Load(nameJson, pathToJson);

        if (jsonData != null)
        {
            JArray array = DialogueImporter.Parse(jsonData);

            foreach (JObject obj in array)
            {
                //JArray arrayobj = JArray.Parse(line);

                string id = obj["id"].Value<string>();

                if (Contains(id))
                {
                    Debug.LogError("id " + id + " already exists");
                    break;
                }

                DialogueNode node = new DialogueNode
                {
                    speaker = obj["speaker"].Value<string>(),
                    text = obj["text"].Value<string>(),
                    nextId = obj["next_id"].Value<string>(),
                };

                nodesDatabase.Add(id, node);
                nodes.Add(node);
            }
            isBuilt = true;
        }
    }

    public DialogueNode GetNode(string id)
    {
        DialogueNode node = nodesDatabase[id];

        if (node == null)
        {
            Debug.LogError("id " + id + " not found");
        }

        return node;
    }

    public DialogueNode TryGetNode(string id)
    {
        DialogueNode node = null;
        try
        {
            node = nodesDatabase[id];
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return node;
    }

    public bool Contains(string id)
    {
        return nodesDatabase.ContainsKey(id);
    }

    public List<DialogueNode> GetAll()
    {
        return nodes;
    }
}

