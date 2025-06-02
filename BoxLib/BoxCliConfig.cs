using System.Text.Json;

namespace BoxLib
{

    public enum BoxClientType
    {
        Jwt,
        ClientCredentials,
        OAuth
    }
    
    public class BoxClientConfigObject
    {
        public string? AsUser { get; set; } = null;
        public BoxClientType ClientType { get; set; } = BoxClientType.Jwt;

        public Dictionary<string, object>? ClientConfig { get; set; } = null;
    }

    public class BoxUtilsConfig
    {
        public string currentProfile { get; set; } = "default";
        public Dictionary<string, BoxClientConfigObject> Profiles { get; set; } = new Dictionary<string, BoxClientConfigObject>
        {
            { "default", new BoxClientConfigObject() }
        };
    }

    public static class BoxCliConfig
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        private static string defaultFileName = "config.json";

        private static string GetConfigFilePath()
        {
            string configDir = GetConfigDirPath();
            return Path.Combine(configDir, defaultFileName);
        }

        private static string GetConfigDirPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "boxutils");
            }
            else
            {
                var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                return !string.IsNullOrEmpty(xdg)
                    ? Path.Combine(xdg, "boxcli")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "boxutils");
            }
        }

        private static BoxUtilsConfig ReadBoxUtilsConfig()
        {
            var configFilePath = GetConfigFilePath();
            if (!System.IO.File.Exists(configFilePath))
            {
                // If config doesn't exist, return a new default config
                return new BoxUtilsConfig();
            }
            string json = System.IO.File.ReadAllText(configFilePath);
            return JsonSerializer.Deserialize<BoxUtilsConfig>(json, jsonSerializerOptions)
                ?? new BoxUtilsConfig();
        }

        private static void WriteBoxUtilsConfig(BoxUtilsConfig config)
        {
            string configFilePath = GetConfigFilePath();
            string json = JsonSerializer.Serialize(config, jsonSerializerOptions);
            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath)!);
            System.IO.File.WriteAllText(configFilePath, json);
        }

        public static void SetClientAppConfig(string appConfigFileName, BoxClientType clientType, string? profileName = null)
        {
            var config = ReadBoxUtilsConfig();
            string profile = profileName ?? config.currentProfile;

            // Read the app config as a Dictionary<string, object>
            Dictionary<string, object>? clientConfig = null;
            try
            {
                string json = File.ReadAllText(appConfigFileName);
                clientConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(json, jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error reading config file as Dictionary: {ex.Message}");
                return;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Config file not found: {ex.Message}");
                return;
            }

            // Get or create the profile
            if (!config.Profiles.ContainsKey(profile))
            {
                config.Profiles[profile] = new BoxClientConfigObject();
            }
            config.Profiles[profile].ClientType = clientType;
            config.Profiles[profile].ClientConfig = clientConfig;

            WriteBoxUtilsConfig(config);
        }

        public static string GetClientAppConfigAsString(string? profileName = null)
        {
            var config = ReadBoxUtilsConfig();
            string profile = profileName ?? config.currentProfile;
            if (!config.Profiles.ContainsKey(profile))
            {
                throw new BoxException($"Profile not found: {profile}");
            }
            var clientConfig = config.Profiles[profile].ClientConfig;
            return JsonSerializer.Serialize(clientConfig, jsonSerializerOptions);
        }

        public static string? GetAsUser(string? profileName = null)
        {
            var config = ReadBoxUtilsConfig();
            string profile = profileName ?? config.currentProfile;
            if (!config.Profiles.ContainsKey(profile))
            {
                return null;
            }
            return config.Profiles[profile].AsUser;
        }

        public static void SetCurrentProfile(string profileName)
        {
            var config = ReadBoxUtilsConfig();
            if (!config.Profiles.ContainsKey(profileName))
            {
                config.Profiles[profileName] = new BoxClientConfigObject();
            }
            config.currentProfile = profileName;
            WriteBoxUtilsConfig(config);
        }

        public static string GetCurrentProfile()
        {
            var config = ReadBoxUtilsConfig();
            return config.currentProfile;
        }

        public static IEnumerable<string> ListProfiles()
        {
            var config = ReadBoxUtilsConfig();
            return config.Profiles.Keys;
        }

        // New method to print all profiles to the console
        public static void PrintProfiles()
        {
            var profiles = ListProfiles();
            Console.WriteLine("Available profiles:");
            foreach (var profile in profiles)
            {
                Console.WriteLine($"- {profile}");
            }
        }

        public static void SetAsUser(string asUser, string? profileName = null)
        {
            var config = ReadBoxUtilsConfig();
            string profile = profileName ?? config.currentProfile;
            if (!config.Profiles.ContainsKey(profile))
            {
                throw new BoxException($"Profile not found: {profile}");
            }
            config.Profiles[profile].AsUser = asUser;
            WriteBoxUtilsConfig(config);
        }
    }
}
