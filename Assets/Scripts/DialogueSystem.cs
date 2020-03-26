using System;
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

    public enum TextSpeedValue
    {
        SLOWEST,
        SLOWER,
        SLOW,
        NORMAL,
        FAST,
        FASTER,
        FASTEST
    }


    [SerializeField]
    private string dsfName;

    [SerializeField]
    private TextMeshProUGUI TMP_DIALOGUETEXT;

    [SerializeField]
    private Image textBoxUI;

    private static TextSpeedValue SpeedValue;

    private static float TextSpeed;

    public static List<string> Dialogue = new List<string>();

    public static bool RunningDialogue { get; private set; } = false;

    public static uint LineIndex { get; private set; } = 0;

    public static uint CursorPosition { get; private set; } = 0;

    private static bool typeIn;

    public static int DialogueSet { get; private set; } = -1;

    public static List<DialogueSystemSpriteChanger> DialogueSystemSpriteChangers { get; private set; } = new List<DialogueSystemSpriteChanger>();

    const int reset = 0;
    const string BOLD = "<b>", BOLDEND = "</b>";
    const string ITALIZE = "<i>", ITALIZEEND = "</i>";
    const string UNDERLINE = "<u>", UNDERLINEEND = "</u>";
    const string SKIP = "<skip>";
    const string EXPRESSION = "<exp>";
    const string SPEED = "sp=";
    const string dslFileExtention = ".dsf";
    const string STRINGNULL = "";
    const bool SUCCESSFUL = true;
    const bool FAILURE = false;

    void Awake()
    {
        Instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        PARSER.Define_Expressions();

        REQUEST_DIALOGUE_SET(0);

        DialogueSystemSpriteChangers = FIND_ALL_SPRITECHANGERS();

        Run();
    }

    public static void Run()
    {
        if (Dialogue.Count != 0 && InBounds((int)LineIndex, Dialogue) && IS_TYPE_IN() == false)
        {
            //We'll parse the very first dialogue that is ready to be displayed
            Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

            Instance.StartCoroutine(ExclusionCycle());

            Instance.StartCoroutine(PrintCycle());

        }
    }

    static IEnumerator ExclusionCycle()
    {
        while (true)
        {
            ExcludeAllTags(Dialogue[(int)LineIndex]);

            yield return new WaitForFixedUpdate();
        }
    }

    static IEnumerator PrintCycle()
    {
        while (true)
        {
            if (IS_TYPE_IN() == false)
            {


                ENABLE_DIALOGUE_BOX();

                GET_TMPGUI().text = STRINGNULL;

                var text = STRINGNULL;

                if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];

                //Typewriter effect
                if (PARSER.LINE_HAS(text, PARSER.tokens[0]))
                {
                    for (CursorPosition = 0; CursorPosition < text.Length - PARSER.tokens[0].Length + 1; CursorPosition++)
                    {

                        try
                        {
                            if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];

                            UPDATE_TEXT_SPEED(SpeedValue);

                            GET_TMPGUI().text = text.Substring(0, (int)CursorPosition);

                        }
                        catch { }

                        yield return new WaitForSeconds(TextSpeed);

                    }
                }

                SET_TYPE_IN_VALUE(true);
                Instance.StartCoroutine(WaitForResponse());

            }

            yield return new WaitForEndOfFrame();
        }

    }

    static void ExcludeAllTags(string _text)
    {
        //Bold tag
        ExcludeStyleTag(BOLD, BOLDEND, ref _text);

        //Italize tag
        ExcludeStyleTag(ITALIZE, ITALIZEEND, ref _text);

        //Underline tag
        ExcludeStyleTag(UNDERLINE, UNDERLINEEND, ref _text);

        //Speed Command Tag: It will consider all of the possible values.
        for (int value = 0; value < Enum.GetValues(typeof(TextSpeedValue)).Length; value++)
            ExecuteSpeedFunctionTag(PARSER.delimiters[0] + SPEED + value + PARSER.delimiters[1], ref _text);

        ExecuteActionFunctionTag(SKIP, ref _text);
        ExecuteExpressionFunctionTag(EXPRESSION, ref _text);
    }

    static bool ExcludeStyleTag(string _openTag, string _closeTag, ref string _line)
    {
        try
        {
            if (_line.Substring((int)CursorPosition, _openTag.Length).Contains(_openTag))
            {
                ShiftCursorPosition(_openTag.Length);
                return SUCCESSFUL;
            }

            else if (_line.Substring((int)CursorPosition, _closeTag.Length).Contains(_closeTag))
            {
                ShiftCursorPosition(_closeTag.Length);
                return SUCCESSFUL;
            }
            else
                return FAILURE;
        }
        catch { }
        return FAILURE;
    }

    static void ExecuteSpeedFunctionTag(string _tag, ref string _line)
    {
        try
        {
            if (_line.Substring((int)CursorPosition, _tag.Length + sizeof(int)).Contains(_tag))
            {
                _line = _line.Replace(_tag, "");

                Dialogue[(int)LineIndex] = _line;

                int speed = Convert.ToInt32(_tag.Split('<')[1].Split('=')[1].Split('>')[0]);

                SpeedValue = (TextSpeedValue)speed;
            }
        }
        catch { }
    }

    static void ExecuteActionFunctionTag(string _tag, ref string _line)
    {
        try
        {

            if (_line.Substring((int)CursorPosition, _tag.Length).Contains(_tag))
            {

                _line = _line.Replace(_tag, "");

                ShiftCursorPosition(_tag.Length - 1);

                Dialogue[(int)LineIndex] = _line;
            }
        }
        catch { }
    }

    static void ExecuteExpressionFunctionTag(string _tag, ref string _line)
    {
        bool notFlaged = true;
        try
        {
            if (_line.Substring((int)CursorPosition, _tag.Length + 2).Contains(_tag))
            {
                /*The system will now take this information, from 0 to the current position
                 and split it down even further, taking all the information.*/

                _line = _line.Remove((int)CursorPosition, _tag.Length + 2);

                Dialogue[(int)LineIndex] = _line;

                string value = "";

                if (_line.Substring((int)CursorPosition, "EXPRESSION".Length + 2).Contains("EXPRESSION"))
                {
                    value = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);

                    if (value.Contains("]"))
                        value = value.Split(':')[2].Split(']')[0];

                }
                //Check if a key matches
                string data = STRINGNULL;

                if (PARSER.DefinedExpressions.ContainsKey(value))
                {
                    if (value.GetType() == typeof(string))
                    {
                        data = FindKey(value, PARSER.DefinedExpressions);

                        _line = _line.Replace("EXPRESSION::" + value + "]", "");

                        Dialogue[(int)LineIndex] = _line;

                        notFlaged = false;
                    }
                }

                else if (PARSER.DefinedExpressions.ContainsValue(Convert.ToInt32(value)))
                {
                    if (Convert.ToInt32(value).GetType() == typeof(int))
                    {

                        data = FindValueAndConvertToKey(Convert.ToInt32(value), PARSER.DefinedExpressions);

                        _line = _line.Replace("EXPRESSION::" + value + "]", "");

                        Dialogue[(int)LineIndex] = _line;

                        notFlaged = false;
                    }
                }

                if (notFlaged)
                {
                    //Otherwise, we'll through an error saying this hasn't been defined.
                    Debug.LogError(_line + " has not been defined. Perhaps declaring it in the associated .dsf File.");
                    return;
                }

                Debug.Log(data);

                //We get the name, keep if it's EXPRESSION or POSE, and the emotion value
                string characterName = data.Split('_')[0];
                string changeType = data.Split('_')[1];
                string characterState = data.Split('_')[2];

                //Now we see if we can grab the image, and have it change...
                DialogueSystemSpriteChanger changer = Find_Sprite_Changer(characterName + "_" + changeType);

                changer.CHANGE_IMAGE(characterState);
            }
        }
        catch { }
    }

    static string FindKey(string _key, Dictionary<string, int> _dictionary)
    {
        while (true)
        {
            List<string> keys = new List<string>(_dictionary.Keys);

            foreach (string key in keys)
            {
                if (key == _key)
                    return key;
            }

            return STRINGNULL;
        }
    }

    static string FindValueAndConvertToKey(int _value, Dictionary<string, int> _dictionary)
    {
        while (true)
        {
            List<string> keys = new List<string>(_dictionary.Keys);

            int index = 1;

            foreach(string key in keys)
            {
                if (_value == index)
                    return keys[index - 1];

                index++;
            }

            return STRINGNULL;
        }
    }

    static bool InBounds(int index, List<string> array) => (index >= reset) && (index < array.Count);

    static DialogueSystemSpriteChanger Find_Sprite_Changer(string _name)
    {
        foreach (DialogueSystemSpriteChanger instance in DialogueSystemSpriteChangers)
        {
            if (_name == instance.Get_Prefix())
                return instance;
        }

        Debug.LogError("The SpriteChange by the name of " + _name + " doesn't exist.");
        return null;
    }

    static List<DialogueSystemSpriteChanger> FIND_ALL_SPRITECHANGERS()
    {
        DialogueSystemSpriteChanger[] instances = FindObjectsOfType<DialogueSystemSpriteChanger>();

        List<DialogueSystemSpriteChanger> list = new List<DialogueSystemSpriteChanger>();

        foreach (DialogueSystemSpriteChanger instance in instances)
        {
            list.Add(instance);
        }

        return list;
    }

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

                        DialogueSet = _dialogueSet;

                        PARSER.GetDialogue(position);
                    }

                    position++;
                }
            }
        }
        Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
    }

    public static IEnumerator WaitForResponse()
    {
        while (IS_TYPE_IN())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (LineIndex < Dialogue.Count - 1)
                {
                    Progress();
                    CursorPosition = reset;
                }
                else
                {
                    RunningDialogue = false;
                    LineIndex = 0;
                    SET_TYPE_IN_VALUE(false);
                    DISABLE_DIALOGUE_BOX();
                    Dialogue.Clear();
                    Instance.StopAllCoroutines();
                    CursorPosition = reset;
                }
            }

            yield return null;
        }
    }

    public static void Progress()
    {
        if (LineIndex < Dialogue.Count - 1 && IS_TYPE_IN() == true)
        {
            LineIndex += 1;

            GET_TMPGUI().text = STRINGNULL;
            SET_TYPE_IN_VALUE(false);

            //We'll parse the next line.
            Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);
        }
    }

    public static uint ShiftCursorPosition(int _newPosition)
    {
        try
        {
            CursorPosition += (uint)_newPosition;
        }
        catch { }
        return CursorPosition;
    }

    public static uint ShiftCursorPosition(int _newPosition, string _tag, string _removeFrom)
    {
        try
        {
            CursorPosition += (uint)_newPosition;
            _removeFrom = _removeFrom.Replace(_tag, "");
        }
        catch { }
        return CursorPosition;
    }

    public static void UPDATE_TEXT_SPEED(TextSpeedValue _textSpeed)
    {
        switch (_textSpeed)
        {
            case TextSpeedValue.SLOWEST: TextSpeed = 0.25f; return;
            case TextSpeedValue.SLOWER: TextSpeed = 0.1f; return;
            case TextSpeedValue.SLOW: TextSpeed = 0.05f; return;
            case TextSpeedValue.NORMAL: TextSpeed = 0.025f; return;
            case TextSpeedValue.FAST: TextSpeed = 0.01f; return;
            case TextSpeedValue.FASTER: TextSpeed = 0.005f; return;
            case TextSpeedValue.FASTEST: TextSpeed = 0.0025f; return;
            default: return;
        }
    }

    public static string GET_DIALOGUE_SCRIPTING_FILE() => Instance.dsfName + dslFileExtention;

    public static bool IS_TYPE_IN() => typeIn;

    public static void SET_TYPE_IN_VALUE(bool value) { typeIn = value; }

    public static TextMeshProUGUI GET_TMPGUI() => Instance.TMP_DIALOGUETEXT;

    public static void ENABLE_DIALOGUE_BOX() => Instance.textBoxUI.gameObject.SetActive(true);

    public static void DISABLE_DIALOGUE_BOX() => Instance.textBoxUI.gameObject.SetActive(false);

    public void OnDisable()
    {
        StopAllCoroutines();
    }
}