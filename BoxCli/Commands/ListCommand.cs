using BoxLib;
using System.CommandLine;

namespace BoxCli.Commands
{
    class ListCommand : Command
    {
        public ListCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, boxItemFetcher) { }

        public override Task Execute(string[] args)
        {
            // Use System.CommandLine to parse arguments
            string? filter = null;
            string? type = null;

            var filterArg = new System.CommandLine.Argument<string?>("filter", () => null, "Optional filter string");
            var typeOpt = new System.CommandLine.Option<string>(["--type", "-t"], description: "Type to filter (folder/file)");

            var rootCommand = new System.CommandLine.RootCommand
            {
                filterArg,
                typeOpt,
            };

            // Remove the handler assignment, parse manually:
            var parseResult = rootCommand.Parse(args);
            filter = parseResult.GetValueForArgument(filterArg);
            type = parseResult.GetValueForOption(typeOpt);

            try
            {
                var items = boxItemFetcher.GetItemsSnapshot();
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(type) && item.Type.ToString().ToLower() != type.ToLower())
                        continue;
                    if (!string.IsNullOrEmpty(filter) && !item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
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
    }
}