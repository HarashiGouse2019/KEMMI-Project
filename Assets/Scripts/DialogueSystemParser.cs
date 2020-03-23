using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DSLParser
{
    public class DialogueSystemParser
    {
        public static char[] delimiters { get; } = { '<', '>', '[', ']' };

        public static string[] tokens { get; } = { "<<", "::" };

        public static string[] keywords { get; } = { "SPEED", "BOLD", "ITALIZE", "UNDERLINE", "SOUND", "CUE" };

        public static string[] validTextSpeeds { get; } = { "SLOWER", "SLOW", "NORMAL", "FAST", "FASTER" };

        public static bool LINE_HAS(string line, string token) => line.Contains(token);
    }
}
