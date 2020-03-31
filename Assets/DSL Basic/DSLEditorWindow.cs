using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DSLEditorWindow : EditorWindow
{

#if UNITY_EDITOR
    /*This DSL Editor is used to write all the dialogue
     that you are going to have. You could even open up an
     external code editor to do this as well, but it's more practical if
     done through here.*/
    public bool fileDialogOpen = false;
    public string fileToImport = "";

    static DSLEditorWindow window = null;

    public string content = "";

    public Vector2 contentScrollView;

    void Update()
    {

    }

    [MenuItem("Window/DSL Editor")]
    //Display the window
    public static void Open()
    {
       window = GetWindow<DSLEditorWindow>("DSL Editor");
        window.Show();
    }

    void OnGUI()
    {
        //Window Code
        //GUILayout is used for labels, spaces, and buttons
        GUILayout fileDirectoryInput = new GUILayout();

        GUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Import File", fileToImport);
        if (GUILayout.Button("Import"))
        {
            
        }
        
        GUILayout.EndHorizontal();

        GUILayout.BeginArea(new Rect(0, 24, window.position.width,150));
        //This is for properties and fields and what not.
        EditorGUILayout.TextField("Title", "Hello");
        EditorGUILayout.TextField("Author", "Unknown");
        EditorGUILayout.TextField("Date", "");
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(0, 100, window.position.width, window.position.height));
        contentScrollView = EditorGUILayout.BeginScrollView(contentScrollView);
        content = EditorGUILayout.TextArea(content, GUILayout.Width(window.position.width - 20), GUILayout.Height(window.position.height - 150));
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();

    }

    void OpenFileDialog()
    {

    }
}
#endif