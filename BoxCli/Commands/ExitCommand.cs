using System.CommandLine;

namespace BoxCli
{
    public partial class Program
    {
        public System.CommandLine.Command ExitCommand()
        {
            var command = new System.CommandLine.Command("exit", "Exit the Box terminal");
            command.SetHandler(() =>
            {
                Console.WriteLine("Exiting Box terminal...");
                continueExecution = false;
            });
            return command;
        }
    }
}