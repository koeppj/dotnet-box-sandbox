using System.CommandLine;
using System.CommandLine.Help;

namespace BoxCli
{
    public partial class Program
    {
        public System.CommandLine.Command HelpCommand()
        {
            var command = new System.CommandLine.Command("help", "Display help information for Box Terminal commands");
            command.SetHandler(() =>
            {
                var helpBuilder = new HelpBuilder(LocalizationResources.Instance);
                helpBuilder.Write(terminalCommand, Console.Out);
            });
            return command;
        }

    }
}