// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using BoxLib;

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
                try 
                {
                    var utils = new BoxUtils(configFile, asUser);
                    var items = await utils.ListFolderItemsAsync(id);
                    var done = false;
                    // Loop through all items in the folder
                    // and print their details.
                    // If there are more items, get the next page
                    // and repeat    
                    while (!done)
                    {
                        foreach (var item in items.Items)
                        {
                            Console.WriteLine($"{item.Type} called '{item.Name}' with ID {item.Id}");
                        }
                        if (items.NextMarker == null)
                        {
                            done = true;
                        }
                        else
                        {
                            items = await utils.ListFolderItemsAsync(id, items.NextMarker);
                        }
                    }
                }
                catch (BoxException ex)
                {
                    Console.WriteLine($"Error listing items: {ex.Message}");
                }
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
                try
                {
                    var lister = new BoxUtils(configFile, asUser);
                    var item = await lister.ItemByPathAsync(path);
                    Console.WriteLine($"{item.Type} called '{item.Name}' with ID {item.Id}");
                }
                catch (BoxException ex)
                {
                    Console.WriteLine($"Error getting item for path: {ex.Message}");
                }
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
                try
                {
                    var lister = new BoxUtils(configFile, asUser);
                    var msg = await lister.UploadFile(folderId, filePath);
                    Console.WriteLine(msg);
                }
                catch (BoxException ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
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
                try
                {
                    var msg = await lister.DeleteFile(id);
                    Console.WriteLine(msg);
                }
                catch (BoxException ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                }
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

