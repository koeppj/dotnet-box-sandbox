using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxCli
{
    public class TypeaheadCommandReader
    {
        private readonly BoxItemFetcher _boxItemFetcher;

        public TypeaheadCommandReader(BoxItemFetcher boxItemFetcher)
        {
            _boxItemFetcher = boxItemFetcher;
        }

        public async Task<string[]> ReadCommandAsync()
        {
            return await Task.Run(() =>
            {
                var buffer = new List<char>();
                var words = new List<string>();
                bool isFirstWord = true;
                ConsoleKeyInfo key;

                while (true)
                {
                    key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        if (buffer.Count > 0)
                        {
                            words.Add(new string(buffer.ToArray()));
                        }
                        Console.WriteLine();
                        break;
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        if (buffer.Count > 0)
                        {
                            words.Add(new string(buffer.ToArray()));
                            buffer.Clear();
                            isFirstWord = false;
                            Console.Write(" ");
                        }
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (buffer.Count > 0)
                        {
                            buffer.RemoveAt(buffer.Count - 1);
                            Console.Write("\b \b");
                        }
                    }
                    else
                    {
                        buffer.Add(key.KeyChar);
                        Console.Write(key.KeyChar);

                        // Typeahead logic for non-command words
                        if (!isFirstWord && buffer.Count > 0)
                        {
                            char firstChar = buffer[0];
                            if (firstChar != '.' && firstChar != '/' && firstChar != '-')
                            {
                                string partial = new string(buffer.ToArray());
                                var match = _boxItemFetcher.GetItemByPartialName(partial);
                                if (match.FirstMatch != null && !match.HasMultipleMatches)
                                {
                                    string completion = match.FirstMatch.Name.Substring(partial.Length);
                                    if (!string.IsNullOrEmpty(completion))
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                        Console.Write(completion);
                                        Console.ResetColor();

                                        var tabKey = Console.ReadKey(intercept: true);
                                        if (tabKey.Key == ConsoleKey.Tab)
                                        {
                                            // Move cursor back over the greyed-out completion
                                            for (int i = 0; i < completion.Length; i++)
                                            {
                                                Console.Write("\b");
                                            }
                                            // Write the completion in normal color
                                            Console.Write(completion);

                                            foreach (var c in completion)
                                            {
                                                buffer.Add(c);
                                            }
                                        }
                                        else if (tabKey.Key == ConsoleKey.Enter)
                                        {
                                            // Move cursor back over the greyed-out completion
                                            for (int i = 0; i < completion.Length; i++)
                                            {
                                                Console.Write("\b");
                                            }
                                            // Write the completion in normal color
                                            Console.Write(completion);

                                            foreach (var c in completion)
                                            {
                                                buffer.Add(c);
                                            }

                                            if (buffer.Count > 0)
                                            {
                                                words.Add(new string(buffer.ToArray()));
                                            }
                                            Console.WriteLine();
                                            break;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < completion.Length; i++)
                                            {
                                                Console.Write("\b \b");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return words.ToArray();
            });
        }
    }
}