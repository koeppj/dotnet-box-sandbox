using BoxLib;

namespace BoxCli.Commands
{
    abstract class Command
    {
        protected Func<string> GetCurrentFolderId;
        protected Action<string> SetCurrentFolderId;
        protected BoxUtils BoxUtils;
        protected List<BoxItem> FolderItems;

        protected Command(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            List<BoxItem> folderItems)
        {
            GetCurrentFolderId = getCurrentFolderId;
            SetCurrentFolderId = setCurrentFolderId;
            BoxUtils = boxUtils;
            FolderItems = folderItems;
        }

        public abstract Task Execute(string argument);
    }
}