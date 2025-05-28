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

    public static class BoxCliConfig
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        private static string defaultFileName = "config.json";
        public static string GetConfigFilePath(string? profileName = null)
        {
            string configDir = GetConfigDirPath();
            string configFileName = profileName ?? defaultFileName;
            return Path.Combine(configDir, configFileName);
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

        private static BoxClientConfigObject ReadBoxClientConfig(string? profileName = null)
        {
            var configFilePath = GetConfigFilePath(profileName);
            if (!System.IO.File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {configFilePath}");
            }

            string json = System.IO.File.ReadAllText(configFilePath);
            return JsonSerializer.Deserialize<BoxClientConfigObject>(json, jsonSerializerOptions)
                ?? throw new InvalidOperationException("Failed to deserialize BoxClientConfigObject from config file.");
        }

        private static void WriteBoxClientConfig(BoxClientConfigObject config, string? profileName = null)
        {
            string configFilePath = GetConfigFilePath(profileName);
            string json = JsonSerializer.Serialize(config, jsonSerializerOptions);
            System.IO.File.WriteAllText(configFilePath, json);
        }

        public static void SetClientAppConfig(string appConfigFileName, BoxClientType clientType, string? profileName = null )
        {
            var configFilePath = GetConfigFilePath(profileName);

            if (!System.IO.File.Exists(configFilePath))
            {
                Console.WriteLine($"Client Configuration file not found: {configFilePath}");
                return;
            }

            // Read the config file as a Dictionary<string, object>
            Dictionary<string, object>? clientConfig = null;
            try
            {
                string json = System.IO.File.ReadAllText(appConfigFileName);
                clientConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
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

            // Get the existing config object or create a new one
            var config = ReadBoxClientConfig(configFilePath);
            var boxClientConfigObject = new BoxClientConfigObject
            {
                ClientType = clientType,
                ClientConfig = new Dictionary<string, object> { },
                AsUser = config.AsUser
            };

            try
            {
                boxClientConfigObject = ReadBoxClientConfig(configFilePath);
            }
            catch (JsonException)
            {
                // Handle the case where the config file is not in the expected format
                Console.WriteLine($"Error reading config file: {configFilePath}. Creating a new one.");
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the config file does not exist
                Console.WriteLine($"Config file not found: {configFilePath}. Creating a new one.");
            }

            // Update (or create) the config object with the new client type and config
            config.ClientConfig = clientConfig;
            config.ClientType = clientType;
            try
            {
                WriteBoxClientConfig(config, configFilePath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error writing config file: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied writing config file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error writing config file: {ex.Message}");
            }
        }

        public static string? GetClientAppConfigAsString(string? profileName = null)
        {
            string configFilePath = GetConfigFilePath(profileName);
            if (!System.IO.File.Exists(configFilePath))
            {
                Console.WriteLine($"Configuration file not found: {configFilePath}");
                return null;
            }

            var config = ReadBoxClientConfig(configFilePath);

            // Extract ClientConfig as JSON string
            if (config.ClientConfig == null)
            {
                return null;
            }
            return JsonSerializer.Serialize(config.ClientConfig, jsonSerializerOptions);
        }

        public static string? GetAsUser(string? profileName = null)
        {
            var configFilePath = GetConfigFilePath(profileName);
            var config = ReadBoxClientConfig(configFilePath);
            return config.AsUser;
        }
    }
}
