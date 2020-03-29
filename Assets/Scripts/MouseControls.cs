using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControls : MonoBehaviour
{
    static MouseControls Instance;

    //Mouse controls

    public static string CurrentlyOn { get; set; } = "";
    public static GameObject Object { get; set; } = null;


    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Object.transform.position = Input.mousePosition;
    }

    public static GameObject SetCurrentlyOn(GameObject _obj)
    {
        CurrentlyOn = _obj.name;
        Object = _obj;
        return Object;
    }
}
