using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseControls : MonoBehaviour
{

    int w = 64; //The width of screen
    int h = 64; //The height of screen

    static MouseControls Instance;

    static Vector2 mouse; //This will grab our mouse's x and y coordinates

    public Texture2D[] cursorImage; //The cursor image will be referenced


    //Mouse controls

    public static string CurrentlyOn { get; set; } = "";
    public static GameObject Object { get; set; } = null;

    public bool isClicking = false;

    public SpriteRenderer renderer;

    Vector3 screenPoint;
    Vector3 offset;

    void Awake()
    {
        Instance = this;
        renderer = GetComponent<SpriteRenderer>();
    }


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false; //Our system cursor will not be visible
    }

    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        isClicking = Input.GetMouseButton(0);
        //Despite the system cursor not being visible, the position of it is still given.
        //Our Vector2 move is assign the cursors current position in game
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(mouse.x - (w / 2), mouse.y - (h / 2), w, h), cursorImage[isClicking?1:0]);
        //Draws our graphically drawn cursor (not the system cursor)
    }
    public static GameObject SetCurrentlyOn(GameObject _obj)
    {
        CurrentlyOn = _obj.name;
        Object = _obj;
        return Object;
    }
}
