using System.CommandLine;

namespace BoxLib
{
    class BoxLibCmds
    {
        public static Command CreateBoxLibCommand()
        {
            var profileOption = new Option<string>("--profile", "Profile name to use for the configuration")
            {
                IsRequired = false,
                ArgumentHelpName = "profile"
            };
            var asUserOption = new Option<string>("--as-user", "User ID to impersonate (optional)")
            {
                IsRequired = true,
                ArgumentHelpName = "as-user"
            };
            var configFileOption = new Option<string>("--config-file", "Path and filename of Box SDK App configuration file")
            {
                IsRequired = true,
                ArgumentHelpName = "config-file"
            };
            var clientTypeOption = new Option<string>("--client-type", "Type of Box client to configure (Jwt, ClientCredentials, OAuth)")
            {
                IsRequired = true,
                ArgumentHelpName = "client-type"
            };
            var setClientConfigCommand = new Command("set-client-config", "Set the Box client configuration")
            {
                profileOption,
                configFileOption,
                clientTypeOption
            };
            setClientConfigCommand.SetHandler((profile, configFile, clientType) =>
            {
                // Logic to set the Box client configuration
                Console.WriteLine($"Setting client config for profile: {profile}, config file: {configFile}, client type: {clientType}");
            }, profileOption, configFileOption, clientTypeOption);

            
            var boxLibCommand = new Command("config", "Box Utilities configuration commands")
            {
                setClientConfigCommand
            };

            return boxLibCommand;
        }
    }
}