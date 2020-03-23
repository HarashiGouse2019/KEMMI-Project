using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using PARSER = DSLParser.DialogueSystemParser;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    public enum TextSpeed
    {
        SLOWER,
        SLOW,
        NORMAL,
        FAST,
        FASTER
    }


    [SerializeField]
    private string dsfName;

    [SerializeField]
    private TextMeshProUGUI TMP_DIALOGUETEXT;

    [SerializeField]
    private Image textBoxUI;

    [SerializeField]
    private TextSpeed text_Speed;

    private static float textSpeed;

    public static List<string> dialogue = new List<string>();

    public static bool runningDialogue { get; private set; } = false;

    public static uint lineIndex { get; private set; } = 0;

    private static bool typeIn;

    const int reset = 0;

    void Awake()
    {
        Instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        REQUEST_DIALOGUE_SET(0);
        Run();
    }

    public static void Run()
    {
        if (dialogue.Count != 0 && InBounds((int)lineIndex, dialogue) && IS_TYPE_IN() == false)
            Instance.StartCoroutine(PrintCycle());
    }

    static IEnumerator PrintCycle()
    {
        while (true)
        {
            var text = "";
            
            if (lineIndex < dialogue.Count) text = dialogue[(int)lineIndex];

            if (IS_TYPE_IN() == false)
            {
                ENABLE_DIALOGUE_BOX();

                GET_TMPGUI().text = "";

                //Typewriter effect
                if (PARSER.LINE_HAS(text, PARSER.tokens[0]))
                {
                    for (int value = 0; value < text.Length - PARSER.tokens[0].Length + 1; value++)
                    {
                        UPDATE_TEXT_SPEED(Instance.text_Speed);
                        GET_TMPGUI().text = text.Substring(0, value);
                        yield return new WaitForSeconds(textSpeed);
                    }
                }
            }

            SET_TYPE_IN_VALUE(true);
            Instance.StartCoroutine(WaitForResponse());

            yield return null;
        }
    }

    static bool InBounds(int index, List<string> array) => (index >= 0) && (index < array.Count);

    public static void REQUEST_DIALOGUE_SET(int _dialogueSet)
    {
        string dsPath = Application.streamingAssetsPath + @"/" + GET_DIALOGUE_SCRIPTING_FILE();

        string line = null;

        int position = 0;

        bool foundDialogueSet = false;

        if (File.Exists(dsPath))
        {
            using (StreamReader fileReader = new StreamReader(dsPath))
            {
                while (true)
                {
                    line = fileReader.ReadLine();

                    if (line == null)
                    {
                        if (foundDialogueSet)
                            return;
                        else
                        {
                            Debug.Log("Dialogue Set " + _dialogueSet.ToString("D3, CultureInfo.InvariantCulture") + " does not exist. Try adding it to the .dsf referenced.");
                            return;
                        }
                    }

                    line.Split(PARSER.delimiters);

                    if (line.Contains("<DIALOGUE_SET_" + _dialogueSet.ToString("D3", CultureInfo.InvariantCulture) + ">"))
                    {
                        foundDialogueSet = true;
                        GetDialogue(position);
                    }

                    position++;
                }
            }
        }
        Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
    }

    static void GetDialogue(int _position)
    {
        string dsPath = Application.streamingAssetsPath + @"/" + GET_DIALOGUE_SCRIPTING_FILE();

        string line = null;

        bool atTargetLine = false;

        if (File.Exists(dsPath))
        {
            using (StreamReader fileReader = new StreamReader(dsPath))
            {
                int position = 0;

                while (true)
                {
                    line = fileReader.ReadLine();

                    if (line == "<END>" && atTargetLine)
                    {
                        runningDialogue = true;
                        return;
                    }

                    if (position > _position)
                    {
                        atTargetLine = true;
                        if (line != "") dialogue.Add(line);
                    }

                    position++;
                }
            }
        }
    }

    public static IEnumerator WaitForResponse()
    {
        while (IS_TYPE_IN())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (lineIndex < dialogue.Count - 1)
                {
                    Progress();

                }
                else
                {
                    runningDialogue = false;
                    lineIndex = 0;
                    SET_TYPE_IN_VALUE(false);
                    DISABLE_DIALOGUE_BOX();
                    dialogue.Clear();
                    Instance.StopAllCoroutines();
                }
            }

            yield return null;
        }
    }

    public static void Progress()
    {
        if (lineIndex < dialogue.Count - 1 && IS_TYPE_IN() == true)
        {
            lineIndex += 1;

            GET_TMPGUI().text = "";
            SET_TYPE_IN_VALUE(false);
        }
    }

    public static void UPDATE_TEXT_SPEED(TextSpeed _textSpeed)
    {
        switch (_textSpeed)
        {
            case TextSpeed.SLOWER: textSpeed = 1f; return;
            case TextSpeed.SLOW: textSpeed = 0.5f; return;
            case TextSpeed.NORMAL: textSpeed = 0.050f; return;
            case TextSpeed.FAST: textSpeed = 0.025f; return;
            case TextSpeed.FASTER: textSpeed = 0.005f; return;
            default: return;
        }
    }

    public static string GET_DIALOGUE_SCRIPTING_FILE() => Instance.dsfName + ".dsf";

    public static bool IS_TYPE_IN() => typeIn;

    public static void SET_TYPE_IN_VALUE(bool value) {typeIn = value; }

    public static TextMeshProUGUI GET_TMPGUI() => Instance.TMP_DIALOGUETEXT;

    public static void ENABLE_DIALOGUE_BOX() => Instance.textBoxUI.gameObject.SetActive(true);

    public static void DISABLE_DIALOGUE_BOX() => Instance.textBoxUI.gameObject.SetActive(false);
}