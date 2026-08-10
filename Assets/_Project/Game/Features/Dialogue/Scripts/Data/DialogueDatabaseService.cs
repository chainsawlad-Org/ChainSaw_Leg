using UnityEngine;
using System.Collections.Generic;
using System;

public class DialogueDatabaseService : ApplicationServiceBase
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
    public Dictionary<string, DialogueNode> nodesDatabase = new Dictionary<string, DialogueNode>();

    //override Initialize();

    public bool isBuilt = false;

    public void Build()
    {

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

