using BoxLib;
using Spectre.Console;
using System.CommandLine;

namespace BoxCli
{
    partial class Program
    {

        public async Task DoChangeDirectory(string path)
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
                    boxItemFetcher.PopulateItemsAsync(itemId);
                    folderPath.Push(itemId);
                }
            }
            // Ensure items are loaded for the final folder
            boxItemFetcher.PopulateItemsAsync(folderPath.Peek());
        }

        private Command ChangeDirectory()
        {
            var folderNameArg = new Argument<string>("folderId", "The ID of the folder to change to or '..' to go up one level.")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            var command = new Command("cd", "Change the current working directory in Box.")
            {
                folderNameArg
            };
            command.SetHandler(async (path) =>
            {
                await DoChangeDirectory(path);
            }, folderNameArg);
            return command;
        }
    }
}