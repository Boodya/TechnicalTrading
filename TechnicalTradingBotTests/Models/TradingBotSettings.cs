using System;
using System.Collections.Generic;
using System.Text;
using TechnicalTradingBot.Model;
using TechnicalTradingBot.Providers;
using TinkoffConnector;

namespace TechnicalTradingBotTests.Models
{
    internal class TradingBotSettings
    {
        public IExecutionOrchestrator ExecutionOrchestrator { get; private set; }
        public IStockMarketExecutionProvider StockMarketExecutionProvider { get; private set; }
        public ICandleProvider CandleProvider { get; private set; }
        public Dictionary<string, decimal> StartBalance { get; private set; }

        private string _stockHistoryDatabasePath;
        public BotSettingsModel HalfYearTradingBotSettings { get
            {
                return new BotSettingsModel()
                {
                    InstrumentsAmountToTrade = 3,
                    IsShortAvailable = false,
                    ExecutionOrchestrator = ExecutionOrchestrator,
                    StockHistoryDatabasePath = _stockHistoryDatabasePath,
                    StockMarketExecutionProvider = StockMarketExecutionProvider,
                    CandlesProvider = CandleProvider,
                    Currency = TechnicalTradingBot.Currency.Rub,
                    RecalculationPeriod = new TimeSpan(7, 0, 0, 0),
                };
            } 
        }

        public TradingBotSettings(IExecutionOrchestrator dtProvider, 
            string currency, string stockHistoryDatabasePath)
        {
            ExecutionOrchestrator = dtProvider;
            _stockHistoryDatabasePath = stockHistoryDatabasePath;
            CandleProvider = new LocalHistoryCandlesProvider(ExecutionOrchestrator, "Rub");
            StartBalance = new Dictionary<string, decimal>();
            StartBalance.Add("Rub", 100000);
            StartBalance.Add("Usd", 10000);
            StartBalance.Add("Eur", 10000);
            StockMarketExecutionProvider = new FakeStockMarketExecutionProvider(ExecutionOrchestrator,
                CandleProvider, StartBalance);
        }
    }
}
