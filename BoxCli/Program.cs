using BoxLib;
using BoxCli.Commands;
using Spectre.Console;

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
            Console.WriteLine("Welcome to Box CLI!");

            Authenticate(args);

            if (boxUtils == null || boxItemFetcher == null)
            {
                Console.WriteLine("Authentication failed. Exiting.");
                Environment.Exit(1);
            }

            typeaheadReader = new TypeaheadCommandReader(boxItemFetcher); // Initialize

            var commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase)
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

        static void Authenticate(string[] args)
        {
            if (args.Length < 1)
            {
                AnsiConsole.MarkupLine("[bold] Usage: BoxCli <clientId> <clientSecret>[/]");
                Environment.Exit(1);
            }
            string configFile = args[0];
            string? asUser = args.Length > 1 ? args[1] : null;
            boxUtils = new BoxUtils(configFile, asUser);
            boxItemFetcher = new BoxItemFetcher(boxUtils);
            boxItemFetcher.PopulateItemsAsync(GetCurrentFolderId()).Wait();
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
