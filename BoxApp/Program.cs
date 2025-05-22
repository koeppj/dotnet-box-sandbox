// See https://aka.ms/new-console-template for more information
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configOption = new Option<string>(
            "--config",
            "Path to the Box JWT config JSON file"
        )
        { IsRequired = true };
        var asUserOption = new Option<string>(
            "--as-user",
            "User ID to impersonate"
        )
        { IsRequired = false };
        
        var itemsCommand = new Command("items", "List items in a folder");
        var idOption = new Option<string>(
            "--id",
            "ID of the folder to list items from"
        )
        { IsRequired = true };
        itemsCommand.AddOption(idOption);
        itemsCommand.SetHandler(
            async (string configFile, string id, string asUser) =>
            {
                var lister = new BoxUtils(configFile, asUser);
                await lister.ListFolderItemsAsync(id);
            },
            configOption, idOption, asUserOption
        );

        var getItemForPathCommand = new Command("get-item", "Get Item for a path");
        var pathOption = new Option<string>(
            "--path",
            "Path to the item"
        )
        { IsRequired = true };
        getItemForPathCommand.AddOption(pathOption);
        getItemForPathCommand.SetHandler(
            async (string configFile, string path, string asUser) =>
            {
                var lister = new BoxUtils(configFile, asUser);
                await lister.ItemIdByPath(path);
            },
            configOption, pathOption, asUserOption
        );

        var uploadCommand = new Command("upload", "Upload a file to a folder");
        var folderIdOption = new Option<string>(
            "--folder-id",
            "ID of the folder to upload the file to"
        )
        { IsRequired = true };
        var filePathOption = new Option<string>(
            "--file-path",
            "Path to the file to upload"
        )
        { IsRequired = true };
        uploadCommand.AddOption(folderIdOption);
        uploadCommand.AddOption(filePathOption);
        uploadCommand.SetHandler(
            async (string configFile, string folderId, string filePath, string asUser) =>
            {
                var lister = new BoxUtils(configFile, asUser);
                await lister.UploadFile(folderId, filePath);
            },
            configOption, folderIdOption, filePathOption, asUserOption
        );

        var addMetadataCommand = new Command("add-metadata", "Add metadata to an item");
        addMetadataCommand.AddOption(idOption);
        addMetadataCommand.SetHandler(
            async (string configFile, string asUser, string id) =>
            {
                var lister = new BoxUtils(configFile, asUser);
                await lister.ApplyMatadataToItem(id);
            },
            configOption, asUserOption, idOption
        );

        var deleteFleCommand = new Command("delete-file", "Delete a file");
        deleteFleCommand.AddOption(idOption);
        deleteFleCommand.SetHandler(
            async (string configFile, string asUser, string id) =>
            {
                var lister = new BoxUtils(configFile, asUser);
                await lister.DeleteFile(id);
            },
            configOption, asUserOption, idOption
        );

        var rootCommand = new RootCommand("Box Folder Lister Tool");
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(asUserOption);
        rootCommand.AddCommand(itemsCommand);
        rootCommand.AddCommand(getItemForPathCommand);
        rootCommand.AddCommand(uploadCommand);
        rootCommand.AddCommand(addMetadataCommand);
        rootCommand.AddCommand(deleteFleCommand);

        return await rootCommand.InvokeAsync(args);
    }
}

