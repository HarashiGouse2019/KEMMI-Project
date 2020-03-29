﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteDialogueNode : MonoBehaviour
{
    public int nodeNumber = 0;

    public void ExecuteNode()
    {
        DialogueNode node = DialogueSystem.GET_NODES()[nodeNumber];
        Debug.Log(node.name);
        node.ChangeRequstValue(node.GetRunValue(), true);

    }
}
