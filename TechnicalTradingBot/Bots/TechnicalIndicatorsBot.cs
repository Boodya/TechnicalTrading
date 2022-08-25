using System;
using System.Collections.Generic;
using TechnicalTradingBot.Model;
using StockMarketAnalyzer.History;
using StockMarketAnalyzer.Analyzers;
using StockMarketAnalyzer;
using StockHistoryExporter;
using StockMarketAnalyzer.Models;
using System.Linq;

namespace TechnicalTradingBot.Bots
{
    public class TechnicalIndicatorsBot : BotBase
    {
        private AnalyzerHistoryRepository _analyzerHistory;
        private Dictionary<string, IMarketAnalyzer> _tickerAnalyzers;
        private AnalyzersHub _analyzersHub;
        private readonly object _statusUpdateLocker = new object();
        private DateTime _lastRecalculationOperationDt;
        private decimal _onPositionBalance = 0;

        public TechnicalIndicatorsBot(BotSettingsModel settings) : base(settings)
        {
            _analyzersHub = new AnalyzersHub();
            _settings.ExecutionOrchestrator.
                SubscribeOnUpdate(TimeUpdateOperation);
            Initialize();
        }
        protected override void Execute(CandlesUpdateModel update)
        {
            lock (_statusUpdateLocker)
            {
                if (!_isExecute)
                    return;
                if (Status == ExecutionStatus.ExportStockMarketData
                    || Status == ExecutionStatus.AnalyzingIndicators
                    || Status == ExecutionStatus.Initialization)
                    return;
                if (!_tickerAnalyzers.ContainsKey(update.Ticker))
                    return;
                Status = ExecutionStatus.Trade;
            }

            var ticker = update.Ticker;
            var results = _tickerAnalyzers[update.Ticker].Analyze(update.Candles);
            if (results.Decision == OoplesFinance.StockIndicators.Enums.Signal.None)
                return;

            var operation = OperationType.Buy;
            if (results.Decision == OoplesFinance.StockIndicators.Enums.Signal.StrongSell ||
                results.Decision == OoplesFinance.StockIndicators.Enums.Signal.Sell)
                operation = OperationType.Sell;

            var openedPosition = _settings.StockMarketExecutionProvider.GetOpenedPosition(ticker);
            //If position is opened
            if (openedPosition != null)
            {
                //And we decide to close it
                if ((openedPosition.Amount > 0 && operation == OperationType.Sell) ||
                    (openedPosition.Amount < 0 && operation == OperationType.Buy))
                {
                    WriteOutput($"Closing position on {ticker}.", BotOutputCategory.BotOperation);
                    _settings.StockMarketExecutionProvider.AddOrderToQueue(ticker, _settings.Currency.ToString(),
                        operation, Math.Abs(openedPosition.Amount), (order) =>
                         {
                             WriteOutput($"Position closed.", BotOutputCategory.BotOperation);
                         });
                }
            }
            else//If we are opening position
            {
                if (operation == OperationType.Sell && !_settings.IsShortAvailable)
                    return;
                WriteOutput($"Opening position on {ticker}.", BotOutputCategory.BotOperation);
                var amount = (int)Math.Truncate(_onPositionBalance / update.Candles.Last().Close);
                amount *= operation == OperationType.Sell ? -1 : 1;
                _settings.StockMarketExecutionProvider.AddOrderToQueue(ticker, _settings.Currency.ToString(),
                        operation, amount, (order) =>
                        {
                            WriteOutput($"Position opened.", BotOutputCategory.BotOperation);
                        });
            }
        }

        protected override void TimeUpdateOperation(DateTime currentTime)
        {
            _currentDateTime = currentTime;
            if (currentTime - _lastRecalculationOperationDt >= _settings.RecalculationPeriod)
            {
                lock(_statusUpdateLocker)
                    Status = ExecutionStatus.ExportStockMarketData;
                ImportFullStockMarketHistory(currentTime);
                RecalculateTradingAnalyzers(currentTime);
                LoadAnalyzersCalculationResults();
                _lastRecalculationOperationDt = currentTime;
            }
        }
        protected override void Initialize()
        {
            lock(_statusUpdateLocker)
                Status = ExecutionStatus.Initialization;
            _analyzerHistory = new AnalyzerHistoryRepository(_settings.StockHistoryDatabasePath);
            //LoadAnalyzersCalculationResults();
        }

