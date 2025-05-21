// See https://aka.ms/new-console-template for more information
using Box.Sdk.Gen;
using System.CommandLine;
using System.CommandLine.Invocation;

public class BoxFolderLister
{
    private readonly BoxClient _client;

    public BoxFolderLister(string configFile)
    {
        var auth = new BoxJwtAuth(JwtConfig.FromConfigFile(configFile));
        _client = new BoxClient(auth);
    }

    public async Task ListFolderItemsAsync(string folderId)
    {
        var folder = await _client.Folders.GetFolderItemsAsync(folderId);

        if (folder.Entries != null)
        {
            Console.WriteLine($"Items in folder {folderId}: {folder.Entries.Count}");
        }
        else
        {
            Console.WriteLine($"No entries found in folder {folderId}.");
        }
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
        var itemsCommand = new Command("items", "List items in the root folder");
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

        var rootCommand = new RootCommand("Box Folder Lister Tool");
        rootCommand.AddOption(configOption);
        rootCommand.AddCommand(itemsCommand);

        return await rootCommand.InvokeAsync(args);
    }
}

