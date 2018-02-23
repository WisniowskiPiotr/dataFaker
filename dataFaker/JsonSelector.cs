using System;
using System.Collections.Generic;
using System.Text;

namespace dataFaker
{
    public static class JsonSelector
    {
        public static string GetFirstValueOfToken(string tokenName, string jsonText)
        {
            int start = jsonText.IndexOf("\"" + tokenName + "\":");
            if (start == -1)
                return string.Empty;
            start = start + 3 + tokenName.Length;
            char openChar = jsonText[start];
            return GetSubstringEnclosedIn(jsonText, openChar, start);
        }

        private static char GetMachingEndChar(char startChar)
        {
            switch (startChar)
            {
                case '{':
                    return '}';
                case '[':
                    return ']';
                case '(':
                    return ')';
                case '<':
                    return '>';
                default:
                    return startChar;
            }
        }

        private static string GetSubstringEnclosedIn(string text, char openChar, int startIndex = 0)
        {
            char closeChar = GetMachingEndChar(openChar);
            int oppenedLevels = 0;
            StringBuilder stringBuilder = new StringBuilder();
            int index = startIndex;
            do
            {
                if (index > text.Length - 1)
                {
                    return string.Empty;
                }
                if (text[index] == openChar && (openChar != closeChar || oppenedLevels==0))
                {
                    oppenedLevels++;
                }
                else if (text[index] == closeChar)
                {
                    oppenedLevels--;
                }
                stringBuilder.Append(text[index]);
                index++;
            }
            while (oppenedLevels > 0);
            string result = stringBuilder.ToString();
            result = result.Substring(1, result.Length - 2);
            return result;
        }
    }
}
