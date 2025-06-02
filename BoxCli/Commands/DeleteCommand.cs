using System.CommandLine;
using BoxLib;

namespace BoxCli.Commands
{
    class DeleteCommand : Command
    {
        public DeleteCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, boxItemFetcher) { }

        public override async Task Execute(string[] args)
        {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Usage: del <fileOrFolderId>");
                return;
            }

            await DoWork(args[0]);
        }

        public async Task DoWork(string itemId)
        {
            try
            {
                await boxUtils.DeleteFile(itemId);
                Console.WriteLine($"Item '{itemId}' deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
            }
            finally
            {
                try
                {
                    await boxItemFetcher.PopulateItemsAsync(GetCurrentFolderId());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error refreshing items: {ex.Message}");
                }
            }
        }

        public static System.CommandLine.Command CreateCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher)
        {
            var itemArg = new System.CommandLine.Argument<string>("itemId", "ID of the file or folder to delete")
            {
                Arity = System.CommandLine.ArgumentArity.ExactlyOne
            };
            var command = new System.CommandLine.Command("del", "Delete a file or folder")
            {
                itemArg
            };
            command.SetHandler(async (item) =>
            {
                var cmd = new DeleteCommand(getCurrentFolderId, setCurrentFolderId, boxUtils, boxItemFetcher);
                await cmd.DoWork(item);
            }, itemArg);
            return command; 
        }
    }
}