// See https://aka.ms/new-console-template for more information
using Box.Sdk.Gen;
using System.CommandLine;

public class BoxFolderLister
{
    private readonly BoxClient _client;

    public BoxFolderLister(string configFile)
    {
        // Initialize the Box client using JWT authentication
        var auth = new BoxJwtAuth(JwtConfig.FromConfigFile(configFile));
        _client = new BoxClient(auth);
    }

    // <summary>
    // Lists items in a specified folder.
    // </summary>
    // <param name="folderId">The ID of the folder to list items from.</param>
    public async Task ListFolderItemsAsync(string folderId)
    {
        var folder = await _client.Folders.GetFolderItemsAsync(folderId);
        if (folder.Entries != null)
        {
            foreach (var item in folder.Entries)
            {
                if (item.FolderMini != null)
                {
                    Console.WriteLine($"Folder: {item.FolderMini.Name} (ID: {item.FolderMini.Id})");
                }
                else if (item.FileFull != null)
                {
                    Console.WriteLine($"File: {item.FileFull.Name} (ID: {item.FileFull.Id})");
                }
                else if (item.WebLink != null)
                {
                    Console.WriteLine($"Web Link: {item.WebLink.Name} (ID: {item.WebLink.Id})");
                }
                else
                {
                    Console.WriteLine($"Unknown item type: {item.GetType()}");
                }
            }
        }
        else
        {
            Console.WriteLine($"No entries found in folder {folderId}.");
        }
    }

    public async Task ItemIdByPath(string path)
    {
        var pathParts = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        var folderId = "0"; // Start with the root folder ID
        var found = true;
        Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink? foundItem = null;
        foreach (var part in pathParts)
        {
            var items = await _client.Folders.GetFolderItemsAsync(folderId);
            if (items.Entries != null)
            {
                var item = FindItemInEntries(part, items.Entries);
                if (item == null)
                {
                    found = false;
                    break;
                }
                foundItem = item;
                folderId = GetIdFromItem(item);
            }
            else
            {
                found = false;
                break;
            }
        }
        if (found && foundItem != null)
        {
            Console.WriteLine($"Found {GetItemType(foundItem)} '{GetNameFromItem(foundItem)}' with ID {GetIdFromItem(foundItem)}");
        }
        else
        {
            Console.WriteLine($"Item '{path}' not found.");
        }
    }

    private static Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink? FindItemInEntries(
        string itemName,
        IReadOnlyList<Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.FolderMini != null && entry.FolderMini.Name == itemName)
            {
                return entry;
            }
            else if (entry.FileFull != null && entry.FileFull.Name == itemName)
            {
                return entry;
            }
            else if (entry.WebLink != null && entry.WebLink.Name == itemName)
            {
                return entry;
            }
        }
        return null;
    }

    private static string GetIdFromItem(
        Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
    {
        if (item.FolderMini != null)
        {
            return item.FolderMini.Id;
        }
        else if (item.FileFull != null)
        {
            return item.FileFull.Id;
        }
        else if (item.WebLink != null)
        {
            return item.WebLink.Id;
        }
        throw new InvalidOperationException("Unknown item type");
    }

    private static string GetItemType(
        Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
    {
        if (item.FolderMini != null)
        {
            return "Folder";
        }
        else if (item.FileFull != null)
        {
            return "File";
        }
        else if (item.WebLink != null)
        {
            return "Web Link";
        }
        throw new InvalidOperationException("Unknown item type");
    }

    private static string GetNameFromItem(
        Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
    {
        if (item.FolderMini != null)
        {
            return item.FolderMini.Name ?? "FolderMini.Name is null";
        }
        else if (item.FileFull != null)
        {
            return item.FileFull.Name ?? "FileFull.Name is null";
        }
        else if (item.WebLink != null)
        {
            return item.WebLink.Name ?? "WebLink.Name is null";
        }
        throw new InvalidOperationException("Unknown item type");
    }
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configOption = new Option<string>(
            "--config",
            "Path to the Box JWT config JSON file"
        )
        { IsRequired = true };
        var itemsCommand = new Command("items", "List items in a folder");
        var idOption = new Option<string>(
            "--id",
            "ID of the folder to list items from"
        )
        { IsRequired = true };
        itemsCommand.AddOption(idOption);
        itemsCommand.SetHandler(
            async (string configFile, string id) =>
            {
                var lister = new BoxFolderLister(configFile);
                await lister.ListFolderItemsAsync(id);
            },
            configOption, idOption
        );

        var getItemForPathCommand = new Command("get-item", "Get Item for a path");
        var pathOption = new Option<string>(
            "--path",
            "Path to the item"
        )
        { IsRequired = true };
        getItemForPathCommand.AddOption(pathOption);
        getItemForPathCommand.SetHandler(
            async (string configFile, string path) =>
            {
                var lister = new BoxFolderLister(configFile);
                await lister.ItemIdByPath(path);
            },
            configOption, pathOption
        );

        var rootCommand = new RootCommand("Box Folder Lister Tool");
        rootCommand.AddOption(configOption);
        rootCommand.AddCommand(itemsCommand);
        rootCommand.AddCommand(getItemForPathCommand);

        return await rootCommand.InvokeAsync(args);
    }
}

