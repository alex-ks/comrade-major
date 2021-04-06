﻿using System;
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

        private static DiscordSocketClient client_ = new DiscordSocketClient();
        // lateinit
        private static MessageProcessor processor_ = null!;
        // lateinit
        private static AppSettings appSettings_ = null!;

        public static async Task Main(string[] args)
        {
            var settings = await ReadSettings();
            if (settings == null)
            {
                await Console.Out.WriteLineAsync("appsettings.json is absent or invalid");
                return;
            }
            appSettings_ = settings;

            client_.Log += Log;
            await client_.LoginAsync(TokenType.Bot, appSettings_.BotToken);
            await client_.StartAsync();
            // await client_.SetStatusAsync(UserStatus.Invisible);
            client_.Ready += OnReady;
            client_.UserUpdated += OnUserUpdated;
            client_.GuildMemberUpdated += OnGuildUserUpdated;
            client_.MessageReceived += OnMessageReceived;

            while (Console.ReadLine() != "exit")
                continue;

            await client_.StopAsync();
        }

        static Task OnReady()
        {
            processor_ = new MessageProcessor(client_.CurrentUser);
            return Task.CompletedTask;
        }

        static async Task OnMessageReceived(SocketMessage message)
        {
            var guildChannel = message.Channel as SocketGuildChannel;
            if (guildChannel != null)
            {
                await Console.Out.WriteLineAsync($"{message.Author.Username}@{guildChannel.Guild.Name}#{guildChannel.Name}: {message.Content}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"{message.Author.Username}#{message.Channel.Name}: {message.Content}");
            }

            using (var context = new ComradeMajorDbContext(appSettings_.ConnectionString))
            {
                await processor_.ProcessMessage(message, context);
            }
        }

        static async Task OnGuildUserUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            await Console.Out.WriteLineAsync($"Guild user {before.Username} changed");
            using (var context = new ComradeMajorDbContext(appSettings_.ConnectionString))
            {
                Debug.Assert(before.Id == after.Id);
                Debug.Assert(before.Username == after.Username);
                Debug.Assert(before.Discriminator == after.Discriminator);

                if (context.Find<User>(before.Id) == null)
                {
                    context.Add(new User(after.Id, after.Username, after.Discriminator));
                    await context.SaveChangesAsync();
                }

                if (before.Activity != after.Activity)
                {
                    await Console.Out.WriteLineAsync(
                        $"Main activity was {before.Activity?.Name ?? "nothing"} and became {after.Activity?.Name ?? "nothing"}");
                    if (before.Activity != null)
                    {
                        context.Add(new UserAction(before.Id,
                                                   before.Activity.Name,
                                                   Database.ActionType.kFinished,
                                                   DateTimeOffset.UtcNow));
                    }
                    if (after.Activity != null)
                    {
                        context.Add(new UserAction(after.Id,
                                                   after.Activity.Name,
                                                   Database.ActionType.kStarted,
                                                   DateTimeOffset.UtcNow));
                    }
                    await context.SaveChangesAsync();
                }
            }
        }

        static async Task OnUserUpdated(SocketUser before, SocketUser after)
        {
            await Console.Out.WriteLineAsync($"User {before.Username} changed");
            if (before.Activity != after.Activity)
            {
                await Console.Out.WriteLineAsync(
                    $"Activity was {before.Activity?.Name ?? "nothing"} and became {after.Activity?.Name ?? "nothing"}");
            }
        }

        static async Task Tick()
        {
            var r = from guild in client_.Guilds
                    from user in guild.Users
                    let activity = user.Activity
                    select Console.Out.WriteLineAsync($"{user.Username} makes {activity?.Name ?? "nothing"}");
            await Task.WhenAll(r);
        }

        static async Task Log(LogMessage msg)
        {
            await Console.Out.WriteLineAsync(msg.ToString());
        }

        static async Task<AppSettings?> ReadSettings()
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
