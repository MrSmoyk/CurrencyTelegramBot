using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Interfaces;
using CurrencyTelegramBot.Services.ResourcesAndConstants;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyTelegramBot.Services.Implementations
{
    public class CurrencyBotService : ICurrencyBotService
    {
        private BaseCurrency baseCurrency;
        private readonly ExchangeRateService exchangeRateService;

        private readonly ReplyKeyboardMarkup replyKeyboardMarkup =
            new(new[]
            {
               new KeyboardButton[]{ MessageResource.ReplyMarkupToday },
               new KeyboardButton[]
               {
                   MessageResource.ReplyMarkupTodayUSD,
                   MessageResource.ReplyMarkupTodayEUR
               }
            })
            { ResizeKeyboard = true };

        public CurrencyBotService(BaseCurrency _baseCurrency, ExchangeRateService _exchangeRateService)
        {
            baseCurrency = _baseCurrency;
            exchangeRateService = _exchangeRateService;
        }

        public CurrencyBotService() : this(new BaseCurrency(), new ExchangeRateService())
        {
        }

        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(client, update.Message);
                return;
            }

            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQuery(client, update.CallbackQuery);
            }
        }

        public async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text != null)
            {


                if (message.Text == MessageResource.ReplyMarkupToday)
                {
                    if (baseCurrency.Date != DateTime.Today)
                    {
                        baseCurrency = await exchangeRateService.GetBaseCurrency(DateTime.Today);
                    }

                    await client.SendTextMessageAsync(message.Chat.Id, await exchangeRateService.GetAllRates(baseCurrency),
                        replyToMessageId: message.MessageId, replyMarkup: replyKeyboardMarkup);

                }
                else if (message.Text == MessageResource.ReplyMarkupTodayUSD)
                {
                    if (baseCurrency.Date != DateTime.Today)
                    {
                        baseCurrency = await exchangeRateService.GetBaseCurrency(DateTime.Today);
                    }

                    await client.SendTextMessageAsync(message.Chat.Id, await exchangeRateService.GetSelectedRates(baseCurrency, "USD"),
                        replyToMessageId: message.MessageId, replyMarkup: replyKeyboardMarkup);

                }
                else if (message.Text == MessageResource.ReplyMarkupTodayEUR)
                {
                    if (baseCurrency.Date != DateTime.Today)
                    {
                        baseCurrency = await exchangeRateService.GetBaseCurrency(DateTime.Today);
                    }

                    await client.SendTextMessageAsync(message.Chat.Id, await exchangeRateService.GetSelectedRates(baseCurrency, "EUR"),
                        replyToMessageId: message.MessageId, replyMarkup: replyKeyboardMarkup);
                }
                else if (!DateTime.TryParseExact(message.Text, Constants.inputDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
                {
                    await client.SendTextMessageAsync(message.Chat.Id,
                        MessageResource.ErrorWrongDateFormat,
                        replyToMessageId: message.MessageId, replyMarkup: replyKeyboardMarkup);
                }
                else if (date > DateTime.Today)
                {
                    await client.SendTextMessageAsync(message.Chat.Id,
                        MessageResource.ErrorWrongDateFuture, replyToMessageId: message.MessageId,
                        replyMarkup: replyKeyboardMarkup);
                }
                else if (date < DateTime.Today.AddYears(-4))
                {
                    await client.SendTextMessageAsync(message.Chat.Id,
                        MessageResource.ErrorWrongDateToLongAgo,
                        replyToMessageId: message.MessageId, replyMarkup: replyKeyboardMarkup);
                }
                else
                {
                    if (baseCurrency.Date != date)
                    {
                        baseCurrency = await exchangeRateService.GetBaseCurrency(date);
                    }

                    var availableRates = await exchangeRateService.GetAvailableRates(baseCurrency);

                    if (availableRates.Count == 0)
                    {
                        await client.SendTextMessageAsync(message.Chat.Id,
                        MessageResource.ErrorRatesCount,
                        replyToMessageId: message.MessageId);

                        await Task.CompletedTask;
                    }

                    var buttons = new List<InlineKeyboardButton[]>();

                    for (var i = 0; i < availableRates.Count; i++)
                    {
                        if (availableRates.Count - 1 == i)
                        {
                            buttons.Add(new[] { new InlineKeyboardButton(availableRates[i]) { CallbackData = availableRates[i] } });
                        }
                        else
                        {
                            buttons.Add(new[]
                            {
                                new InlineKeyboardButton(availableRates[i]) {CallbackData = availableRates[i]},
                                new InlineKeyboardButton(availableRates[i + 1])
                                {
                                    CallbackData = availableRates[i + 1]
                                }
                            });
                        }
                        i++;
                    }

                    buttons.Add(new[] { new InlineKeyboardButton("Show all") { CallbackData = "all" } });

                    var keyboard = new InlineKeyboardMarkup(buttons.ToArray());

                    await client.SendTextMessageAsync(message.Chat.Id,
                        MessageResource.CurrencySelect, replyToMessageId: message.MessageId, replyMarkup: keyboard);
                }
            }
        }

        public async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data != null && callbackQuery.Message != null && baseCurrency.ExchangeRate.Count != 0)
            {
                if (baseCurrency.ExchangeRate.Count == 0)
                {
                    await client.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id, MessageResource.ErrorRatesCount);
                }
                else if (baseCurrency.ExchangeRate.Any(c => c.Currency == callbackQuery.Data))
                {
                    await client.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id, await exchangeRateService.GetSelectedRates(baseCurrency, callbackQuery.Data));
                }
                else if (callbackQuery.Data == "all")
                {
                    await client.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id, await exchangeRateService.GetAllRates(baseCurrency));
                }
                else
                {
                    await client.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id, MessageResource.ErrorCanNotFind);
                }
            }
            else if (callbackQuery.Message != null)
            {
                Console.WriteLine("CurrencyBotService.HandleCallbackQuery: CallbackQuery is null.");

                await client.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id, MessageResource.ErrorBaseMessage);
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);

            return Task.CompletedTask;
        }
    }
}
