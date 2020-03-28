using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using PARSER = DSLParser.DialogueSystemParser;

public class DialogueSystemEvents
{
    //Interface for events
    public interface IExecuteOnEnd
    {
        void ExecuteOnEnd();
    }

    public interface IExecuteOnCommand
    {
        void ExecuteOnCommand();
    }

    public interface IExecuteOnStart
    {
        void ExecuteOnStart();
    }
}

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

    public static List<string> ActionString { get; private set; } = new List<string>();
    public List<string> actionString;
    public static uint ActionIndex { get; private set; } = 0;
    public uint actionIndex;

    public static List<string> ExpressionValue { get; private set; } = new List<string>();
    public static uint ExpressionIndex { get; private set; } = 0;
    public List<string> expressionValue;
    public uint expressionIndex;

    private static bool OnDelay = false;

    private static bool typeIn;

    public static int DialogueSet { get; private set; } = -1;

    public static List<DialogueSystemSpriteChanger> DialogueSystemSpriteChangers { get; private set; } = new List<DialogueSystemSpriteChanger>();

    const int reset = 0;

    static readonly string BOLD = "<b>", BOLDEND = "</b>";
    static readonly string ITALIZE = "<i>", ITALIZEEND = "</i>";
    static readonly string UNDERLINE = "<u>", UNDERLINEEND = "</u>";



    static readonly Regex ACTION = new Regex(@"(<)+\w*action=\w*[a-zA-Z]+(>$)");


    static readonly Regex EXPRESSION = new Regex(@"(<)+\w*exp=\w*[A-Z0-9_-]+(>$)");

    static readonly Regex HALT = new Regex(@"(<)+\w*halt=\w*[0-9]+(>$)");

    static readonly Regex SPEED = new Regex(@"(<)+\w*sp=\w*[0-6](>$)");

    static readonly string dslFileExtention = ".dsf";
    static readonly string STRINGNULL = "";

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

        string testTag = "<action=\"CLAP\">";
        Match testMatch = ACTION.Match(testTag.Substring(0, testTag.Length));
        if (testMatch.Success)
        {
            string value = testTag.Trim('"');

            Debug.Log("If you got this message, that means that you were successful!!! " + value);

        }
    }

<<<<<<< HEAD
    void Update()
    {
        actionString = ActionString;
        actionIndex = ActionIndex;

        expressionValue = ExpressionValue;
        expressionIndex = ExpressionIndex;

        if (!OnDelay)
            ExcludeAllTags(Dialogue[(int)LineIndex]);
    }

=======
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!
    public static void Run()
    {
        if (Dialogue.Count != 0 && InBounds((int)LineIndex, Dialogue) && IS_TYPE_IN() == false)
        {
            //We'll parse the very first dialogue that is ready to be displayed
            Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

<<<<<<< HEAD
                Instance.StartCoroutine(PrintCycle());
            }
        }
    }

