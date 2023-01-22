namespace CurrencyTelegramBot.Services.ResourcesAndConstants
{
    public class Constants
    {
        public static readonly string[] inputDateFormat = { "dd.MM.yyyy", "d.M.yyyy", "d.M.yy", "dd/MM/yyyy", "d/M/yyyy", "d/M/yy" };

        public const string ApiPB = "https://api.privatbank.ua/p24api/exchange_rates?json&date=";

        public const string telegramBotToken = "Your Token";
    }
}
