using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Interfaces;
using CurrencyTelegramBot.Services.Requests;
using CurrencyTelegramBot.Services.ResourcesAndConstants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace CurrencyTelegramBot.Services.Implementations
{
    public class ExchangeRateService : IExchangeRateService
    {
        public Task<string> GetAllRates(BaseCurrency baseCurrency)
        {
            string response = "";

            try
            {
                if (baseCurrency.BaseCurrencyLit == string.Empty || baseCurrency.Date < DateTime.Today.AddYears(-4) || baseCurrency.Date > DateTime.Today)
                {
                    throw new FormatException(MessageResource.ErrorWrongBaseCurrencyFormat);
                }

                if (baseCurrency.ExchangeRate.Count == 0)
                {
                    response = MessageResource.ErrorRatesCount;
                    return Task.FromResult(response);
                }

                if (baseCurrency.Date == DateTime.Today)
                {
                    response = string.Format(MessageResource.TodeyBaseCurrencyLitForma, baseCurrency.BaseCurrencyLit);
                }
                else
                {
                    response = string.Format(MessageResource.BaseCurrencyLitForma, baseCurrency.BaseCurrencyLit);
                }

                foreach (var currency in baseCurrency.ExchangeRate)
                {
                    if (currency.PurchaseRateNB > 0 && currency.SaleRateNB > 0)
                    {
                        response += "\n" + string.Format(MessageResource.ExchangeRateForma, currency.Currency, currency.PurchaseRateNB, currency.SaleRateNB);
                    }
                }
            }
            catch (Exception ex)
            {
                response = MessageResource.ErrorBaseMessage;
                Console.WriteLine($"ExchangeRateService.GetAllRates: {ex.Message}");
            }

            return Task.FromResult(response);
        }

        public Task<string> GetSelectedRates(BaseCurrency baseCurrency, string currencyLit)
        {
            string response = "";

            try
            {
                if (baseCurrency.BaseCurrencyLit == string.Empty || baseCurrency.Date < DateTime.Today.AddYears(-4) ||
                baseCurrency.Date > DateTime.Today)
                {
                    throw new FormatException(MessageResource.ErrorWrongBaseCurrencyFormat);
                }

                if (baseCurrency.ExchangeRate.Count == 0)
                {
                    return Task.FromResult(MessageResource.ErrorRatesCount);
                }

                if (baseCurrency.Date == DateTime.Today)
                {
                    response += string.Format(MessageResource.TodaySelectedRatesForma, baseCurrency.BaseCurrencyLit, currencyLit);
                }
                else
                {
                    response += string.Format(MessageResource.SelectedRatesForma, baseCurrency.BaseCurrencyLit, currencyLit, $"{baseCurrency.Date:dd.MM.yyyy}");
                }

                if (!baseCurrency.ExchangeRate.Exists(c => c.Currency == currencyLit))
                {
                    return Task.FromResult(MessageResource.ErrorCanNotFind);
                }

                var minorCurrency = baseCurrency.ExchangeRate.First(
                c => c.Currency == currencyLit);

                if (minorCurrency.PurchaseRateNB == 0 || minorCurrency.SaleRateNB == 0)
                {
                    return Task.FromResult(MessageResource.ErrorRatesCount);
                }

                response += string.Format(MessageResource.MinorCurrencyForma, minorCurrency.PurchaseRateNB, minorCurrency.SaleRateNB);
            }
            catch (Exception ex)
            {
                response = MessageResource.ErrorBaseMessage;
                Console.WriteLine($"ExchangeRateService.GetSelectedRates: {ex.Message}");
            }

            return Task.FromResult(response);
        }

        public async Task<BaseCurrency> GetBaseCurrency(DateTime date)
        {
            if (date < DateTime.Today.AddYears(-4) || date > DateTime.Today)
            {
                throw new FormatException("ExchangeRateService.GetBaseCurrency: Wrong date format.");
            }

            try
            {
                var jsonResponse = await new ApiJsonRequest().GetStringResponse(Constants.ApiPB + date.ToString("dd.MM.yyyy"));

                var baseCurrency = JsonConvert.DeserializeObject<BaseCurrency>(jsonResponse, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy" });
                var uahToUah = baseCurrency.ExchangeRate.Find(x => x.Currency == "UAH");

                baseCurrency.ExchangeRate.Remove(uahToUah);

                if (baseCurrency == null || baseCurrency.BaseCurrencyLit == string.Empty ||
                    baseCurrency.Date < DateTime.Today.AddYears(-4) || baseCurrency.Date > DateTime.Today)
                {
                    throw new FormatException(MessageResource.ErrorWrongBaseCurrencyFormat);
                }

                return baseCurrency;

            }
            catch (Exception ex)
            {
                throw new Exception($"ExchangeRateService.GetBaseCurrency: {ex.Message}");
            }
        }

        public Task<List<string>> GetAvailableRates(BaseCurrency baseCurrency)
        {
            List<string> rates = new List<string>();

            foreach (var currency in baseCurrency.ExchangeRate)
            {
                rates.Add(currency.Currency);
            }

            return Task.FromResult(rates);
        }
    }
}
