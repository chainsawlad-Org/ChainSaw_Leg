using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDatabaseService : ApplicationServiceBase
{
    private readonly List<DialogueNode> nodes = new();
    private readonly Dictionary<string, DialogueNode> nodesDatabase = new();
    private readonly DialogueImporter importer;

    public IReadOnlyList<DialogueNode> Nodes => nodes;
    public IReadOnlyDictionary<string, DialogueNode> NodesDatabase => nodesDatabase;
    public bool IsBuilt { get; private set; }

    public DialogueDatabaseService(DialogueImporter importer)
    {
        this.importer = importer;
    }
    public override UniTask Initialize()
    {
        Build(importer.Import());

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
            nodes.Add(node);
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

    public IReadOnlyList<DialogueNode> GetAll()
    {
        return nodes;
    }
}

