using BoxLib;

namespace BoxCli.Commands
{
    class ChangeDirectoryCommand : Command
    {
        public ChangeDirectoryCommand(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            List<BoxItem> folderItems)
            : base(getCurrentFolderId, setCurrentFolderId, boxUtils, folderItems) { }

        public override async Task Execute(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("Usage: cd <folderId>");
                return;
            }
            // Optionally: check if folder exists
            try
            {
                // You may want to validate the folder exists here
                SetCurrentFolderId(argument);
            }
            catch
            {
                Console.WriteLine("Folder not found.");
            }
        }
    }
}