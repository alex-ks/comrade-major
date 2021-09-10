using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ComradeMajor.Database;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace ComradeMajor.App
{
    class Program
    {
        private const string kAppSettingsPath = "appsettings.json";

        public static async Task Main(string[] args)
        {
            var settings = await ReadSettingsAsync();
            if (settings == null)
            {
                await Console.Out.WriteLineAsync("appsettings.json is absent or invalid");
                return;
            }

            var bot = new ComradeMajorBot(settings);
            await bot.StartAsync();

            while (Console.ReadLine() != "exit")
                continue;

            await bot.StopAsync();
        }

        private static async Task<AppSettings?> ReadSettingsAsync()
        {
            if (!File.Exists(kAppSettingsPath))
                return null;

            var settingsStr = await File.ReadAllTextAsync(kAppSettingsPath);
            var settings = JsonConvert.DeserializeObject<AppSettings>(settingsStr);

            if (settings == null || settings.BotToken == null || settings.ConnectionString == null)
                return null;

            return settings;
        }
    }
}
