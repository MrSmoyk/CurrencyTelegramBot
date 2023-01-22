using CurrencyTelegramBot.Services.Implementations;
using CurrencyTelegramBot.Services.ResourcesAndConstants;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(Constants.telegramBotToken);

        var botService = new CurrencyBotService();

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: botService.HandleUpdateAsync,
            pollingErrorHandler: botService.HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
           );

        var me = await botClient.GetMeAsync();

        Console.ReadLine();

        cts.Cancel();
    }
}