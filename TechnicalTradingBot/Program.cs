using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TechnicalTradingBot.Bots;
using TechnicalTradingBot.Model;
using TechnicalTradingBot.Providers;

namespace TechnicalTradingBot
{
    internal class Program
    {
        private const string _historyDBPath = "C:\\Virtual CD\\MyProjects\\StockDatabase";
        private static IExecutionOrchestrator _dateTimeProvider;
        public static IExecutionOrchestrator ExecutionOrchestrator { get; private set; }
        public static IStockMarketExecutionProvider StockMarketExecutionProvider { get; private set; }
        public static ICandleProvider CandleProvider { get; private set; }
        public static Dictionary<string, decimal> StartBalance { get; private set; }

        private static string _stockHistoryDatabasePath;
        public static BotSettingsModel HalfYearTradingBotSettings
        {
            get
            {
                return new BotSettingsModel()
                {
                    InstrumentsAmountToTrade = 10,
                    IsShortAvailable = false,
                    ExecutionOrchestrator = ExecutionOrchestrator,
                    StockHistoryDatabasePath = _stockHistoryDatabasePath,
                    StockMarketExecutionProvider = StockMarketExecutionProvider,
                    CandlesProvider = CandleProvider,
                    Currency = Currency.Rub,
                    RecalculationPeriod = new TimeSpan(7, 0, 0, 0),
                };
            }
        }

        static void Main(string[] args)
        {
            _dateTimeProvider = new TestingExecutionOrchestrator(
                DateTime.Now.AddMonths(-2), DateTime.Now);
            ExecutionOrchestrator = _dateTimeProvider;
            _stockHistoryDatabasePath = _historyDBPath;
            CandleProvider = new LocalHistoryCandlesProvider(ExecutionOrchestrator, "Rub");
            StartBalance = new Dictionary<string, decimal>();
            StartBalance.Add("Rub", 100000);
            StartBalance.Add("Usd", 10000);
            StartBalance.Add("Eur", 10000);
            StockMarketExecutionProvider = new FakeStockMarketExecutionProvider(ExecutionOrchestrator,
                CandleProvider, StartBalance);

            Execute();
        }

        static void Execute()
        {
            var bot = new TechnicalIndicatorsBot(HalfYearTradingBotSettings);
            bool isExecutionCompleted = false;
            var startBalance = StartBalance["Rub"];
            var resultBalance = startBalance;
            bot.StartExecution();
            _dateTimeProvider.StartExecution(() =>
            {
                isExecutionCompleted = true;
            });
            bot.SubscribeOnOutputMessage((message) =>
            {
                var profit = CalculateTotalProfitRate(
                    StockMarketExecutionProvider.GetClosedPositionResults());
                Console.WriteLine($"{message} ({profit}%)");
            }, BotOutputCategory.BotOperation);
            while (!isExecutionCompleted)
                Thread.Sleep(1000);
            var totalTrades = StockMarketExecutionProvider.GetClosedPositionResults();
            var totalProfit = CalculateTotalProfitRate(totalTrades);
            Console.WriteLine($"Results Execution balance {startBalance + ((startBalance / 100) * totalProfit)}," +
                $" Start balance {startBalance}." +
                $" Profit: {totalProfit}." +
                $" Total trades: {totalTrades.Count}" +
                $" Total Succesful Trades {totalTrades.Where(t => t.Profit > 0).ToList().Count}");
        }

        private static decimal CalculateTotalProfitRate(List<PositionDataModel> data)
        {
            var balance = StartBalance["Rub"];
            var resultsBalance = balance;
            foreach(var position in data.Where(d => d.Currency == "Rub"))
            {
                resultsBalance += balance / 100 * position.Profit;
            }
            return (resultsBalance - balance) / balance * 100;
        }
    }
}
