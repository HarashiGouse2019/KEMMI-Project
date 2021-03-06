﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteDialogueNode : MonoBehaviour
{
    public int nodeNumber = 0;

    bool executeOnStart = false;

    void Start()
    {
        if (executeOnStart)
            ExecuteNode();
    }

    public void ExecuteNode()
    {
        DialogueNode node = DialogueSystem.GET_NODES()[nodeNumber];
        node.ChangeRequestValue(node.GetRunValue(), true);

    }
}
