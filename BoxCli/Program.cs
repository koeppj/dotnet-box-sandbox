using BoxLib;
using BoxCli.Commands;

namespace BoxCli
{
    class Program
    {
        static string currentFolderId = "0";
        static BoxUtils? boxUtils;
        static List<BoxItem> folderItems = new List<BoxItem>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to Box CLI!");

            Authenticate(args);

            var commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase)
            {
                { "ls", new ListCommand(() => currentFolderId, id => currentFolderId = id, boxUtils, folderItems) },
                { "cd", new ChangeDirectoryCommand(() => currentFolderId, id => currentFolderId = id, boxUtils, folderItems) },
                { "del", new DeleteCommand(() => currentFolderId, id => currentFolderId = id, boxUtils, folderItems) }
                // Add more commands as needed
            };

            while (true)
            {
                Console.Write($"Box:{currentFolderId}> ");
                var input = Console.ReadLine();
                if (input == null)
                    continue;
                var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                var command = parts[0].ToLower();
                var argument = parts.Length > 1 ? parts[1] : "";

                if (command == "exit")
                    return;
                if (command == "help")
                {
                    PrintHelp();
                    continue;
                }

                if (commands.TryGetValue(command, out var handler))
                {
                    await handler.Execute(argument);
                }
                else
                {
                    Console.WriteLine("Unknown command. Type 'help' for options.");
                }
            }
        }

        static void Authenticate(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: BoxCli <clientId> <clientSecret>");
                Environment.Exit(1);
            }
            string configFile = args[0];
            string? asUser = args.Length > 1 ? args[1] : null;
            boxUtils = new BoxUtils(configFile, asUser);
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
