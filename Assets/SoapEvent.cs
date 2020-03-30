using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoapEvent : MouseEvents
{
    // Update is called once per frame
    void Update()
    {
        switch (eventID)
        {
            case 0:
                DialogueSystem.REQUEST_DIALOGUE_SET(4);
                DialogueSystem.Run(5);
                gameObject.SetActive(false);
                break;
        }
    }

    public void SpawnInObj()
    {

    }
}
