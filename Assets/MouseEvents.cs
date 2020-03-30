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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        if(allowClicking) clickingAmount++;
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
