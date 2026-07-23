using SWLOR.Game.Server.Enumeration;
using System.Text.RegularExpressions;

namespace SWLOR.Game.Server
{
    public class ApplicationSettings
    {
        public string LogDirectory { get; }
        public string RedisIPAddress { get; }
        public string BugDiscordWebhookUrl { get; }
        public string HoloNetWebhookUrl { get; }
        public string DMShoutWebhookUrl { get; }
        public string PropertyBroadcastWebhookUrl { get; }
        public string ServerNotificationWebhookUrl { get; }
        public ServerEnvironmentType ServerEnvironment { get; }
        public GameProfileType GameProfile { get; }
        public string DataNamespace { get; }

        private static ApplicationSettings _settings;
        public static ApplicationSettings Get()
        {
            if (_settings == null)
                _settings = new ApplicationSettings();

            return _settings;
        }

        private ApplicationSettings()
        {
            LogDirectory = Environment.GetEnvironmentVariable("SWLOR_APP_LOG_DIRECTORY");
            RedisIPAddress = Environment.GetEnvironmentVariable("NWNX_REDIS_HOST");
            BugDiscordWebhookUrl = Environment.GetEnvironmentVariable("SWLOR_BUG_DISCORD_WEBHOOK_URL");
            HoloNetWebhookUrl = Environment.GetEnvironmentVariable("SWLOR_HOLONET_WEBHOOK_URL");
            DMShoutWebhookUrl = Environment.GetEnvironmentVariable("SWLOR_DM_SHOUT_WEBHOOK_URL");
            PropertyBroadcastWebhookUrl = Environment.GetEnvironmentVariable("SWLOR_PROPERTY_BROADCAST_WEBHOOK_URL");
            ServerNotificationWebhookUrl = Environment.GetEnvironmentVariable("SWLOR_SERVER_NOTIFICATION_WEBHOOK_URL");
            GameProfile = ParseGameProfile(Environment.GetEnvironmentVariable("SWLOR_GAME_PROFILE"));
            DataNamespace = NormalizeDataNamespace(Environment.GetEnvironmentVariable("SWLOR_DATA_NAMESPACE"));

            if (GameProfile == GameProfileType.Shadowrun && string.IsNullOrWhiteSpace(DataNamespace))
            {
                throw new InvalidOperationException(
                    "The Shadowrun game profile requires SWLOR_DATA_NAMESPACE so Erie cannot share world data with another module.");
            }

            var environment = Environment.GetEnvironmentVariable("SWLOR_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(environment) &&
                (environment.ToLower() == "prod" || environment.ToLower() == "production"))
            {
                ServerEnvironment = ServerEnvironmentType.Production;
            }
            else if (!string.IsNullOrWhiteSpace(environment) &&
                     (environment.ToLower() == "test" || environment.ToLower() == "testing"))
            {
                ServerEnvironment = ServerEnvironmentType.Test;
            }
            else
            {
                ServerEnvironment = ServerEnvironmentType.Development;
            }
        }

        public static GameProfileType ParseGameProfile(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                null or "" or "starwars" or "star_wars" => GameProfileType.StarWars,
                "shadowrun" => GameProfileType.Shadowrun,
                _ => throw new InvalidOperationException(
                    $"Unsupported SWLOR_GAME_PROFILE '{value}'. Expected 'starwars' or 'shadowrun'.")
            };
        }

        public static string NormalizeDataNamespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().ToLowerInvariant();
            if (!Regex.IsMatch(normalized, "^[a-z0-9_-]+$"))
            {
                throw new InvalidOperationException(
                    "SWLOR_DATA_NAMESPACE may contain only letters, numbers, underscores, and hyphens.");
            }

            return normalized;
        }

        public static string BuildEntityKeyPrefix(string dataNamespace, string entityTypeName)
        {
            var normalized = NormalizeDataNamespace(dataNamespace);
            return string.IsNullOrWhiteSpace(normalized)
                ? entityTypeName
                : $"{normalized}:{entityTypeName}";
        }

        public static string BuildEntityIndexName(string dataNamespace, string entityTypeName)
        {
            var normalized = NormalizeDataNamespace(dataNamespace);
            return string.IsNullOrWhiteSpace(normalized)
                ? entityTypeName
                : $"{normalized}_{entityTypeName}";
        }

        public static int GetStartingCredits(GameProfileType gameProfile)
        {
            return gameProfile == GameProfileType.Shadowrun ? 20000 : 200;
        }

    }
}
