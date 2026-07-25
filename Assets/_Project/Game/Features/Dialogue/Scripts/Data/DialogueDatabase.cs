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

                DialogueNode node = new DialogueNode
                {
                    speaker = obj["speaker"].Value<string>(),
                    text = obj["text"].Value<string>(),
                    nextId = obj["next_id"].Value<string>(),
                };

                nodesDatabase.Add(id, node);
                nodes.Add(node);
            }
        }
    }

    public void GetNode(int id)
    {

    }

    public void TryGetNode(int id)
    {

    }

    public void Contains(int id)
    {

    }

    public void GetAll()
    {

    }
}

