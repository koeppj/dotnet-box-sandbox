using BoxLib;

namespace BoxCli.Commands
{
    abstract class Command
    {
        protected Func<string> GetCurrentFolderId;
        protected Action<string> SetCurrentFolderId;
        protected BoxUtils boxUtils;
        protected BoxItemFetcher boxItemFetcher;

        protected Command(
            Func<string> getCurrentFolderId,
            Action<string> setCurrentFolderId,
            BoxUtils boxUtils,
            BoxItemFetcher boxItemFetcher)
        {
            GetCurrentFolderId = getCurrentFolderId;
            SetCurrentFolderId = setCurrentFolderId;
            this.boxUtils = boxUtils;
            this.boxItemFetcher = boxItemFetcher;
        }

        public abstract Task Execute(string[] args);

    }
}