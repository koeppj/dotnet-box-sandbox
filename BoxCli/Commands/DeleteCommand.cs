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
            try
            {
                var itemId = boxItemFetcher.GetItemIdByName(args[0]);
                if (itemId == null)
                {
                    Console.WriteLine($"Item '{args[0]}' not found.");
                    return;
                }
                await boxUtils.DeleteFile(itemId);
                Console.WriteLine($"Item '{args[0]}' deleted successfully.");
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
    }
}