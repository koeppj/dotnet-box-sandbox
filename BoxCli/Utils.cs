using System;
using System.Collections.Generic;
using System.Text;

namespace BoxCli
{
    public static class Utils
    {
        public static string[] ParseWords(string input)
        {
            var words = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';
            bool escape = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (escape)
                {
                    // Add literally, regardless of context
                    sb.Append(c);
                    escape = false;
                }
                else if (c == '\\')
                {
                    // Start escape sequence
                    escape = true;
                }
                else if (inQuotes)
                {
                    if (c == quoteChar)
                    {
                        // End of quoted section
                        inQuotes = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (c == '"' || c == '\'')
                {
                    // Start quoted section
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        words.Add(sb.ToString());
                        sb.Clear();
                    }
                    // Skip additional spaces
                    while (i + 1 < input.Length && char.IsWhiteSpace(input[i + 1]))
                    {
                        i++;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            // Add last word, if any
            if (sb.Length > 0)
                words.Add(sb.ToString());

            return words.ToArray();
        }
    }
}
