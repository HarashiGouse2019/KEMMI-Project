﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
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

    public static float TextSpeed { get; private set; }

    public static List<string> Dialogue = new List<string>();

    public static bool RunningDialogue { get; private set; } = false;

    public static uint LineIndex { get; private set; } = 0;

    public static uint CursorPosition { get; private set; } = 0;

    public static string ActionString { get; private set; }

    private static bool OnDelay = false;

    private static bool typeIn;


    public static int DialogueSet { get; private set; } = -1;

    public static List<DialogueSystemSpriteChanger> DialogueSystemSpriteChangers { get; private set; } = new List<DialogueSystemSpriteChanger>();

    const int reset = 0;
    const string BOLD = "<b>", BOLDEND = "</b>";
    const string ITALIZE = "<i>", ITALIZEEND = "</i>";
    const string UNDERLINE = "<u>", UNDERLINEEND = "</u>";
    const string SKIP = "<skip>";
    const string EXPRESSION = "<exp>";
    const string HALT = "<halt>";
    const string SPEED = "sp=";
    const string dslFileExtention = ".dsf";
    const string STRINGNULL = "";
    const bool SUCCESSFUL = true;
    const bool FAILURE = false;

    void Awake()
    {
        Instance = this;
        PARSER.Define_Expressions();
        PARSER.Define_Poses();
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueSystemSpriteChangers = FIND_ALL_SPRITECHANGERS();
    }

    void Update()
    {
        
    }

    public static void Run()
    {

        if (Dialogue.Count != 0 && InBounds((int)LineIndex, Dialogue) && IS_TYPE_IN() == false)
        {
            if (Dialogue[(int)LineIndex].Contains("@ "))
            {
                Dialogue[(int)LineIndex] = Dialogue[(int)LineIndex].Replace("@ ", "");

                //We'll parse the very first dialogue that is ready to be displayed
                Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

                Instance.StartCoroutine(PrintCycle());

                Instance.StartCoroutine(ExclusionCycle());
            }
        }
    }

    static IEnumerator ExclusionCycle()
    {
        while (true)
        {
            if (!OnDelay)
                ExcludeAllTags(Dialogue[(int)LineIndex]);

            yield return null;
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

                    for (CursorPosition = 0; CursorPosition < text.Length - PARSER.tokens[0].Length + 1; CursorPosition += (uint)((OnDelay) ? 0 : 1))
                    {
                        try
                        {

                            if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];

                            GET_TMPGUI().text = text.Substring(0, (int)CursorPosition);

                            UPDATE_TEXT_SPEED(SpeedValue);
                        }
                        catch { }


                        yield return new WaitForSeconds(TextSpeed);



                        continue;
                    }
                }

                SET_TYPE_IN_VALUE(true);
                Instance.StartCoroutine(WaitForResponse());

            }

            yield return null;
        }

    }

    static void ExcludeAllTags(string _text)
    {
        //Action tag!
        ExecuteActionFunctionTag(SKIP, ref _text);


        //Bold tag
        ExcludeStyleTag(BOLD, BOLDEND, ref _text);

        //Italize tag
        ExcludeStyleTag(ITALIZE, ITALIZEEND, ref _text);

        //Underline tag
        ExcludeStyleTag(UNDERLINE, UNDERLINEEND, ref _text);

        //Speed Command Tag: It will consider all of the possible values.
        for (int value = 0; value < Enum.GetValues(typeof(TextSpeedValue)).Length; value++)
            ExecuteSpeedFunctionTag(PARSER.delimiters[0] + SPEED + value + PARSER.delimiters[1], ref _text);

        //Expression tag!
        ExecuteExpressionFunctionTag(EXPRESSION, ref _text);

        //Halt tage
        ExecuteWaitFunctionTag(HALT, ref _text);
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

    static bool ExecuteSpeedFunctionTag(string _tag, ref string _line)
    {
        try
        {
            if (_line.Substring((int)CursorPosition, _tag.Length + sizeof(int)).Contains(_tag))
            {
                _line = _line.Replace(" " + _tag + " ", "");

                Dialogue[(int)LineIndex] = _line;

                int speed = Convert.ToInt32(_tag.Split('<')[1].Split('=')[1].Split('>')[0]);

                SpeedValue = (TextSpeedValue)speed;

                return SUCCESSFUL;
            }
        }
        catch { }

        return FAILURE;
    }

    static bool ExecuteActionFunctionTag(string _tag, ref string _line)
    {
        try
        {
            if (_line.Substring((int)CursorPosition, _tag.Length + 2).Contains(_tag))
            {


                if (OnDelay == false)
                {
                    ShiftCursorPosition(ActionString.Length - 1);

                    _line = _line.Remove((int)CursorPosition - ActionString.Length + 1, _tag.Length - 1);

                    Dialogue[(int)LineIndex] = _line;

                    ActionString = STRINGNULL;

                    return SUCCESSFUL;
                }

            }
        }
        catch { }

        return FAILURE;
    }

    static bool ExecuteWaitFunctionTag(string _tag, ref string _line)
    {
        /*The wait command will take a 4 digit number.
         We will then convert this into a value that is easily understood
         by textSpeed. We'll be mainly affecting the textSpeed to create our
         WAIT function... unless...*/

        try
        {

            //Debug.Log(_line.Substring((int)CursorPosition, _tag.Length));
            if (_line.Substring((int)CursorPosition, _tag.Length) == _tag)
            {
                _line = _line.Remove((int)CursorPosition, _tag.Length);

                Dialogue[(int)LineIndex] = _line;

                /*Now we do a substring from the current position to 4 digits.*/

                string value = _line.Substring((int)CursorPosition, 5);

                Regex ex = new Regex(@"[*\d]");

                Match match = ex.Match(value);

                if (match.Success)
                {
                    string newValue = Regex.Replace(value, @"[^\d]", "");

                    //We got to make sure that our number is actually a legit number
                    if (Convert.ToInt32(newValue).GetType() == typeof(int))
                    {
                        int millsecs = Convert.ToInt32(newValue);

                        Instance.StartCoroutine(DelaySpan(millsecs));

                        _line = _line.Remove((int)CursorPosition, newValue.Length + 2);

                        Dialogue[(int)LineIndex] = _line;

                        return SUCCESSFUL;
                    }
                }
            }
        }
        catch { }

        return FAILURE;
    }

    static bool ExecuteExpressionFunctionTag(string _tag, ref string _line)
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
                    return FAILURE;
                }

                //We get the name, keep if it's EXPRESSION or POSE, and the emotion value
                string characterName = data.Split('_')[0];
                string changeType = data.Split('_')[1];
                string characterState = data.Split('_')[2];

                //Now we see if we can grab the image, and have it change...
                DialogueSystemSpriteChanger changer = Find_Sprite_Changer(characterName + "_" + changeType);

                changer.CHANGE_IMAGE(characterState);

                return SUCCESSFUL;
            }
        }
        catch { }
        return FAILURE;
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

            foreach (string key in keys)
            {
                if (_value == index)
                    return keys[index - 1];

                index++;
            }

            return STRINGNULL;
        }
    }

    static bool InBounds(int index, List<string> array) => (index >= reset) && (index < array.Count);

    static IEnumerator DelaySpan(float _millseconds)
    {
        OnDelay = true;

        while (OnDelay)
        {
            yield return new WaitForSeconds(_millseconds / 1000f);
            OnDelay = false;

            if (ActionString != STRINGNULL)
            {
                Debug.Log("Okay boomer...");

                ShiftCursorPosition(ActionString.Length - 1);

                Dialogue[(int)LineIndex] = Dialogue[(int)LineIndex].Replace(SKIP, "");

                ActionString = STRINGNULL;
            }
        }
    }

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
                    End();
                }
            }

            yield return null;
        }
    }

    public static void End()
    {
        RunningDialogue = false;
        LineIndex = 0;
        SET_TYPE_IN_VALUE(false);
        DISABLE_DIALOGUE_BOX();
        Dialogue.Clear();
        Instance.StopAllCoroutines();
        CursorPosition = reset;
    }

    public static void Progress()
    {
        if (LineIndex < Dialogue.Count - 1 && IS_TYPE_IN() == true)
        {
            LineIndex += 1;

            GET_TMPGUI().text = STRINGNULL;
            SET_TYPE_IN_VALUE(false);

            if (Dialogue[(int)LineIndex].Contains("@ "))
            {

                Dialogue[(int)LineIndex] = Dialogue[(int)LineIndex].Replace("@ ", "");

                //We'll parse the next dialogue that is ready to be displayed
                Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);
            }
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
        }
    }

    public static void UPDATE_ACTION_STRING(string _value)
    {
        ActionString = _value;
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