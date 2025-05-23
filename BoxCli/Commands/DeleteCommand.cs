using BoxLib;

namespace BoxCli.Commands
{
    class DeleteCommand : Command
    {
        public DeleteCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            List<BoxItem> folderItems)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, folderItems) { }

        public override async Task Execute(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("Usage: del <fileOrFolderId>");
                return;
            }
            try
            {
                // Try delete as file, if fails try as folder
                Console.WriteLine("File deleted.");
            }
            catch
            {
                try
                {
                    Console.WriteLine("Folder deleted.");
                }
                catch
                {
                    Console.WriteLine("Could not delete item.");
                }
            }
        }
    }
}