=======
            Instance.StartCoroutine(PrintCycle());

        }
    }
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!
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
                    for (CursorPosition = 1; CursorPosition < text.Length - PARSER.tokens[0].Length + 1; CursorPosition += (uint)((OnDelay) ? 0 : 1))
                    {
<<<<<<< HEAD
                        yield return new WaitForSeconds(TextSpeed);

                        try
                        {
=======
                        
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                        if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];

                        GET_TMPGUI().text = text.Substring(0, (int)CursorPosition);

                        UPDATE_TEXT_SPEED(SpeedValue);


<<<<<<< HEAD

=======
                        yield return new WaitForSeconds(TextSpeed);

                        ExcludeAllTags(Dialogue[(int)LineIndex]);

                        continue;
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!
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
<<<<<<< HEAD

        PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

        //Action tag!
        ExecuteActionFunctionTag(ACTION, ref _text);

=======
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!
        //Bold tag
        ExcludeStyleTag(BOLD, BOLDEND, ref _text);

        //Italize tag
        ExcludeStyleTag(ITALIZE, ITALIZEEND, ref _text);

        //Underline tag
        ExcludeStyleTag(UNDERLINE, UNDERLINEEND, ref _text);

        ////Speed Command Tag: It will consider all of the possible values.
        //ExecuteSpeedFunctionTag(SPEED, ref _text);

<<<<<<< HEAD
        ////Expression tag!
        //ExecuteExpressionFunctionTag(EXPRESSION, ref _text);
=======
        //Action tag!
        ExecuteActionFunctionTag(SKIP, ref _text);

        //Expression tag!
        ExecuteExpressionFunctionTag(EXPRESSION, ref _text);
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

        ////Halt tage
        //ExecuteWaitFunctionTag(HALT, ref _text);
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

    static bool ExecuteSpeedFunctionTag(Regex _tagExpression, ref string _line)
    {
        int startBracketPos = 0;
        int endBracketPos = 0;
        if (_line.Substring((int)CursorPosition, "<sp=".Length).Contains("<sp="))
        {
            Debug.Log("HI!!!");
            for (int index = (int)CursorPosition; index < _line.Length; index++)
            {
<<<<<<< HEAD
                if (_line[index] == '<')
                    startBracketPos = index;
=======
                _line = _line.Replace(_tag, "");
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                if (_line[index] == '>')
                {
                    endBracketPos = index;


                    try
                    {
                        string tag = _line.Substring(startBracketPos, (endBracketPos - startBracketPos) + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            _line = _tagExpression.Replace(tag, "");

                            Dialogue[(int)LineIndex] = _line;

                            int speed = Convert.ToInt32(tag.Split('<')[1].Split('=')[1].Split('>')[0]);

                            SpeedValue = (TextSpeedValue)speed;

                            return SUCCESSFUL;
                        }
                    }
                    catch { }
                }
            }
        }
        return FAILURE;
    }

    static bool ExecuteActionFunctionTag(Regex _tagExpression, ref string _line)
    {
        int startBracketPos = 0;
        int endBracketPos = 0;

        for (int index = (int)CursorPosition; index < _line.Length; index++)
        {
<<<<<<< HEAD
            Debug.Log("Okay...");
            if (_line[index] == '<')
                startBracketPos = index;

            if (_line[index] == '>')
            {
                endBracketPos = index;
=======

            if (_line.Substring((int)CursorPosition, _tag.Length).Contains(_tag))
            {
                _line = _line.Remove((int)CursorPosition , _tag.Length);

                Dialogue[(int)LineIndex] = _line;
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                try
                {
<<<<<<< HEAD
                    string tag = _line.Substring(startBracketPos, (endBracketPos - startBracketPos) + 1);



                    if (_tagExpression.IsMatch(tag))
                    {
                        Debug.Log("ACTION IS FANTASTIC BEBE!!!");
=======
                    ShiftCursorPosition(ActionString.Length);
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                        if (OnDelay == false)
                        {
                            string value = "*" + tag.Split('<')[1].Split('=')[1].Split('>')[0] + "*";

                            Debug.Log(value);

                            _line = _tagExpression.Replace(tag, value);

                            ShiftCursorPosition(value.Length);

                            

                            Dialogue[(int)LineIndex] = _line;
                        }
                        return SUCCESSFUL;
                    }
                }
                catch { }
            }
        }
        return FAILURE;
    }

    static bool ExecuteWaitFunctionTag(Regex _tagExpression, ref string _line)
    {
        /*The wait command will take a 4 digit number.
         We will then convert this into a value that is easily understood
         by textSpeed. We'll be mainly affecting the textSpeed to create our
         WAIT function... unless...*/
        int startBracketPos = 0;
        int endBracketPos = 0;
        if (_line.Substring((int)CursorPosition, "<halt=".Length).Contains("<halt="))
        {
            Debug.Log("HI!!!");
            for (int index = (int)CursorPosition; index < _line.Length; index++)
            {
                if (_line[index] == '<')
                    startBracketPos = index;

                if (_line[index] == '>')
                {
                    endBracketPos = index;


<<<<<<< HEAD
                    string tag = _line.Substring(startBracketPos, (endBracketPos - startBracketPos) + 1);
=======
                string value = _line.Substring((int)CursorPosition, 7);

                Debug.Log(value);
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                    try
                    {

                        if (_tagExpression.IsMatch(tag))
                        {
                            Debug.Log("WAIT IS FANTASTIC BEBE!!!");

<<<<<<< HEAD
                            _line = _tagExpression.Replace(tag, "");
=======
                if (match.Success)
                {
                    Debug.Log("Okay");

                    string newValue = Regex.Replace(value, @"[^\d]", "");
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                            Dialogue[(int)LineIndex] = _line;

<<<<<<< HEAD
                            /*Now we do a substring from the current position to 4 digits.*/
=======
                        Debug.Log("Waiting for " + newValue + " milliseconds...");

                        Instance.StartCoroutine(DelaySpan(millsecs));
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!

                            int value = Convert.ToInt32(tag.Split('<')[1].Split('=')[1].Split('>')[0]);

                            int millsecs = Convert.ToInt32(value);

                            Instance.StartCoroutine(DelaySpan(millsecs));

                            Dialogue[(int)LineIndex] = _line;

                            return SUCCESSFUL;

                        }
                    }
                    catch { }
                }
                else
                    Debug.Log("Nope");
            }
        }
        return FAILURE;
    }


    #region EXECUTE EXPRESSSION
    static bool ExecuteExpressionFunctionTag(Regex _tagExpression, ref string _line)
    {


        bool notFlaged = true;
        int index = 0;
        int startBracketPos = 0;
        int endBracketPos = 0;
        if (_line.Substring((int)CursorPosition, "<exp=".Length).Contains("<exp="))
        {
            foreach (char letter in _line)
            {
                index = (int)CursorPosition;

                if (letter == '<')
                    startBracketPos = index;

                if (letter == '>')
                    endBracketPos = index;

                string tag = _line.Substring(startBracketPos, (endBracketPos - startBracketPos) + 1);

                try
                {
                    if (_tagExpression.IsMatch(tag))
                    {
                        Debug.Log("EXPRESSION IS FANTASTIC BEBE!!!");

                        /*The system will now take this information, from 0 to the current position
                         and split it down even further, taking all the information.*/

                        _line = _tagExpression.Replace(tag, "");

                        int value = Convert.ToInt32(tag.Split('<')[1].Split('=')[1].Split('>')[0]);

                        Dialogue[(int)LineIndex] = _line;

                        return ValidateExpressionsAndChange(value.ToString(), _line, ref notFlaged);
                    }
                }
                catch { }
            }
        }
        return FAILURE;
    }
    #endregion

    static bool ValidateExpressionsAndChange(string value, string _line, ref bool _notFlag)
    {
        //Check if a key matches
        string data = STRINGNULL;

        if (PARSER.DefinedExpressions.ContainsKey(value))
        {
            if (value.GetType() == typeof(string))
            {
                data = FindKey(value, PARSER.DefinedExpressions);
                _notFlag = false;
            }
        }

        else if (PARSER.DefinedExpressions.ContainsValue(Convert.ToInt32(value)))
        {
            if (Convert.ToInt32(value).GetType() == typeof(int))
            {

                data = FindValueAndConvertToKey(Convert.ToInt32(value), PARSER.DefinedExpressions);

                _notFlag = false;
            }
        }

        if (_notFlag)
        {
            //Otherwise, we'll through an error saying this hasn't been defined.
            Debug.LogError(value + " has not been defined. Perhaps declaring it in the associated .dsf File.");
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
<<<<<<< HEAD
            Debug.Log("yes?");

=======
>>>>>>> parent of 4833228... DSL IS NOT BUGGY NO MORE!!!!
            OnDelay = false;
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
        }
    }

    public static void UPDATE_ACTION_STRING(string _value)
    {
        ActionString.Add(_value);
    }

    public static void UPDATE_EXPRESSION_VALUE(string _value)
    {
        ExpressionValue.Add(_value);
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