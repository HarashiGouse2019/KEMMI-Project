using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MouseEvents : MonoBehaviour
{
    //This will be if we 
    [SerializeField]
    protected bool allowClicking = false;

    [SerializeField]
    protected int clickingAmount = 0;

    [SerializeField]
    protected MouseDataReceiver receiver = null;

    [SerializeField]
    protected bool useMouseReceiver = false;

    [SerializeField]
    protected int eventID = 0;

    IEnumerator inputCycle;

    // Start is called before the first frame update
    void Start()
    {
        inputCycle = RecieverMouseInput();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ReceiveInput();
    }

    void ReceiveInput()
    {
        StartCoroutine(inputCycle);
    }

    IEnumerator RecieverMouseInput()
    {
        while (allowClicking)
        {
            if (Input.GetMouseButtonDown(0))
                clickingAmount++;

            yield return null;
        }
    }

    public void EnableClicking()
    {
        allowClicking = true;
    }

    public void DisableClicking()
    {
        allowClicking = false;
    }

    public void MakeObjectDraggable()
    {
        receiver.dragable = true;
    }

    public void StopDragging()
    {
        receiver.dragable = false;
    }

    public void ChangeEventID(int _value)
    {
        eventID = _value;
    }
}
