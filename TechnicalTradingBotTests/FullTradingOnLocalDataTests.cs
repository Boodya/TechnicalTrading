using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using TechnicalTradingBot.Bots;
using TechnicalTradingBot.Providers;
using TechnicalTradingBotTests.Models;

namespace TechnicalTradingBotTests
{
    public class Tests
    {
        private TradingBotSettings _settings;
        private const string _historyDBPath = "C:\\Virtual CD\\MyProjects\\StockDatabase";
        private IExecutionOrchestrator _dateTimeProvider;
        [SetUp]
        public void Setup()
        {
            _dateTimeProvider = new TestingExecutionOrchestrator(
                DateTime.Now.AddMonths(-6), DateTime.Now);
            _settings = new TradingBotSettings(_dateTimeProvider, "Rub", _historyDBPath);
        }

        [Test]
        public void HalfYearSimulatingTestRub()
        {
            var bot = new TechnicalIndicatorsBot(_settings.HalfYearTradingBotSettings);
            bool isExecutionCompleted = false;
            var startBalance = _settings.StartBalance["Rub"];
            var resultBalance = startBalance;
            bot.StartExecution();
            _dateTimeProvider.StartExecution(() =>
            {
                var results = _settings.StockMarketExecutionProvider.GetClosedPositionResults();
                results.Where(r=>r.Currency == "Rub").OrderBy(r => r.PositionOpenedDate).ToList().ForEach(r =>
                {
                    resultBalance += resultBalance / 100 * r.Profit;
                });
                isExecutionCompleted = true;
            });
            bot.SubscribeOnOutputMessage((message) =>
            {
                Console.WriteLine(message);
            }, BotOutputCategory.BotOperation);
            while (!isExecutionCompleted)
                Thread.Sleep(1000);
            Assert.IsTrue(resultBalance > startBalance);
        }
    }
}