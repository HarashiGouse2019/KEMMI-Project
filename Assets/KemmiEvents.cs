using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KemmiEvents : MouseEvents
{
    // Update is called once per frame
    void Update()
    {
        if (allowClicking == true)
            Debug.Log("YES!!!");

        switch (eventID)
        {
            case 0:
                if (clickingAmount == 1)
                {
                    allowClicking = false;
                    DialogueSystem.REQUEST_DIALOGUE_SET(1);
                    DialogueSystem.Run(2);
                    ChangeEventID(1);
                }
                break;

            case 1:
                if (clickingAmount == 2)
                {
                    allowClicking = false;
                    DialogueSystem.REQUEST_DIALOGUE_SET(2);
                    DialogueSystem.Run(3);
                    ChangeEventID(2);
                }
                break;

            case 2:
                if (clickingAmount == 3)
                {
                    allowClicking = false;
                    DialogueSystem.REQUEST_DIALOGUE_SET(3);
                    DialogueSystem.Run(4);
                    ChangeEventID(3);
                }
                break;

        }
    }
}


