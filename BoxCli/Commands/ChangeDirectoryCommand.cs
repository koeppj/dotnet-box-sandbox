using Box.Sdk.Gen.Managers;
using BoxLib;
using Spectre.Console;
using System.CommandLine;

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
            await DoWOrk(args[0]);
        }

        public async Task DoWOrk(string path)
        {
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in pathParts)
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

        public static System.CommandLine.Command CreateCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher,
            Stack<string> folderPath)
        {
            var folderNameArg = new Argument<string>("folderId", "The ID of the folder to change to or '..' to go up one level.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            var command = new System.CommandLine.Command("cd", "Change the current working directory in Box.")
            {
                folderNameArg
            };
            command.SetHandler(async (path) =>
            {
                var changeDirectoryCommand = new ChangeDirectoryCommand(
                    getCurrentFolderId,
                    setCurrentFolderId,
                    boxUtils,
                    boxItemFetcher,
                    folderPath);
                await changeDirectoryCommand.DoWOrk(path);
            }, folderNameArg);
            return command;
        }
    }
}