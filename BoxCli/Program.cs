using BoxLib;
using BoxCli.Commands;
using Spectre.Console;
using Spectre.Console.Extensions;
using System.CommandLine;
using Box.Sdk.Gen.Managers;

namespace BoxCli
{
    class Program
    {
        static Stack<string> folderPath = new Stack<string>(new[] { "0" }); // "0" is root
        static BoxUtils? boxUtils;
        static BoxItemFetcher? boxItemFetcher;
        static TypeaheadCommandReader? typeaheadReader; // Add this

        static async Task Main(string[] args)
        {           
            // Define command line options
            var configOption = new Option<string>(
                ["--config", "-c"],
                description: "Path to the configuration file.");
            var asUserOption = new Option<string?>(
                ["--as-user", "-u"],
                description: "Make API calls as a user.");

            var rootCommand = new RootCommand
            {
                configOption,
                asUserOption
            };

            var setCientConfigCommand = new System.CommandLine.Command("set-client", "Save client configuration");
            var setClientConfigOption = new Option<string>(
                ["--config", "-c"],
                description: "Path to the configuration file.")
                { IsRequired = true };
            var clientTypeOption = new Option<BoxClientType>(
                ["--client-type", "-t"],
                description: "Client type (Jwt, ClientCredentials, OAuth).");
            setCientConfigCommand.SetHandler((config, clientType) =>
            {
                // Logic to save client configuration
                BoxCliConfig.SetClientAppConfig(config, clientType);
            }, configOption, clientTypeOption);

            rootCommand.Description = "Box CLI";

            string? configFile = null;
            string? asUser = null;

            rootCommand.SetHandler((string config, string? asUserValue) =>
            {
                configFile = config;
                asUser = asUserValue;
            }, configOption, asUserOption);
            rootCommand.Add(setCientConfigCommand);

            await rootCommand.InvokeAsync(args);

            Authenticate(configFile, asUser);

            if (boxUtils == null || boxItemFetcher == null)
            {
                Console.WriteLine("Authentication failed. Exiting.");
                Environment.Exit(1);
            }

            typeaheadReader = new TypeaheadCommandReader(boxItemFetcher); // Initialize

            var commands = new Dictionary<string, Commands.Command>(StringComparer.OrdinalIgnoreCase)
            {
                { "ls", new ListCommand(GetCurrentFolderId, SetCurrentFolderId, boxUtils, boxItemFetcher) },
                { "cd", new ChangeDirectoryCommand(GetCurrentFolderId, SetCurrentFolderId, boxUtils, boxItemFetcher, folderPath) },
                { "del", new DeleteCommand(GetCurrentFolderId, SetCurrentFolderId, boxUtils, boxItemFetcher) }
            };

            while (true)
            {
                AnsiConsole.Markup($"[blue bold]Box:{string.Join("/", folderPath.Reverse())}> [/]");
                var parts = await typeaheadReader.ReadCommandAsync(); // Use the new class
                if (parts.Length == 0)
                    continue;

                var command = parts[0].ToLower();
                var arguments = parts.Skip(1).ToArray();

                if (command == "exit")
                    return;
                if (command == "help")
                {
                    PrintHelp();
                    continue;
                }

                if (commands.TryGetValue(command, out var handler))
                {
                    await handler.Execute(arguments);
                }
                else
                {
                    Console.WriteLine("Unknown command. Type 'help' for options.");
                }
            }
        }

        static string GetCurrentFolderId() => folderPath.Peek();
        static void SetCurrentFolderId(string id)
        {
            folderPath.Push(id);
        }

        static void Authenticate(string? configFile, string? asUser)
        {
            configFile ??= BoxCliConfig.GetConfigFilePath(null);
            if (!File.Exists(configFile))
            {
                AnsiConsole.MarkupLine($"[red]Config file not found: {configFile}[/]");
                Environment.Exit(1);
            }
            boxUtils = new BoxUtils();
            boxItemFetcher = new BoxItemFetcher(boxUtils);
            AnsiConsole.Markup("[bold] Authenticating...[/]");
            boxItemFetcher.PopulateItemsAsync(GetCurrentFolderId()).Spinner(Spinner.Known.Dots);
            AnsiConsole.MarkupLine("[green] Authentication successful![/]");
        }

        static void PrintHelp()
        {
            Console.WriteLine(@"
Available commands:
  ls                  List current directory contents
  cd <folderId>       Change directory to folderId
  del <id>            Delete file or folder by id
  help                Show this help
  exit                Quit the CLI
");
        }
    }
}
