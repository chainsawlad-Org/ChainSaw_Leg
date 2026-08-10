using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;

public class DialogueDatabaseService : ApplicationServiceBase
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
    public Dictionary<string, DialogueNode> nodesDatabase = new Dictionary<string, DialogueNode>();
    public bool IsBuilt { get; private set; }

    private readonly DialogueImporter importer;
    public DialogueDatabaseService(DialogueImporter importer)
    {
        this.importer = importer;
    }
    public override UniTask Initialize()
    {
        nodes = importer.Import();
        if (nodes != null)
        {
            Build(nodes);
            IsBuilt = true;
            Debug.Log("Database ready");
        }
        else
        {
            IsBuilt = false;
            Debug.Log("Database is null");
        }

        return UniTask.CompletedTask;
    }

    public void Build(List<DialogueNode> nodes)
    {
        foreach (DialogueNode node in nodes)
        {
            nodesDatabase.Add(node.id, node);
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
        nodesDatabase.TryGetValue(id, out DialogueNode node);
        
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

