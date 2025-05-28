using System.CommandLine;

namespace BoxLib
{
    class BoxLibCmds
    {
        public static Command CreateBoxLibCommand()
        {
            var boxLibCommand = new Command("config", "Box Utilities configuration commands")
            {
                new Command("setclient", "Manage BoxLib configuration")
                {
                    new Option<string>("--client-type", "Type of Box client to configure (Jwt, ClientCredentials, OAuth)") {IsRequired = true },
                    new Option<string>("--config-file", "Path and filename of Box SDK App configuration file") { IsRequired = true },
                    new Option<string>("--profile", "Profile name to use for the configuration")
                }
                .SetHandler((string clientType, string configFile, string? profile) =>
                {
                    // Logic to save client configuration
                    BoxCliConfig.SetClientAppConfig(configFile, Enum.Parse<BoxClientType>(clientType, true), profile);
                })  
                ,
                new Command("setuser", "Authentication related commands")
                {
                    new Option<string>("--profile", "Profile name to use for the configuration")
                    {
                        IsRequired = false,
                        ArgumentHelpName = "profile"
                    },
                    new Option<string>("--as-user", "User ID to impersonate (optional)")
                    {
                        IsRequired = false,
                        ArgumentHelpName = "as-user"
                    }
                },
            };

            return boxLibCommand;
        }
    }
}