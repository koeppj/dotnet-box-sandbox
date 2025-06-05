// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using BoxLib;

class Program
{
    static async Task<int> Main(string[] args)
    {
            var profileOption = new Option<string?>(
                "--profile",
                () => null,
                "Box profile name to use for authentication (optional)"
            )
            { IsRequired = false };
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
            itemsCommand.AddOption(profileOption);
            itemsCommand.AddOption(asUserOption);
            itemsCommand.SetHandler(
                async (string? profile, string id, string asUser) =>
                {
                    try 
                    {
                        var utils = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
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
                                Console.WriteLine($"{item.Type.ToString().ToLower()} called '{item.Name}' with ID {item.Id}");
                            }
                            Console.WriteLine($"Total items: {items.Items.Count} and next marker: {items.NextMarker}");
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
                profileOption, idOption, asUserOption
            );

            var getItemForPathCommand = new Command("get-item", "Get Item for a path");
            var pathOption = new Option<string>(
                "--path",
                "Path to the item"
            )
            { IsRequired = true };
            getItemForPathCommand.AddOption(pathOption);
            getItemForPathCommand.AddOption(profileOption);
            getItemForPathCommand.AddOption(asUserOption);
            getItemForPathCommand.SetHandler(
                async (string? profile, string path, string asUser) =>
                {
                    try
                    {
                        var lister = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
                        var item = await lister.ItemByPathAsync(path);
                        Console.WriteLine($"{item.Type} called '{item.Name}' with ID {item.Id}");
                    }
                    catch (BoxException ex)
                    {
                        Console.WriteLine($"Error getting item for path: {ex.Message}");
                    }
                },
                profileOption, pathOption, asUserOption
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
            uploadCommand.AddOption(profileOption);
            uploadCommand.AddOption(asUserOption);
            uploadCommand.SetHandler(
                async (string? profile, string folderId, string filePath, string asUser) =>
                {
                    try
                    {
                        var lister = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
                        var msg = await lister.UploadFile(folderId, filePath);
                        Console.WriteLine(msg);
                    }
                    catch (BoxException ex)
                    {
                        Console.WriteLine($"Error uploading file: {ex.Message}");
                    }
                },
                profileOption, folderIdOption, filePathOption, asUserOption
            );

            var addMetadataCommand = new Command("add-metadata", "Add metadata to an item");
            addMetadataCommand.AddOption(idOption);
            addMetadataCommand.AddOption(profileOption);
            addMetadataCommand.AddOption(asUserOption);
            addMetadataCommand.SetHandler(
                async (string? profile, string asUser, string id) =>
                {
                    var lister = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
                    await lister.ApplyMatadataToItem(id);
                },
                profileOption, asUserOption, idOption
            );

            var deleteFleCommand = new Command("delete-file", "Delete a file");
            deleteFleCommand.AddOption(idOption);
            deleteFleCommand.AddOption(profileOption);
            deleteFleCommand.AddOption(asUserOption);
            deleteFleCommand.SetHandler(
                async (string? profile, string asUser, string id) =>
                {
                    var lister = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
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
                profileOption, asUserOption, idOption
            );

            var itemInfoCommand = new Command("item-info", "Get information about an item");
            itemInfoCommand.AddOption(idOption);
            itemInfoCommand.AddOption(profileOption);
            itemInfoCommand.AddOption(asUserOption);
            itemInfoCommand.SetHandler(
                async (string? profile, string asUser, string id) =>
                {
                    var lister = profile != null ? new BoxUtils(profile, asUser) : new BoxUtils();
                    try
                    {
                        var item = await lister.GetItemInfoNByIdAsync(id);
                        if (item == String.Empty)
                        {
                            Console.WriteLine($"Item with ID {id} not found.");
                            return;
                        }
                        Console.WriteLine(item);
                    }
                    catch (BoxException ex)
                    {
                        Console.WriteLine($"Error getting item info: {ex.Message}");
                    }
                },
                profileOption, asUserOption, idOption
            );

            var rootCommand = new RootCommand("Box Folder Lister Tool");
            rootCommand.AddOption(profileOption);
            rootCommand.AddOption(asUserOption);
            rootCommand.AddCommand(itemsCommand);
            rootCommand.AddCommand(getItemForPathCommand);
            rootCommand.AddCommand(uploadCommand);
            rootCommand.AddCommand(addMetadataCommand);
            rootCommand.AddCommand(deleteFleCommand);
            rootCommand.AddCommand(itemInfoCommand);
            rootCommand.AddCommand(BoxLibCmds.CreateBoxLibCommand());
            rootCommand.Description = "A command line tool for Box folder operations.";

            return await rootCommand.InvokeAsync(args);
    }
}

