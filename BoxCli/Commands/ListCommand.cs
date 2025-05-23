using BoxLib;

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
            try
            {
                var items = boxItemFetcher.GetItemsSnapshot();
                foreach (var item in items)
                {
                    Console.WriteLine($"{item.Type}\t{item.Id}\t{item.Name}");
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