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

    public interface IExecute
    {
        void Execute();
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
    private string dslFileName = "";

    [SerializeField]
    private TextMeshProUGUI TMP_DIALOGUETEXT = null;

    [SerializeField]
    private Image textBoxUI = null;

    private static TextSpeedValue SpeedValue;

    public static float TextSpeed { get; private set; }

    public static List<string> Dialogue = new List<string>();

    public static bool RunningDialogue { get; private set; } = false;

    public static uint LineIndex { get; private set; } = 0;

    public static uint CursorPosition { get; private set; } = 0;

    private static bool OnDelay = false;

    private static bool typeIn;

    public static int DialogueSet { get; private set; } = -1;

    [SerializeField]
    private List<DialogueNode> nodes = new List<DialogueNode>();

    public static int CurrentNode { get; private set; } = -1;

    public static List<DialogueSystemSpriteChanger> DialogueSystemSpriteChangers { get; private set; } = new List<DialogueSystemSpriteChanger>();

    const int reset = 0;

    static readonly string BOLD = "<b>", BOLDEND = "</b>";
    static readonly string ITALIZE = "<i>", ITALIZEEND = "</i>";
    static readonly string UNDERLINE = "<u>", UNDERLINEEND = "</u>";



    static readonly Regex ACTION = new Regex(@"(<)+\w*action=\w*[a-zA-Z ]+(>$)");

    static readonly Regex INSERT = new Regex(@"(<)+\w*ins=\w*[a-zA-Z!@#$%^&*()_\-=\\/<>?,./{}[\|: ]+(>$)");

    static readonly Regex EXPRESSION = new Regex(@"(<)+\w*exp=\w*[A-Z0-9_-]+(>$)");

    static readonly Regex POSE = new Regex(@"(<)+\w*pose=\w*[A-Z0-9_-]+(>$)");

    static readonly Regex HALT = new Regex(@"(<)+\w*halt=\w*[0-9]+(>$)");

    static readonly Regex SPEED = new Regex(@"(<)+\w*sp=\w*[0-6](>$)");

    static readonly string dslFileExtention = ".dsl";
    static readonly string STRINGNULL = "";

    const bool SUCCESSFUL = true;
    const bool FAILURE = false;

    void Awake()
    {
        Instance = this;
        try
        {
            PARSER.Define_Expressions();
            PARSER.Define_Poses();
            PARSER.Define_Characters();
        }
        catch { }
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueSystemSpriteChangers = FIND_ALL_SPRITECHANGERS();
    }

    void FixedUpdate()
    {

        if (!OnDelay && Dialogue.Count != 0)
        {
            ExcludeAllFunctionTags(Dialogue[(int)LineIndex]);
            ExcludeAllStyleTags(Dialogue[(int)LineIndex]);
        }
    }

    public static void Run(int _nodeValue = -1)
    {

        if (InBounds((int)LineIndex, Dialogue) && IS_TYPE_IN() == false)
        {
            Dialogue[(int)LineIndex] = Dialogue[(int)LineIndex].Replace("@ ", "").Replace("<< ", "");

            RunningDialogue = true;

            CurrentNode = _nodeValue;

            //We'll parse the very first dialogue that is ready to be displayed
            Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

            Instance.StartCoroutine(PrintCycle());

        }
    }

    static IEnumerator PrintCycle()
    {
        while (true)
        {
            if (OnDelay)
                continue;

            if (IS_TYPE_IN() == false)
            {
                ENABLE_DIALOGUE_BOX();

                GET_TMPGUI().text = STRINGNULL;

                var text = STRINGNULL;

                if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];


                for (CursorPosition = 0; CursorPosition < text.Length + 1; CursorPosition += (uint)((OnDelay) ? 0 : 1))
                {
                    try
                    {

                        if (LineIndex < Dialogue.Count) text = Dialogue[(int)LineIndex];

                        GET_TMPGUI().text = text.Substring(0, (int)CursorPosition);

                        UPDATE_TEXT_SPEED(SpeedValue);
                    }
                    catch
                    {

                        Debug.LogWarning("Cursor Position is at: " + CursorPosition + ", but text is " + text.Length + " long.");

                    }


                    yield return new WaitForSeconds(TextSpeed);
                }

                SET_TYPE_IN_VALUE(true);
                Instance.StartCoroutine(WaitForResponse());

            }

            yield return null;
        }

    }
    static void ExcludeAllStyleTags(string _text)
    {
        //Bold
        ExcludeStyleTag(BOLD, BOLDEND, ref _text);

        //Italize tag
        ExcludeStyleTag(ITALIZE, ITALIZEEND, ref _text);

        //Underline tag
        ExcludeStyleTag(UNDERLINE, UNDERLINEEND, ref _text);
    }

    static void ExcludeAllFunctionTags(string _text)
    {

        PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);

        //Action tag!
        ExecuteActionFunctionTag(ACTION, ref _text);

        //Insert tag!
        ExecuteInsertFunctionTag(INSERT, ref _text);

        //Speed Command Tag: It will consider all of the possible values.
        ExecuteSpeedFunctionTag(SPEED, ref _text);

        //Expression tag!
        ExecuteExpressionFunctionTag(EXPRESSION, ref _text);

        //Pose tag!
        ExecutePoseFunctionTag(POSE, ref _text);

        //Halt tage
        ExecuteWaitFunctionTag(HALT, ref _text);
    }

    static bool ExcludeStyleTag(string _openTag, string _closeTag, ref string _line)
    {
        try
        {
            if (_line.Substring((int)CursorPosition, _openTag.Length).Contains(_openTag))
            {
                ShiftCursorPosition(_openTag.Length - 1);
                return SUCCESSFUL;
            }
        }
        catch { }

        try
        {
            if (_line.Substring((int)CursorPosition, _closeTag.Length).Contains(_closeTag))
            {
                ShiftCursorPosition(_closeTag.Length - 1);
                return SUCCESSFUL;
            }
        }
        catch { }

        return FAILURE;
    }

    #region This one works
    static bool ExecuteSpeedFunctionTag(Regex _tagExpression, ref string _line)
    {
        try
        {
            string tag = _line.Substring((int)CursorPosition, "<sp=".Length);

            if (tag.Contains("<sp="))
            {
                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);
                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {

                        endTagPos = (Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {

                            //<sp=3>
                            int speed = Convert.ToInt32(tag.Split(PARSER.Delimiters)[1].Split('=')[1]);

                            SpeedValue = (TextSpeedValue)speed;

                            _line = ReplaceFirst(_line, tag, "");

                            Dialogue[(int)LineIndex] = _line;

                            ShiftCursorPosition(-1);

                            return SUCCESSFUL;

                        }
                    }
                }
            }
        }
        catch { }

        return FAILURE;
    }
    #endregion

    #region This one works
    static bool ExecuteActionFunctionTag(Regex _tagExpression, ref string _line)
    {
        try
        {
            string tag = _line.Substring((int)CursorPosition, "<action=".Length);

            if (tag.Contains("<action="))
            {
                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);
                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {

                        endTagPos = (int)(Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            Debug.Log("WOW FANTASTIC BABY ");
                            if (OnDelay == false)
                            {
                                string value = "*" + tag.Split(PARSER.Delimiters)[1].Split('=')[1] + "*";

                                _line = ReplaceFirst(_line, tag, value);

                                ShiftCursorPosition(value.Length);

                                Dialogue[(int)LineIndex] = _line;
                            }
                            return SUCCESSFUL;
                        }
                    }
                }


            }
        }
        catch { }

        return FAILURE;
    }
    #endregion

    static bool ExecuteInsertFunctionTag(Regex _tagExpression, ref string _line)
    {
        try
        {
            string tag = _line.Substring((int)CursorPosition, "<ins=".Length);

            if (tag.Contains("<ins="))
            {
                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);
                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {

                        endTagPos = (int)(Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            if (OnDelay == false)
                            {
                                string value = tag.Split(PARSER.Delimiters)[1].Split('=')[1];

                                Debug.Log(value);

                                _line = ReplaceFirst(_line, tag, value);

                                ShiftCursorPosition(value.Length);

                                Dialogue[(int)LineIndex] = _line;
                            }
                            return SUCCESSFUL;
                        }
                    }
                }


            }
        }
        catch { }

        return FAILURE;
    }

    #region This one works
    static bool ExecuteWaitFunctionTag(Regex _tagExpression, ref string _line)
    {
        /*The wait command will take a 4 digit number.
         We will then convert this into a value that is easily understood
         by textSpeed. We'll be mainly affecting the textSpeed to create our
         WAIT function... unless...*/
        try
        {
            string tag = _line.Substring((int)CursorPosition, "<halt=".Length);
            if (tag.Contains("<halt="))
            {

                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);

                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {
                        endTagPos = (int)(Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            if (OnDelay == false)
                            {
                                /*Now we do a substring from the current position to 4 digits.*/

                                int value = Convert.ToInt32(tag.Split(PARSER.Delimiters)[1].Split('=')[1]);

                                int millsecs = Convert.ToInt32(value);

                                Instance.StartCoroutine(DelaySpan(millsecs));

                                _line = ReplaceFirst(_line, tag, "");

                                Dialogue[(int)LineIndex] = _line;

                                return SUCCESSFUL;

                            }
                        }

                    }
                }
            }
        }
        catch { }

        return FAILURE;
    }
    #endregion

    #region EXECUTE EXPRESSSION
    static bool ExecuteExpressionFunctionTag(Regex _tagExpression, ref string _line)
    {
        try
        {
            bool notFlaged = true;

            string tag = _line.Substring((int)CursorPosition, "<exp=".Length);
            if (tag.Contains("<exp="))
            {
                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);
                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {
                        endTagPos = (Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            /*The system will now take this information, from 0 to the current position
                             and split it down even further, taking all the information.*/

                            string value = tag.Split('<')[1].Split('=')[1].Split('>')[0];

                            _line = ReplaceFirst(_line, tag, "");

                            Dialogue[(int)LineIndex] = _line;

                            return ValidateExpressionsAndChange(value, ref notFlaged);
                        }
                    }
                }
            }
        }
        catch { }
        return FAILURE;
    }
    #endregion

    #region EXECUTE POSE
    static bool ExecutePoseFunctionTag(Regex _tagExpression, ref string _line)
    {
        try
        {
            bool notFlaged = true;

            string tag = _line.Substring((int)CursorPosition, "<pose=".Length);
            if (tag.Contains("<pose="))
            {
                int startTagPos = (int)CursorPosition;
                int endTagPos = 0;
                string stringRange = _line.Substring((int)CursorPosition, _line.Length - (int)CursorPosition);
                foreach (char letter in stringRange)
                {
                    if (letter == '>')
                    {
                        endTagPos = (Array.IndexOf(stringRange.ToCharArray(), letter));

                        tag = Dialogue[(int)LineIndex].Substring(startTagPos, endTagPos + 1);

                        if (_tagExpression.IsMatch(tag))
                        {
                            /*The system will now take this information, from 0 to the current position
                             and split it down even further, taking all the information.*/

                            string value = tag.Split('<')[1].Split('=')[1].Split('>')[0];

                            _line = ReplaceFirst(_line, tag, "");

                            Dialogue[(int)LineIndex] = _line;

                            return ValidatePosesAndChange(value, ref notFlaged);
                        }
                    }
                }
            }
        }
        catch { }
        return FAILURE;
    }
    #endregion

    static bool ValidateExpressionsAndChange(string value, ref bool _notFlag)
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
            Debug.LogError(value + " has not been defined. Perhaps declaring it in the associated .dsl File.");
            return FAILURE;
        }

        //We get the name, keep if it's EXPRESSION or POSE, and the emotion value
        string characterName = data.Split('_')[0];
        string changeType = data.Split('_')[1];
        string characterState = data.Split('_')[2];

        //Now we see if we can grab the image, and have it change...
        DialogueSystemSpriteChanger changer = Find_Sprite_Changer(characterName + "_" + changeType);

        changer.CHANGE_IMAGE(characterState);

        ShiftCursorPosition(-1);

        return SUCCESSFUL;
    }

    static bool ValidatePosesAndChange(string value, ref bool _notFlag)
    {
        //Check if a key matches
        string data = STRINGNULL;

        if (PARSER.DefinedExpressions.ContainsKey(value))
        {
            if (value.GetType() == typeof(string))
            {
                data = FindKey(value, PARSER.DefinedPoses);
                _notFlag = false;
            }
        }

        else if (PARSER.DefinedExpressions.ContainsValue(Convert.ToInt32(value)))
        {
            if (Convert.ToInt32(value).GetType() == typeof(int))
            {

                data = FindValueAndConvertToKey(Convert.ToInt32(value), PARSER.DefinedPoses);

                _notFlag = false;
            }
        }

        if (_notFlag)
        {
            //Otherwise, we'll through an error saying this hasn't been defined.
            Debug.LogError(value + " has not been defined. Perhaps declaring it in the associated " + dslFileExtention + " File.");
            return FAILURE;
        }

        //We get the name, keep if it's EXPRESSION or POSE, and the emotion value
        string characterName = data.Split('_')[0];
        string changeType = data.Split('_')[1];
        string characterState = data.Split('_')[2];

        Debug.Log(characterState);

        //Now we see if we can grab the image, and have it change...
        DialogueSystemSpriteChanger changer = Find_Sprite_Changer(characterName + "_" + changeType);

        changer.CHANGE_IMAGE(characterState);

        ShiftCursorPosition(-1);

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
            ShiftCursorPosition(-1);
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

        int position = 0;

        bool foundDialogueSet = false;

        if (File.Exists(dsPath))
        {
            string line = null;
            CollectDialogue(_dialogueSet, dsPath, out line, ref position, ref foundDialogueSet);
            return;
        }
        Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "<Pending>")]
    private static void CollectDialogue(int _dialogueSet, string dsPath, out string line, ref int position, ref bool foundDialogueSet)
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
                        Debug.Log("Dialogue Set " + _dialogueSet.ToString("D3", CultureInfo.InvariantCulture) + " does not exist. Try adding it to the .dsf referenced.");
                        return;
                    }
                }

                line.Split(PARSER.Delimiters);

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

    public static IEnumerator WaitForResponse()
    {
        while (IS_TYPE_IN())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (LineIndex < Dialogue.Count)
                {
                    Progress();
                    CursorPosition = reset;
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
        if (CurrentNode != -1 || CurrentNode < Instance.nodes.Count)
            Instance.nodes[CurrentNode].ExecuteOnEnd();
    }

    public static void Progress()
    {
        if (LineIndex < Dialogue.Count - 1 && IS_TYPE_IN() == true)
        {
            LineIndex += 1;

            GET_TMPGUI().text = STRINGNULL;
            SET_TYPE_IN_VALUE(false);

            CursorPosition = reset;

            //We'll parse the next dialogue that is ready to be displayed
            Dialogue[(int)LineIndex] = PARSER.PARSER_LINE(Dialogue[(int)LineIndex]);
        }
        else
        {
            End();
        }
    }

    static string ReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
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

    public static string GET_DIALOGUE_SCRIPTING_FILE() => Instance.dslFileName + dslFileExtention;

    public static List<DialogueNode> GET_NODES() => Instance.nodes;

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