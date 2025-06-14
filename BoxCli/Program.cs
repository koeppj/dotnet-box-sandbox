﻿using BoxLib;
using BoxCli.Commands;
using Spectre.Console;
using Spectre.Console.Extensions;
using System.CommandLine;
using System.Net;
using System.Text.RegularExpressions;

namespace BoxCli
{
    partial class Program
    {
        // Instance fields
        private Stack<string> folderPath;
        private BoxUtils boxUtils;
        private BoxItemFetcher boxItemFetcher;
        private TypeaheadCommandReader typeaheadReader;
        private System.CommandLine.Command terminalCommand;
        private bool continueExecution;

        public Program(string? profile, string? asUser)
        {
            folderPath = new Stack<string>(new[] { "0" }); // "0" is root
            terminalCommand = new System.CommandLine.Command("terminal", "Box terminal command");
            terminalCommand.SetHandler(() =>
            {
                return;
            });
            continueExecution = true;
            boxUtils = new BoxUtils(profile, asUser);
            boxItemFetcher = new BoxItemFetcher(boxUtils);
            typeaheadReader = new TypeaheadCommandReader(boxItemFetcher);
            boxItemFetcher.PopulateItemsAsync(GetCurrentFolderId()).Spinner(Spinner.Known.Dots);
        }

        public async Task Run()
        {
            terminalCommand.AddCommand(HelpCommand());
            terminalCommand.AddCommand(ExitCommand());
            terminalCommand.AddCommand(ChangeDirectory());
            terminalCommand.AddCommand(DeleteCommand());
            terminalCommand.AddCommand(ListCommand());

            while (continueExecution)
            {
                AnsiConsole.Markup($"[blue bold]Box:{string.Join("/", folderPath.Reverse())}> [/]");
                var parts = await typeaheadReader.ReadCommandAsync();
                await terminalCommand.InvokeAsync(parts);
            }
        }

        private string GetCurrentFolderId() => folderPath.Peek();
        private void SetCurrentFolderId(string id)
        {
            folderPath.Push(id);
        }


        static async Task<int> Main(string[] args)
        {
            // Define command line options
            var profileOption = new Option<string>(
                ["--profile", "-p"],
                description: "Box profile name to use for authentication.");
            var asUserOption = new Option<string?>(
                ["--as-user", "-u"],
                description: "Make API calls as a user.");

            var rootCommand = new RootCommand
            {
                profileOption,
                asUserOption
            };

            rootCommand.AddCommand(BoxLibCmds.CreateBoxLibCommand());

            rootCommand.Description = "Box CLI";
            rootCommand.SetHandler(async (string profile, string? asUser) =>
            {
                AnsiConsole.Markup("[bold] Authenticating...[/]");
                var program = new Program(profile, asUser);
                AnsiConsole.MarkupLine("[green] Authentication successful![/]");
                await program.Run();
            }, profileOption, asUserOption);

            return await rootCommand.InvokeAsync(args);
            
        }
    }
}
