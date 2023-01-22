using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Implementations;
using CurrencyTelegramBot.Services.ResourcesAndConstants;
using FluentAssertions;

namespace Service.Tests
{
    public class Tests
    {
        private ExchangeRateService exchangeRateService;

        [SetUp]
        public void Setup()
        {
            exchangeRateService = new ExchangeRateService();
        }

        [Test]
        public async Task GetBaseCurrency_CorrectData_Sucsess()
        {
            var expected = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today
            };

            var result = await exchangeRateService.GetBaseCurrency(DateTime.Today);

            result.Should().NotBeNull();
            result.Should().BeOfType<BaseCurrency>();
            result.BaseCurrencyLit.Should().Be(expected.BaseCurrencyLit);
            result.Date.Should().Be(expected.Date);
        }

        [Test]
        public void GetBaseCurrency_InvalidData_Fail()
        {
            var invalidDate1 = new DateTime(2000, 01, 01);
            var invalidDate2 = new DateTime(3000, 01, 01);

            AggregateException result1 = exchangeRateService.GetBaseCurrency(invalidDate1).Exception;
            var result2 = exchangeRateService.GetBaseCurrency(invalidDate2).Exception;

            result1.Should().NotBeNull();
            result1.Should().BeOfType<AggregateException>();
            result1.Message.Should().Be("One or more errors occurred. (ExchangeRateService.GetBaseCurrency: Wrong date format.)");
            result2.Should().NotBeNull();
            result2.Should().BeOfType<AggregateException>();
            result2.Message.Should().Be("One or more errors occurred. (ExchangeRateService.GetBaseCurrency: Wrong date format.)");

        }

        [Test]
        public async Task GetAvailableRates_CorrectData_Sucsess()
        {
            var testBaseCurrency = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
                ExchangeRate = new List<MinorCurrency>
                {
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRateNB = 50, SaleRateNB = 52},
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRateNB = 53, SaleRateNB = 54}
                }
            };

            var expectedRates = new List<string> { "USD", "EUR" };

            var resultRates = await exchangeRateService.GetAvailableRates(testBaseCurrency);

            resultRates.Should().NotBeNull();
            resultRates.Should().HaveCount(expectedRates.Count);
            resultRates.Should().Equal(expectedRates);
        }

        [Test]
        public async Task GetAllRates_CorrectData_Sucsess()
        {
            var testBaseCurrency = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
                ExchangeRate = new List<MinorCurrency>
                {
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRateNB = 50, SaleRateNB = 52},
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRateNB = 53, SaleRateNB = 54}
                }
            };

            string expected = "Todey's UAH exchange rates are:\nUSD - 50/52\nEUR - 53/54";

            var result = await exchangeRateService.GetAllRates(testBaseCurrency);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetAllRates_InvalidData_ErrorMessages()
        {
            var testBaseCurrency1 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = new DateTime(2000, 01, 01),
            };
            var testBaseCurrency2 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = new DateTime(3000, 01, 01),
            };
            var testBaseCurrency3 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
            };

            string expected1 = MessageResource.ErrorBaseMessage;
            string expected2 = MessageResource.ErrorRatesCount;

            var result1 = await exchangeRateService.GetAllRates(testBaseCurrency1);
            var result2 = await exchangeRateService.GetAllRates(testBaseCurrency2);
            var result3 = await exchangeRateService.GetAllRates(testBaseCurrency3);

            result1.Should().NotBeNull();
            result1.Should().BeEquivalentTo(expected1);

            result2.Should().NotBeNull();
            result2.Should().BeEquivalentTo(expected1);

            result3.Should().NotBeNull();
            result3.Should().BeEquivalentTo(expected2);
        }

        [Test]
        public async Task GetSelectedRates_CorrectData_Sucsess()
        {
            var testBaseCurrency = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
                ExchangeRate = new List<MinorCurrency>
                {
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRateNB = 50, SaleRateNB = 52},
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRateNB = 53, SaleRateNB = 54}
                }
            };

            string expected = "Today`s UAH to USD exchange rate is: 50/52";

            var result = await exchangeRateService.GetSelectedRates(testBaseCurrency, "USD");

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetSelectedRates_InvalidData_ErrorMessages()
        {
            var testBaseCurrency1 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = new DateTime(2000, 01, 01),
            };

            var testBaseCurrency2 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = new DateTime(3000, 01, 01),
            };

            var testBaseCurrency3 = new BaseCurrency()
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
            };

            var testBaseCurrency4 = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
                ExchangeRate = new List<MinorCurrency>
                {
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRateNB = 50, SaleRateNB = 52},
                    new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRateNB = 53, SaleRateNB = 54}
                }
            };

            var testBaseCurrency5 = new BaseCurrency
            {
                BaseCurrencyLit = "UAH",
                Date = DateTime.Today,
                ExchangeRate = new List<MinorCurrency>{
                new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRateNB = 0, SaleRateNB = 0},
                new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRateNB = 53, SaleRateNB = 54}}
            };

            string expected1 = MessageResource.ErrorBaseMessage;
            string expected2 = MessageResource.ErrorRatesCount;
            string expected3 = MessageResource.ErrorCanNotFind;

            var result1 = await exchangeRateService.GetSelectedRates(testBaseCurrency1, "USD");
            var result2 = await exchangeRateService.GetSelectedRates(testBaseCurrency2, "USD");
            var result3 = await exchangeRateService.GetSelectedRates(testBaseCurrency3, "USD");
            var result4 = await exchangeRateService.GetSelectedRates(testBaseCurrency4, "RUR");
            var result5 = await exchangeRateService.GetSelectedRates(testBaseCurrency5, "USD");

            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result3.Should().NotBeNull();
            result4.Should().NotBeNull();
            result5.Should().NotBeNull();

            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected1);
            result3.Should().BeEquivalentTo(expected2);
            result4.Should().BeEquivalentTo(expected3);
            result5.Should().BeEquivalentTo(expected2);
        }
    }
}