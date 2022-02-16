using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ComradeMajor.Database;
using Discord;
using Discord.WebSocket;

namespace ComradeMajor.App
{
    public class MessageProcessor
    {
        private const string kAddressPattern = "товарищ майор";
        private const string kMentionPattern = "товарищ(а|у|а|ем|е) майор(а|у|а|ом|е)";

        private const string kHelpPattern = "те о себе";
        private const string kStatsPattern = "докладывайте|те обстановку";
        private const string kTeaPattern = "ча(й|ю|е|ё)";
        private const string kHerePattern = "(слышите|здесь).*\\?";
        private const string kYearRecapPattern = "подведите итоги года";

        private SocketSelfUser self_;

        public MessageProcessor(SocketSelfUser self)
        {
            self_ = self;
        }

        public async Task ProcessMessage(SocketMessage message, ComradeMajorDbContext context)
        {
            if (message.Author.Id == self_.Id)
                return;

            var text = message.Content.ToLower();

            try
            {
                foreach (var user in message.MentionedUsers)
                {
                    if (user.Username == self_.Username && user.Discriminator == self_.Discriminator)
                    {
                        await ProcessAddressMessage(text, message, context);
                        return;
                    }
                }

                if (Regex.IsMatch(text, kAddressPattern))
                {
                    await ProcessAddressMessage(text, message, context);
                }
                else if (Regex.IsMatch(text, kMentionPattern))
                {
                    await ProcessMentionMessage(text, message);
                }
                else
                {
                    await ProcessNeutralMessage(text, message);
                }
            }
            catch (NotImplementedException)
            {
                await message.Channel.SendMessageAsync("Не положено");
            }
        }

        private async Task ProcessAddressMessage(string normalizedText,
                                                 SocketMessage message,
                                                 ComradeMajorDbContext context)
        {
            bool sentSmth = false;
            if (Regex.IsMatch(normalizedText, kStatsPattern))
            {
                var calc = new StatsCalculator();
                var now = DateTimeOffset.UtcNow;
                var stats = await calc.GetTop(message.Author.Id, context, now - TimeSpan.FromDays(14), now);
                var total = stats.Aggregate(TimeSpan.FromTicks(0), (acc, entry) => acc + entry.Sum);
                var response = string.Join("\n", Enumerable.Empty<string>()
                    .Append($"Пользователь {message.Author.Username}#{message.Author.Discriminator}")
                    .Append($"Всего за последние 2 недели: {Format(total)}")
                    .Concat(stats.Select(e => $"{e.Activity}: {Format(e.Sum)}"))
                );
                await message.Channel.SendMessageAsync(
                    response,
                    messageReference: new MessageReference(message.Id, message.Channel.Id));
                sentSmth = true;
            }
            if (Regex.IsMatch(normalizedText, kHelpPattern))
            {
                throw new NotImplementedException();
            }
            if (Regex.IsMatch(normalizedText, kTeaPattern))
            {
                await message.AddReactionAsync(Reactions.kOk);
                await Task.Delay(500);
                await message.Channel.SendMessageAsync(
                        Reactions.kTea.Name,
                        messageReference: new MessageReference(message.Id, message.Channel.Id));
                sentSmth = true;
            }
            if (Regex.IsMatch(normalizedText, kHerePattern))
            {
                await message.Channel.SendMessageAsync(
                        "Родина слышит!",
                        messageReference: new MessageReference(message.Id, message.Channel.Id));
                sentSmth = true;
            }
            if (Regex.IsMatch(normalizedText, kYearRecapPattern))
            {
                var calc = new StatsCalculator();
                var now = DateTimeOffset.Now;
                var lastYear = now.Year - 1;
                var yearStart = new DateTimeOffset(lastYear, 1, 1, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(now));
                var yearEnd = new DateTimeOffset(lastYear, 12, 31, 23, 59, 59, TimeZoneInfo.Local.GetUtcOffset(now));
                var stats = await calc.GetTop(message.Author.Id, context, yearStart, yearEnd);
                var total = stats.Aggregate(TimeSpan.FromTicks(0), (acc, entry) => acc + entry.Sum);
                var earliestRecord =
                    calc.GetLog(message.Author.Id, context, yearStart, yearEnd)
                        .OrderBy(r => r.Timestamp)
                        .Take(1)
                        .SingleOrDefault();

                string response;
                if (earliestRecord == null)
                {
                    response =
                        $"Пользователь {message.Author.Username}#{message.Author.Discriminator} себя не проявил";
                }
                else
                {
                    var culture = new CultureInfo("ru-RU");
                    var earliestDate = earliestRecord.Timestamp.ToString("d MMMM yyyy, HH:mm", culture);
                    // "d MMMM yyyy, HH:mm"
                    response = string.Join("\n", Enumerable.Empty<string>()
                        .Append($"Пользователь {message.Author.Username}#{message.Author.Discriminator}")
                        .Append($"Досье ведётся с {earliestDate} (UTC)")
                        .Append(string.Empty)
                        .Append($"Всего за {lastYear} год: {Format(total)}")
                        .Append($"Топ-10 за год:")
                        .Concat(stats.Take(10).Select((e, i) => $"{i + 1}. {e.Activity}: {Format(e.Sum)}"))
                    );
                }

                await message.Channel.SendMessageAsync(
                    response,
                    messageReference: new MessageReference(message.Id, message.Channel.Id));
                sentSmth = true;
            }

            if (!sentSmth)
            {
                await ProcessMentionMessage(normalizedText, message);
            }
        }

        private async Task ProcessMentionMessage(string normalizedText, SocketMessage message)
        {
            await message.AddReactionAsync(Reactions.kPositive.Random());
        }

        private async Task ProcessNeutralMessage(string normalizedText, SocketMessage message)
        {
            var rng = new Random();
            var roll = rng.Next(0, 100);
            await Console.Out.WriteLineAsync($"Rolled {roll} of 100");
            if (roll >= 97)
            {
                await message.AddReactionAsync(Reactions.kAll.Random());
            }
        }

        private string Format(TimeSpan ts)
        {
            var parts = Enumerable.Empty<string>();
            int hours = (int)ts.TotalHours;
            if (hours != 0)
			{
				var noun = DeclineEnumeratedNouns(hours, "час", "часа", "часов");
                parts = parts.Append($"{hours} {noun}");
			}
            if (ts.Minutes != 0)
			{
				var noun = DeclineEnumeratedNouns(ts.Minutes, "минута", "минуты", "минут");
                parts = parts.Append($"{ts.Minutes} {noun}");
			}
            if (ts.Seconds != 0)
			{
				var noun = DeclineEnumeratedNouns(ts.Seconds, "секунда", "секунды", "секунд");
                parts = parts.Append($"{ts.Seconds} {noun}");
			}
            if (parts.Count() == 0)
                return "ничего нет";
            return string.Join(" ", parts);
        }
		
        private static string DeclineEnumeratedNouns(int number,
                                                     string nominative,
                                                     string genitive,
                                                     string pluralGenitive)
        {
            switch (number % 10)
            {
                case 1:
                    if (number % 100 != 11)
                        return nominative;
                    break;
                case 2:
                case 3:
                case 4:
                    if ((number % 100) / 10 != 1)
                        return genitive;
                    break;
            }
            return pluralGenitive;
        }
    }
}