        private void ImportFullStockMarketHistory(DateTime currentDate)
        {
            lock (_statusUpdateLocker)
                Status = ExecutionStatus.ExportStockMarketData;
            WriteOutput($"Starting Full Stock Market data importing...", BotOutputCategory.BotOperation);
            var processor = new StockMarketExporter();
            processor.SubscribeOnOutputMessage((message) =>
            {
                WriteOutput($"{message}", BotOutputCategory.Importing);
            });
            processor.ExportToLocalDatabaseAsync(currentDate.AddDays(-7), currentDate);
            WriteOutput("Stock market data was downloaded to history database", BotOutputCategory.BotOperation);
        }

        private void RecalculateTradingAnalyzers(DateTime currentDate)
        {
            lock (_statusUpdateLocker)
                Status = ExecutionStatus.AnalyzingIndicators;
            WriteOutput($"Recalculating Analyzers...", BotOutputCategory.BotOperation);
            var processor = new AnalyzersProcessor();
            processor.SubscribeOnOutputMessage((message) =>
            {
                WriteOutput(message, BotOutputCategory.Analyzing);
            });
            processor.StartFullProcessing(currentDate.AddDays(-7), currentDate);
            WriteOutput($"Recalculation completed.", BotOutputCategory.BotOperation);
        }

        private void LoadAnalyzersCalculationResults()
        {
            if(_tickerAnalyzers != null && _tickerAnalyzers.Count > 0)
            {
                foreach (var tickerAnalyzer in _tickerAnalyzers)
                {
                    StopTradingOnTicker(tickerAnalyzer.Key);
                }
            }

            _tickerAnalyzers = new Dictionary<string, IMarketAnalyzer>();
            _analyzerHistory.LoadSimulationResults()
                .Take(_settings.InstrumentsAmountToTrade).ToList()
                .ForEach(x =>
                {
                    var analyzer = _analyzersHub.GetInstanceByType(x.AnalyzerType);
                    if (!_tickerAnalyzers.ContainsKey(x.InstrumentTicker))
                    {
                        _tickerAnalyzers.Add(x.InstrumentTicker, analyzer);
                    }
                });

            var totalBalance = _settings.StockMarketExecutionProvider
                .GetFreeBalance(Currency.Rub.ToString());
            if(_tickerAnalyzers.Count > 0)
                _onPositionBalance = Math.Truncate(totalBalance / _tickerAnalyzers.Count);
            foreach (var tickerAnal in _tickerAnalyzers)
            {
                WriteOutput($"Start operating {tickerAnal.Key} with {tickerAnal.Value.GetType()} analyzer.", BotOutputCategory.BotOperation);
                _settings.CandlesProvider.SubscribeOnUpdate(tickerAnal.Key, Execute);
            }
            lock(_statusUpdateLocker)
                Status = ExecutionStatus.Trade;
        }

        private void StopTradingOnTicker(string ticker)
        {
            _settings.CandlesProvider.Unsubscribe(ticker, Execute);
            var openedPosition = _settings.StockMarketExecutionProvider.GetOpenedPosition(ticker);
            WriteOutput($"Stop trading with {ticker}.", BotOutputCategory.BotOperation);
            if (openedPosition != null)
            {
                WriteOutput($"Closing position on {ticker}", BotOutputCategory.BotOperation);
                _settings.StockMarketExecutionProvider.AddOrderToQueue(openedPosition.Ticker, _settings.Currency.ToString(),
                        openedPosition.Amount > 0 ? OperationType.Sell : OperationType.Buy,
                        Math.Abs(openedPosition.Amount), (order) =>
                        {
                            WriteOutput($"Position closed.", BotOutputCategory.BotOperation);
                        });
            }
        }
    }
}
