using BoxLib;
using Spectre.Console;

namespace BoxCli.Commands
{
    class ChangeDirectoryCommand : Command
    {
        private Stack<string> folderPath;

        public ChangeDirectoryCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher,
            Stack<string> folderPath)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, boxItemFetcher)
        {
            this.folderPath = folderPath;
        }

        public override async Task Execute(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Usage: cd <folderId|..|path>");
                return;
            }

            var path = args[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in path)
            {
                if (part == "..")
                {
                    if (folderPath.Count > 1)
                        folderPath.Pop();
                }
                else if (part == ".")
                {
                    // Stay in current directory
                }
                else
                {
                    var itemId = boxItemFetcher.GetItemIdByName(part);
                    if (itemId == null)
                    {
                        AnsiConsole.WriteLine($"Folder '{part}' not found.");
                        return;
                    }
                    await boxItemFetcher.PopulateItemsAsync(itemId);
                    folderPath.Push(itemId);
                }
            }
            // Ensure items are loaded for the final folder
            await boxItemFetcher.PopulateItemsAsync(folderPath.Peek());
        }
    }
}