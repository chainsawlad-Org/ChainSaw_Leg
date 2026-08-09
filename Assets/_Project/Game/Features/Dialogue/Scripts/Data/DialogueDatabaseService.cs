using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

public class DialogueDatabaseService
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
    public Dictionary<string, DialogueNode> nodesDatabase = new Dictionary<string, DialogueNode>();

    public bool isBuilt = false;

    public void Build()
    {
        /*string jsonData = DialogueImporter.Load(nameJson, pathToJson);

        if (jsonData != null)
        {
            JArray array = DialogueImporter.Parse(jsonData);

            foreach (JObject obj in array)
            {

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

            if (DialogueImporter.Validate(nodesDatabase))
            {
                Debug.Log("Dialogue database is importing");
                isBuilt = true;
            }
            else
            {
                Debug.LogError("Dialogue database is not importing");
            }
        }
        else
        {
            Debug.LogError("File " + nameJson + " is null");
        }*/
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

