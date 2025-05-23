using BoxLib;

namespace BoxCli.Commands
{
    class ListCommand : Command
    {
        public ListCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            List<BoxItem> folderItems)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, folderItems) { }

        public override async Task Execute(string argument)
        {
            try
            {
                var items = await BoxUtils.ListFolderItemsAsync(GetCurrentFolderId());
                foreach (var item in items.Items)
                {
                    Console.WriteLine($"{item.Type}\t{item.Id}\t{item.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing folder: {ex.Message}");
            }
        }
    }
}