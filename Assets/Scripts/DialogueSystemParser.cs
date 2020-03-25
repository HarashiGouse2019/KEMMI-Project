using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DSLParser
{
    public class DialogueSystemParser
    {
        public class CommandCallLocation
        {
            public string command { get; private set; } = "";
            public int callPosition { get; private set; } = -1;

            public CommandCallLocation(string command, int callPosition)
            {
                this.command = command;
                this.callPosition = callPosition;
            }
        }

        public static char[] delimiters { get; } = { '<', '>', '[', ']', ',' };

        public static string[] tokens { get; } = { "<<", "::", "END", "@" };

        public static string[] keywords { get; } = { "SPEED", "BOLD", "ITALIZE", "UNDERLINE", "SOUND", "EXPRESSION", "ACTION" };

        public static string[] validTextSpeeds { get; } = { "SLOWEST", "SLOWER", "SLOW", "NORMAL", "FAST", "FASTER", "FASTEST" };

        public static List<CommandCallLocation> commandCallLocations = new List<CommandCallLocation>();
        public static bool LINE_HAS(string line, string token) => line.Contains(token);

        public static int skipValue = 0;

        const bool SUCCESSFUL = true;
        const bool FAILURE = false;

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
            int endingBracketPos = 0;

            //Validate if @ is used. If not, we abort.
            if (line[0] == '@')
                line = line.Replace("@ ", "");
            else
                return "";

            for (int value = 0; value < line.Length; value++)
            {
                //Now, how will we get the position of [ and ]?
                if (line[value] == delimiters[2])
                {
                    startingBracketPos = value;
                }

                else if (line[value] == delimiters[3])
                {
                    endingBracketPos = value;

                    /*At this point, we want to see if a command is actually
                     in between the brackets. If there is, then we can remove
                     from the starting point to the end point, and add our new
                     string to our found commands list.*/

                    string command = line.Substring(startingBracketPos, (endingBracketPos - startingBracketPos) + 1);

                    /*Now we have to see if it contains one of the commands.*/
                    foreach (string keyword in keywords)
                    {
                        //If the parser command is a value one, we can remove it.
                        //This will allow the person
                        if (command.Contains(keyword))
                        {
                            foundCommands.Add(command);
                            CommandCallLocation newCall = new CommandCallLocation(command, startingBracketPos);
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
                    ParseToActionTag(commands, ref line);

                if (tagsParser != SUCCESSFUL)
                    line = line.Replace(commands + " ", "");

            }
            /*We finally got it to work!!!*/

            return line;
        }

        static bool ParseToSpeedTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(delimiters[2] + keywords[0]))
            {
                var speedValue = _styleCommand.Split(delimiters)[1].Split(':')[2];

                /*The Dialogue System's ChangeSpeed function used enumerators,
                 so we need to use the array that we have in the parser, and get there indexes*/
                foreach (string speed in validTextSpeeds)
                {
                    if (speedValue == speed)
                    {
                        _line = _line.Replace(_styleCommand + " ", '<' + "sp=" + Array.IndexOf(validTextSpeeds, speed) + '>');
                        return SUCCESSFUL;
                    }
                }
            }
            return FAILURE;
        }

        static bool ParseToActionTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand.Contains(delimiters[2] + keywords[6]))
            {
                var actionString = '*' + _styleCommand.Split(delimiters)[1].Split(':')[2].Split('"')[1] + "* ";

                /*The action function is simply to add two asteriks between a word.
                 For example: [ACTION::"Sighs"] will be replaced by *Sigh* in the text. 
                 
                 Very easy function to do.*/

                /*The skip tag will do the shift of the cursor for use one the system sees this
                 parsed information.*/
                _line = _line.Replace(_styleCommand + " ", "<skip>" + actionString);

                //Skip value will be assigned, so that the system can read it
                skipValue = actionString.Length - 1;

                return SUCCESSFUL;
            }
            return FAILURE;
        }

        static bool ParseToBoldTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == delimiters[2] + keywords[1] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "<b>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == delimiters[2] + keywords[1] + tokens[1] + tokens[2] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "</b>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }

        static bool ParseToItalizeTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == delimiters[2] + keywords[2] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "<i>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == delimiters[2] + keywords[2] + tokens[1] + tokens[2] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "</i>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }

        static bool ParseToUnderlineTag(string _styleCommand, ref string _line)
        {
            if (_styleCommand == delimiters[2] + keywords[3] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "<u>");
                return SUCCESSFUL;
            }

            else if (_styleCommand == delimiters[2] + keywords[3] + tokens[1] + tokens[2] + delimiters[3])
            {
                _line = _line.Replace(_styleCommand + " ", "</u>");
                return SUCCESSFUL;
            }

            return FAILURE;
        }
    }
}
