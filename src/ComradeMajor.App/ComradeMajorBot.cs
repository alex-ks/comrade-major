
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ComradeMajor.Database;
using Discord;
using Discord.WebSocket;

namespace ComradeMajor.App {
    public class ComradeMajorBot {
        private AppSettings appSettings_;
        // lateinit
        private MessageProcessor processor_ = null!;
        private DiscordSocketClient client_ = new DiscordSocketClient();

        public ComradeMajorBot(AppSettings settings) {
            appSettings_ = settings;
        }

        public async Task StartAsync() {
            client_.Log += Log;
            await client_.LoginAsync(TokenType.Bot, appSettings_.BotToken);
            await client_.StartAsync();
            client_.Ready += OnReady;
            client_.UserUpdated += OnUserUpdated;
            client_.GuildMemberUpdated += OnGuildUserUpdated;
            client_.MessageReceived += OnMessageReceived;
        }

        public async Task StopAsync() {
            await client_.StopAsync();
        }

        private Task OnReady()
        {
            processor_ = new MessageProcessor(client_.CurrentUser);
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage message)
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

        private async Task OnGuildUserUpdated(SocketGuildUser before, SocketGuildUser after)
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

        private async Task OnUserUpdated(SocketUser before, SocketUser after)
        {
            await Console.Out.WriteLineAsync($"User {before.Username} changed");
            if (before.Activity != after.Activity)
            {
                await Console.Out.WriteLineAsync(
                    $"Activity was {before.Activity?.Name ?? "nothing"} and became {after.Activity?.Name ?? "nothing"}");
            }
        }

        private async Task Tick()
        {
            var r = from guild in client_.Guilds
                    from user in guild.Users
                    let activity = user.Activity
                    select Console.Out.WriteLineAsync($"{user.Username} makes {activity?.Name ?? "nothing"}");
            await Task.WhenAll(r);
        }

        private static async Task Log(LogMessage msg) =>
            await Console.Out.WriteLineAsync(msg.ToString());
    }
}