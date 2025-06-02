using System.CommandLine;

namespace BoxLib
{
    public class BoxLibCmds
    {
        public static Command CreateBoxLibCommand()
        {
            var profileOption = new Option<string>("--profile", "Profile name to use for the configuration")
            {
                IsRequired = false,
                ArgumentHelpName = "profile"
            };
            var asUserOption = new Option<string>("--id", "User ID to impersonate (optional)")
            {
                IsRequired = true,
                ArgumentHelpName = "as-user"
            };
            var configFileOption = new Option<string>("--config-file", "Path and filename of Box SDK App configuration file")
            {
                IsRequired = true,
                ArgumentHelpName = "config-file"
            };
            var clientTypeOption = new Option<BoxClientType>("--client-type", "Type of Box client to configure (Jwt, ClientCredentials, OAuth)")
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
                BoxCliConfig.SetClientAppConfig(configFile, clientType, profile);
            }, profileOption, configFileOption, clientTypeOption);

            var setAsUserCommand = new Command("set-as-user", "Set the user to impersonate")
            {
                profileOption,
                asUserOption
            };
            setAsUserCommand.SetHandler((profile, asUser) =>
            {
                // Logic to set the user to impersonate
                try
                {
                    BoxCliConfig.SetAsUser(asUser, profile);
                }
                catch (BoxException ex)
                {
                    Console.WriteLine($"Error validating profile: {ex.Message}");
                }
            }, profileOption, asUserOption);

            var boxLibCommand = new Command("config", "Box Utilities configuration commands")
            {
                setClientConfigCommand,
                setAsUserCommand
            };

            // New command to list all profiles
            var listProfilesCommand = new Command("list-profiles", "List all Box configuration profiles");
            listProfilesCommand.SetHandler(() =>
            {
                BoxCliConfig.PrintProfiles();
            });
            boxLibCommand.AddCommand(listProfilesCommand);

            // New command to set the current profile
            var setCurrentProfileCommand = new Command("set-current-profile", "Set the current Box configuration profile")
            {
                profileOption
            };
            setCurrentProfileCommand.SetHandler((profile) =>
            {
                BoxCliConfig.SetCurrentProfile(profile);
            }, profileOption);
            boxLibCommand.AddCommand(setCurrentProfileCommand);

            return boxLibCommand;
        }
    }
}