using System.CommandLine;
using System.Text.RegularExpressions; // Add for regex

namespace BoxCli
{
    partial class Program
    {
        private Task DoListCmd(string? filter = null, string? type = null)
        {
            if (boxItemFetcher == null)
            {
                Console.WriteLine("BoxItemFetcher is not initialized.");
                return Task.CompletedTask;
            }
            try
            {
                var items = boxItemFetcher.GetItemsSnapshot();
                Regex? filterRegex = null;
                if (!string.IsNullOrEmpty(filter))
                {
                    var pattern = "^" + Regex.Escape(filter).Replace("\\*", ".*") + "$";
                    filterRegex = new Regex(pattern, RegexOptions.IgnoreCase);
                }
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(type) && item.Type.ToString().ToLower() != type.ToLower())
                        continue;
                    if (filterRegex != null && !filterRegex.IsMatch(item.Name))
                        continue;
                    Console.WriteLine($"{item.Type.ToString().ToLower()}\t{item.Id}\t{item.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing folder: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private System.CommandLine.Command ListCommand()
        {
            var filterArg = new System.CommandLine.Argument<string?>("filter", "Optional filter string")
            {
                Arity = System.CommandLine.ArgumentArity.ZeroOrOne
            };
            var typeOption = new System.CommandLine.Option<string>(["--type", "-t"], "Type to filter (folder/file)");
            var command = new System.CommandLine.Command("ls", "List items in the current folder")
            {
                filterArg,
                typeOption
            };
            command.SetHandler(async (string? filter, string? type) =>
            {
                await DoListCmd(filter, type);
            }, filterArg, typeOption);
            return command;
        }
    }
}