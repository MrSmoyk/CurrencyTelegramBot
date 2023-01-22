namespace CurrencyTelegramBot.Models.Models;

public class MinorCurrency
{
    public string BaseCurrency = string.Empty;

    public string Currency = string.Empty;

    public double SaleRateNB { get; set; }
    public double PurchaseRateNB { get; set; }
}