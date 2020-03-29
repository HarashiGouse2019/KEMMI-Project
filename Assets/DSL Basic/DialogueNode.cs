using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueNode : MonoBehaviour, DialogueSystemEvents.IExecuteOnEnd
{
    public static DialogueNode Instance;
    /*Dialogue Node will allow us to have full control over what dialogue to run, when we run them, and
     what action we want to take after running the dialogue set.
     
     We first need to define what dialogue set to execute first with an integer.

    Then, we decide if you should run On Awake or not

    Then, we need a list of events. I'm not going too with it, but we need a UnityEvent of OnEnd
         
         */

    [SerializeField]
    private bool executeOnStart = false;

    [Header("Dialogue Set"), SerializeField]
    private int setValue = 0;

    [Header("Events"), SerializeField]
    private UnityEvent OnEnd = new UnityEvent();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(executeOnStart)
        {
            DialogueSystem.REQUEST_DIALOGUE_SET(setValue);
            DialogueSystem.Run();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeRequstValue(int _value, bool _runImmediately = false)
    {
        setValue = _value;

        switch (_runImmediately)
        {
            case true: DialogueSystem.REQUEST_DIALOGUE_SET(setValue); DialogueSystem.Run(); break;
            case false: DialogueSystem.REQUEST_DIALOGUE_SET(setValue); break;
        }

    }

    public void Run()
    {
        DialogueSystem.Run();
    }

    public void ExecuteOnEnd() {
        OnEnd.Invoke();
    }
}
