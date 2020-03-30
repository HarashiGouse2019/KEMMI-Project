using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace DSLParser
{
    public class DialogueSystemParser
    {
        public class CommandCallLocation
        {
            public string Command { get; private set; } = "";
            public int CallPosition { get; private set; } = -1;

            public int CallLine { get; private set; } = -1;

            public int DialogueSetLocation { get; private set; } = -1;

            private CommandCallLocation(string command, int dialogueSetLocation, int callLine, int callPosition)
            {
                this.Command = command;

                this.DialogueSetLocation = dialogueSetLocation;

                this.CallLine = callLine;

                this.CallPosition = callPosition;
            }

            public static CommandCallLocation New(string command, int dialogueSetLocation, int callLine, int callPosition)
                => new CommandCallLocation(command, dialogueSetLocation, callLine, callPosition);

            public object[] GetCallLocation()
            {
                object[] data =
                {
                    DialogueSetLocation,
                    CallLine,
                    CallPosition
                };


                return data;
            }
        }

        public static char[] Delimiters { get; } = { '<', '>', '[', ']', ',' };

        public static string[] Tokens { get; } = { "<<", "::", "END", "@", "???" };

        public static string[] Keywords { get; } = { "SPEED", "BOLD", "ITALIZE", "UNDERLINE", "SOUND", "EXPRESSION", "ACTION", "HALT", "POSE", "INSERT" };

        public static string[] ValidTextSpeeds { get; } = { "SLOWEST", "SLOWER", "SLOW", "NORMAL", "FAST", "FASTER", "FASTEST" };

        public static List<CommandCallLocation> commandCallLocations = new List<CommandCallLocation>();

        public static Dictionary<string, int> DefinedExpressions { get; private set; } = new Dictionary<string, int>();
        public static Dictionary<string, int> DefinedPoses { get; private set; } = new Dictionary<string, int>();
        public static List<string> DefinedCharacters { get; private set; } = new List<string>();
        public static bool LINE_HAS(string line, string token) => line.Contains(token);

        public static int skipValue = 0;
        public static object returnedValue = null;

        const bool SUCCESSFUL = true;
        const bool FAILURE = false;
        const string STRINGNULL = "";
        const string WHITESPACE = " ";

        public static string PARSER_LINE(string line)
        {
            /*The parsering process in the scripting language will be the most challenging thing!!! So challenging that
             I'm not even scared about the whole hand-drawn animations! I'm more scared of getting this parser system completed.
            Here's what I want to do, 
         

            We will most definitely be passing in a line in order to take each detected command off from that line, and 
            pass it back so that it can be displayed!

            We'll need 2 integers; one for the start of [, the other is the start of ], removing it from the string and
            adding it to our list of commands to execute. We should then take what's in between [] and use the Split method.
            (SPEED::SLOW) would be split into SPEED, ::, and SLOW. We check the 3rd element after we split, and check that value.

            This is just for implementing one command! We'll be needing to implement all of the other commands.
             */
            List<string> foundCommands = new List<string>();

            int startingBracketPos = 0;

            for (int value = 0; value < line.Length; value++)
            {
                //Now, how will we get the position of [ and ]?
                if (line[value] == Delimiters[2])
                {
                    startingBracketPos = value;
                }

                if (line[value] == Delimiters[3])
                {


                    /*At this point, we want to see if a command is actually
                     in between the brackets. If there is, then we can remove
                     from the starting point to the end point, and add our new
                     string to our found commands list.*/

                    string command = line.Substring(startingBracketPos, (value - startingBracketPos) + 1);

                    if (startingBracketPos == 0)
                    {
                        DialogueSystem.ShiftCursorPosition(value);
                    }

                    /*Now we have to see if it contains one of the commands.*/
                    foreach (string keyword in Keywords)
                    {
                        //If the parser command is a value one, we can remove it.
                        //This will allow the person
                        if (command.Contains(keyword))
                        {
                            foundCommands.Add(command);

                            CommandCallLocation newCall =
                                CommandCallLocation.New(command, DialogueSystem.DialogueSet, (int)DialogueSystem.LineIndex, startingBracketPos);

                            commandCallLocations.Add(newCall);
                        }
                    }
                }
            }

            foreach (string commands in foundCommands)
            {
                /*For stuff like [BOLD] and [ITALIZE], and [UNDERLINE], we want to replace those with
                 <b>, <i>, and <u>*/

                bool tagsParser =
                    ParseToBoldTag(commands, ref line) ||
                    ParseToItalizeTag(commands, ref line) ||
                    ParseToUnderlineTag(commands, ref line) ||
                    ParseToSpeedTag(commands, ref line) ||
                    ParseToActionTag(commands, ref line) ||
                    ParserToInsertTag(commands, ref line) ||
                    ParseToExpressionTag(commands, ref line) ||
                    ParseToPoseTag(commands, ref line) ||
                    ParseToWaitTag(commands, ref line);

                if (tagsParser != SUCCESSFUL)
                    line = line.Replace(commands + " ", "");
            }
            /*We finally got it to work!!!*/

            return line;
        }

        public static void Define_Expressions()
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

            string line = null;

            int position = 0;

            bool foundExpression = false;

            if (File.Exists(dsPath))
            {
                using (StreamReader fileReader = new StreamReader(dsPath))
                {
                    while (true)
                    {
                        line = fileReader.ReadLine();

                        if (line == STRINGNULL)
                        {
                            if (foundExpression)
                                return;
                        }

                        line.Split(Delimiters);

                        if (line.Contains("<EXPRESSIONS>"))
                        {
                            foundExpression = true;
                            try { GetExpressions(position); } catch { }
                        }

                        position++;
                    }
                }
            }
            Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
        }

        public static void Define_Poses()
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

            string line = null;

            int position = 0;

            bool foundPose = false;

            if (File.Exists(dsPath))
            {
                using (StreamReader fileReader = new StreamReader(dsPath))
                {
                    while (true)
                    {
                        line = fileReader.ReadLine();

                        if (line == STRINGNULL)
                        {
                            if (foundPose)
                                return;
                        }

                        line.Split(Delimiters);

                        if (line.Contains("<POSES>"))
                        {
                            foundPose = true;
                            try { GetPoses(position); } catch { }
                        }

                        position++;
                    }
                }
            }
            Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
        }

        public static void Define_Characters()
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

            string line = null;

            int position = 0;

            bool foundPose = false;

            if (File.Exists(dsPath))
            {
                using (StreamReader fileReader = new StreamReader(dsPath))
                {
                    while (true)
                    {
                        line = fileReader.ReadLine();

                        if (line == STRINGNULL)
                        {
                            if (foundPose)
                                return;
                        }

                        line.Split(Delimiters);

                        if (line.Contains("<CHARACTERS>"))
                        {
                            foundPose = true;
                            try { GetCharacterNames(position); } catch { }
                        }

                        position++;
                    }
                }
            }
            Debug.LogError("File specified doesn't exist. Try creating one in StreamingAssets folder.");
        }

        public static void GetExpressions(int _position)
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

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

                        if (line == STRINGNULL && atTargetLine)
                            return;


                        if (position > _position)
                        {
                            atTargetLine = true;

                            if (line != STRINGNULL)
                            {
                                string[] data = line.Split('=');
                                DefinedExpressions.Add(data[0].Replace(WHITESPACE, STRINGNULL), Convert.ToInt32(data[1].Replace(WHITESPACE, STRINGNULL)));
                            }
                        }

                        position++;
                    }


                }
            }
        }

        public static void GetPoses(int _position)
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

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

                        if (atTargetLine)
                        {
                            if (line == STRINGNULL)
                                return;
                        }


                        if (position > _position)
                        {
                            atTargetLine = true;

                            if (line != STRINGNULL)
                            {
                                string[] data = line.Split('=');
                                DefinedPoses.Add(data[0].Replace(WHITESPACE, STRINGNULL), Convert.ToInt32(data[1].Replace(WHITESPACE, STRINGNULL)));
                            }

                        }

                        position++;
                    }


                }
            }
        }

        public static void GetCharacterNames(int _position)
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

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

                        if (atTargetLine)
                        {
                            if (line == STRINGNULL)
                                return;
                        }


                        if (position > _position)
                        {
                            atTargetLine = true;

                            if (line != STRINGNULL)
                                DefinedCharacters.Add(line);

                        }

                        position++;
                    }
                }
            }
        }

        public static void GetDialogue(int _position)
        {
            string dsPath = Application.streamingAssetsPath + @"/" + DialogueSystem.GET_DIALOGUE_SCRIPTING_FILE();

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
                            return;

                        if (position > _position)
                        {
                            atTargetLine = true;
                            if (line != STRINGNULL && (line[0] == '@' && line[line.Length - 1] == '<'))
                            {
                                foreach (string character in DefinedCharacters)
                                {
                                    string name = STRINGNULL;
                                    try
                                    {
                                        name = line.Substring(1, character.Length) + ":";
                                    }
                                    catch { }

                                    if (name.Contains(character))
                                        line = line.Replace("<", STRINGNULL).Replace("@" + character, "[INSERT::\"" + name + "\"]");

                                    else if (name.Substring(0, Tokens[4].Length).Contains(Tokens[4]))
                                        line = line.Replace("<", STRINGNULL).Replace("@" + Tokens[4], "[INSERT::\"" + Tokens[4] + ":" + "\"]");

                                    else if (name.Substring(0, WHITESPACE.Length).Contains(" "))
                                        line = line.Replace("<", STRINGNULL).Replace("@" + WHITESPACE, STRINGNULL);

                                    else if (!DefinedCharacters.Exists(x => x.Contains(line.Substring(1, line.IndexOf(" ") - 1))))
                                    {
                                        string unidentifiedName = line.Substring(1, line.IndexOf(" ") - 1);
                                        Debug.LogError("Unknown character definition at line " + (position + 1) + ". Did you define \"" + unidentifiedName + "\" under <CHARACTERS>?");
                                        return;
                                    }

                                }

                                DialogueSystem.Dialogue.Add(line);
                            }

                        }
                        position++;
                    }
                }
            }
        }

        static bool ParseToSpeedTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[0]))
            {
                var speedValue = _styleCommand.Split(Delimiters)[1].Split(':')[2];

                /*The Dialogue System's ChangeSpeed function used enumerators,
                 so we need to use the array that we have in the parser, and get there indexes*/
                foreach (string speed in ValidTextSpeeds)
                {
                    if (speedValue == speed)
                    {
                        _line = _line.Replace(_styleCommand, "<" + "sp=" + Array.IndexOf(ValidTextSpeeds, speed) + ">");
                        return SUCCESSFUL;
                    }
                }
            }
            return FAILURE;
        }

        static bool ParseToActionTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[6]))
            {
                var actionString = _styleCommand.Split(Delimiters)[1].Split(':')[2].Split('"')[1];

                /*The action function is simply to add two asteriks between a word.
                 For example: [ACTION::"Sighs"] will be replaced by *Sigh* in the text. 
                 
                 Very easy function to do.*/

                /*The skip tag will do the shift of the cursor for use one the system sees this
                 parsed information.*/
                _line = _line.Replace(_styleCommand, "<action=" + actionString + ">");

                //Skip value will be assigned, so that the system can read it
                DialogueSystem.UPDATE_ACTION_STRING(actionString);


                return SUCCESSFUL;
            }
            return FAILURE;
        }

        static bool ParseToExpressionTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[5]))
            {
                var value = _styleCommand.Split(Delimiters)[1].Split(':')[2];

                /*The Expression Action is going to be a bit complicated.
                 We'll have to create a expression tag, and just have the expression we are looking for.
                 The expression will act exactly like skip, but this is to let the system know that we need
                 it to use the SpriteChanger, and change the image.*/

                /*The skip tag will do the shift of the cursor for use one the system sees this
                 parsed information.*/
                _line = _line.Replace(_styleCommand, "<exp=" + value + ">");

                //Skip value will be assigned, so that the system can read it
                skipValue = (Keywords[5] + "::" + value).Length - 1;
                returnedValue = value;

                return SUCCESSFUL;
            }
            return FAILURE;
        }

        static bool ParseToPoseTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[8]))
            {
                var value = _styleCommand.Split(Delimiters)[1].Split(':')[2];

                /*The Expression Action is going to be a bit complicated.
                 We'll have to create a expression tag, and just have the expression we are looking for.
                 The expression will act exactly like skip, but this is to let the system know that we need
                 it to use the SpriteChanger, and change the image.*/

                /*The skip tag will do the shift of the cursor for use one the system sees this
                 parsed information.*/
                _line = _line.Replace(_styleCommand, "<pose=" + value + ">");

                //Skip value will be assigned, so that the system can read it
                skipValue = (Keywords[5] + "::" + value).Length - 1;
                returnedValue = value;

                return SUCCESSFUL;
            }
            return FAILURE;
        }

        static bool ParserToInsertTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[9]))
            {
                var word = _styleCommand.Split(Delimiters)[1].Replace(Tokens[1], STRINGNULL).Split('"')[1];
                Debug.Log(word);
                /*The action function is simply to add two asteriks between a word.
                 For example: [ACTION::"Sighs"] will be replaced by *Sigh* in the text. 
                 
                 Very easy function to do.*/

                /*The skip tag will do the shift of the cursor for use one the system sees this
                 parsed information.*/
                _line = _line.Replace(_styleCommand, "<ins=" + word + ">");

                return SUCCESSFUL;
            }
            return FAILURE;
        }

        static bool ParseToWaitTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(Delimiters[2] + Keywords[7]))
            {

                var value = _styleCommand.Split(Delimiters)[1].Split(':')[2];

                /*The Wait should be easy enough. We'll be doing inserting a tag that
                  and then add in the number. At that point, the DialogueSystem will update
                  the textSpeed based on the duration. */

                _line = _line.Replace(_styleCommand, "<halt=" + value + ">");

                return SUCCESSFUL;
            }

            return FAILURE;
        }

        static bool ParseToBoldTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == Delimiters[2] + Keywords[1] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand, "<b>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == Delimiters[2] + Keywords[1] + Tokens[1] + Tokens[2] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand, "</b>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }

        static bool ParseToItalizeTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == Delimiters[2] + Keywords[2] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "<i>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == Delimiters[2] + Keywords[2] + Tokens[1] + Tokens[2] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "</i>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }

        static bool ParseToUnderlineTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == Delimiters[2] + Keywords[3] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "<u>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == Delimiters[2] + Keywords[3] + Tokens[1] + Tokens[2] + Delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "</u>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }
    }
}
