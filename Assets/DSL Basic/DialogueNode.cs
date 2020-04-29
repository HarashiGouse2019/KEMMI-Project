using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Node", menuName = "Dialogue Node")]
public class DialogueNode : ScriptableObject, DialogueSystemEvents.IExecuteOnEnd
{
    public static DialogueNode Instance;
    /*Dialogue Node will allow us to have full control over what dialogue to run, when we run them, and
     what action we want to take after running the dialogue set.
     
     We first need to define what dialogue set to execute first with an integer.

    Then, we decide if you should run On Awake or not

    Then, we need a list of events. I'm not going too with it, but we need a UnityEvent of OnEnd
         
         */


    //Check if you want to execute dialogue on start
    [SerializeField]
    private bool executeOnStart = false;

    //What dialogue set to read from
    [Header("Dialogue Set"), SerializeField]
    private int setValue = 0;

    //Events to be called at the end of the dialogue set
    [Header("Events"), SerializeField]
    private UnityEvent OnEnd = new UnityEvent();

    public void Start()
    {
        //Dialogue will run on start if executeOnStart is toggled on
        if(executeOnStart)
        {
            //Grab Dialogue Set number from the specified dsl file
            DialogueSystem.REQUEST_DIALOGUE_SET(setValue);

            //Run the Dialogue Set
            DialogueSystem.Run();
        }
    }

    /// <summary>
    /// Change the Request value for this node.
    /// </summary>
    /// <param name="_value"></param>
    /// <param name="_runImmediately"></param>
    public void ChangeRequestValue(int _value, bool _runImmediately = false)
    {
        setValue = _value;
        switch (_runImmediately)
        {
            case true: DialogueSystem.REQUEST_DIALOGUE_SET(setValue); DialogueSystem.Run(setValue); break;
            case false: DialogueSystem.REQUEST_DIALOGUE_SET(setValue); break;
        }

    }

    /// <summary>
    /// Run the Dialogue Set from this node.
    /// </summary>
    public void Run()
    {
        Debug.Log("Okay...");
        DialogueSystem.Run(setValue);
    }

    /// <summary>
    /// Execute the listeners in the Unity Events
    /// </summary>
    public void ExecuteOnEnd() {
        OnEnd.Invoke();
    }

    /// <summary>
    /// Return the Dialogue Set value
    /// </summary>
    /// <returns></returns>
    public int GetRunValue() => setValue;
}
