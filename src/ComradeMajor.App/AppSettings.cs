namespace ComradeMajor.App
{
    public class AppSettings
    {
        public string BotToken { get; }
        public string ConnectionString { get; }

        public AppSettings(string botToken, string connectionString)
        {
            BotToken = botToken;
            ConnectionString = connectionString;
        }
    }
}