using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Models;
using StockMarketAnalyzer.Analyzers;
using StockMarketAnalyzer.History;
using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffConnector;
using TinkoffConnector.Connectors;
using TinkoffConnector.Factories;
using TinkoffConnector.History;
using static OoplesFinance.StockIndicators.Calculations;

namespace StockMarketAnalyzer
{
    public class AnalyzersProcessor
    {
        private  IStockHistoryRepository _history;
        private  AnalyzerHistoryRepository _analyzingHistory;
        private  SandboxConnector _connector;
        private  readonly Currency _testingCurrency = Currency.Rub;
        private  int _positionAmount = 1;
        private  bool _isShortAvailable = false;
        private  readonly int _startSimulationBalance = 10000;
        private  bool _isCachedMode = true;
        private  decimal _profitLimit = 5;//Limit to be listed in profit
        private  double _conversionLimit = 70;//Limit to lined in trades result conversion
        private List<Action<string>> _outputSubscribers = new List<Action<string>>();
        private AnalyzersHub _analyzersHub;

        public void StartFullProcessing(DateTime dateFrom, DateTime dateTo)
        {
            PrepareDataToSimulation(_isCachedMode, dateFrom, dateTo);
            _analyzersHub = new AnalyzersHub();
            var totalAnalyzers = _analyzersHub.AllAnalyzers;
            var index = 1;
            WriteOutput("Simulation started.");
            List<TradingSimulationResults> allResults = new List<TradingSimulationResults>();
            foreach (var analyzer in _analyzersHub.AllAnalyzers)
            {
                var res = CalculateAnalyzerOnHistory(analyzer, dateFrom, dateTo);
                if (res != null && res.Count > 0)
                    allResults.AddRange(res);
                WriteOutput($"Calculations complete for {analyzer.GetType().ToString()}. ({index}/{totalAnalyzers.Count})");
                index++;
            }
            var validResults = allResults.Where(r => r.TotalSuccessTradesConversionPercent > _conversionLimit)
                .Where(r => r.TradingResultInPercent > _profitLimit)
                .OrderByDescending(r => r.TradingResultInPercent).ToList();

            List<TradingSimulationResults> results = new List<TradingSimulationResults>();
            for(var i = 0; i < validResults.Count; i++)
            {
                var tick = validResults[i].InstrumentTicker;
                if (results.Where(r => r.InstrumentTicker == tick).FirstOrDefault() == null)
                {
                    results.Add(validResults[i]);
                }
                else continue;
            }
            _analyzingHistory.SaveAnalyzerResults(_testingCurrency.ToString(), results);
            SaveTradingResultsFromSimulation(results);
        }

        private void SaveTradingResultsFromSimulation(List<TradingSimulationResults> simulationResults)
        {
            var tradingResults = simulationResults.Where(r => r.LastDecision == Signal.Buy.ToString()
                || r.LastDecision == Signal.StrongBuy.ToString())
                .OrderByDescending(r => r.TradingResultInPercent)
                .Take(10).ToList();
            var tradingFilePath = Path.Combine(StartupSettings
                .AppSettings.StockDatabasePath, "Trading", "TradingIndicators.csv");
            _analyzingHistory.SaveAnalyzerResults(_testingCurrency.ToString(), tradingResults, tradingFilePath);
        }

        public void RecalculateTradingIndicators(DateTime dateTo)
        {
            var history = new LiteDBHistoryRepository(StartupSettings.AppSettings.StockDatabasePath);
            var analyzingHistory = new AnalyzerHistoryRepository(StartupSettings.AppSettings.StockDatabasePath);
            var analyzersHub = new AnalyzersHub();
            var tradingFilePath = Path.Combine(StartupSettings
                .AppSettings.StockDatabasePath, "Trading", "TradingIndicators.csv");
            var previousResults = analyzingHistory.LoadSimulationResults(_testingCurrency.ToString(), tradingFilePath);
            
            var dtNow = dateTo;
            var startDt = dtNow.AddMonths(-1);
            List<TradingSimulationResults> results = new List<TradingSimulationResults>();
            previousResults.ForEach(r =>
            {
                var result = SimulateHistoryTrading(r.InstrumentTicker,
                    analyzersHub.GetInstanceByType(r.AnalyzerType),
                    history, startDt, dtNow);
                results.Add(result);
            });
            analyzingHistory.SaveAnalyzerResults(_testingCurrency.ToString(), results, tradingFilePath);
        }

        public void SubscribeOnOutputMessage(Action<string> subscription)
        {
            _outputSubscribers.Add(subscription);
        }

        private void WriteOutput(string message)
        {
            foreach(var subscriber in _outputSubscribers)
            {
                subscriber.Invoke(message);
            }
        }

        private void PrepareDataToSimulation(bool isCachedMode, DateTime dateFrom, DateTime dateTo)
        {
            WriteOutput("Preparing all data to simulation...");

            _history = new LiteDBHistoryRepository(StartupSettings.AppSettings.StockDatabasePath);
            _analyzingHistory = new AnalyzerHistoryRepository(
                StartupSettings.AppSettings.StockDatabasePath);
        }

        private List<TradingSimulationResults> CalculateAnalyzerOnHistory(IMarketAnalyzer analyzer, DateTime dateFrom, DateTime dateTo)
        {
            var instruments = _history.Tickers[_testingCurrency.ToString()];
            List<TradingSimulationResults> tradingResults = new List<TradingSimulationResults>();
            foreach (var instrument in instruments)
            {
                var result = SimulateHistoryTrading(instrument, analyzer, _history,
                    dateFrom, dateTo);
                if (result != null)
                    tradingResults.Add(result);
            }
            tradingResults = tradingResults.Where(r => r.TradingResultInPercent > 0).ToList();
            if (tradingResults != null && tradingResults.Count > 0)
            {
                var topResult = tradingResults.Where(r => r.TradingResultInPercent > 0).
                    OrderByDescending(r => r.TotalSuccessTradesConversionPercent).First();
                WriteOutput($"Top result is on {topResult.InstrumentTicker} with {topResult.TotalSuccessfulTrades} succesfull trades. " +
                    $"Total Trades {topResult.TotalTrades}. Profit {topResult.TradingResultInPercent}%");
            }
            return tradingResults;
        }

        private TradingSimulationResults SimulateHistoryTrading(string instrumentTicker, IMarketAnalyzer analyzer, IStockHistoryRepository candlesRepo,
            DateTime startDate, DateTime endDate)
        {

            var result = new TradingSimulationResults()
            {
                AnalyzerType = analyzer.GetType().ToString(),
                InstrumentTicker = instrumentTicker,
                TotalTrades = 0,
                TradingStartBalance = _startSimulationBalance,
                TradingResultBalance = _startSimulationBalance,
                TimeFrame = CandleInterval.Hour.ToString()
            };
            int positionMarker = 0;

            result.TradingSimulationStartDateTime = startDate;
            result.TradingSimulationEndDateTime = endDate;

            var history = Mapper.ToTickerData(candlesRepo.GetStockHistory(_testingCurrency.ToString(), instrumentTicker,
                    result.TradingSimulationStartDateTime, result.TradingSimulationEndDateTime,
                    result.TimeFrame.ToEnum<CandleIntervals>()));
            var decision = analyzer.Analyze(history);

            decimal openPositionPrice = 0;
            DateTime openPositionDateTime = DateTime.MinValue;
            var hoursInDeal = new List<double>();

            for (var i = 0; i < decision.Signals.Count; i++)
            {
                var signal = decision.Signals[i];
                AnalyzeDecision(signal, history[i], result, hoursInDeal,
                    positionMarker, out positionMarker,
                    openPositionPrice, out openPositionPrice,
                    openPositionDateTime, out openPositionDateTime);
            }

            if (positionMarker == 1)
            {
                AnalyzeDecision(Signal.StrongSell, history.Last(), result, hoursInDeal,
                    positionMarker, out positionMarker,
                    openPositionPrice, out openPositionPrice,
                    openPositionDateTime, out openPositionDateTime);
            }
            else if (positionMarker == -1)
            {
                AnalyzeDecision(Signal.StrongBuy, history.Last(), result, hoursInDeal,
                    positionMarker, out positionMarker,
                    openPositionPrice, out openPositionPrice,
                    openPositionDateTime, out openPositionDateTime);
            }
            result.LastDecision = decision.Decision.ToString();
            result.AverageHoursStayedInDeal = hoursInDeal.Sum() / hoursInDeal.Count;
            result.TotalSuccessTradesConversionPercent = (double)result.TotalSuccessfulTrades / (double)result.TotalTrades * 100;
            return result.TotalSuccessfulTrades == 0 ? null : result;
        }

        private void AnalyzeDecision(Signal decision, TickerData tickData, TradingSimulationResults results,
            List<double> hoursInDeal,
            int currentPosition, out int positionMarker,
            decimal openPositionPrice, out decimal openPositionPriceOut,
            DateTime openPositionDateTime, out DateTime openPositionDateTimeOut)
        {
            switch (decision)
            {
                case Signal.Buy:
                case Signal.StrongBuy:
                    {
                        if (currentPosition == 0)//Opening position
                        {
                            currentPosition = 1;
                            _positionAmount = (int)Math.Truncate(results.TradingResultBalance / tickData.Close);
                            openPositionPrice = tickData.Close * _positionAmount;
                            results.TradingResultBalance -= openPositionPrice;
                            openPositionDateTime = tickData.Date;
                        }
                        else if (currentPosition == -1 && _isShortAvailable)//Close Position
                        {
                            currentPosition = 0;
                            var price = tickData.Close * _positionAmount;
                            {
                                if (price < openPositionPrice)
                                    results.TotalSuccessfulTrades++;
                                openPositionPrice = 0;
                                hoursInDeal.Add((tickData.Date - openPositionDateTime).TotalHours);
                                openPositionDateTime = DateTime.MinValue;
                            }
                            results.TradingResultBalance -= price;
                            _positionAmount = 1;
                            results.TotalTrades++;
                        }
                    }
                    break;
                case Signal.Sell:
                case Signal.StrongSell:
                    {
                        if (currentPosition == 0 && _isShortAvailable)//Opening position
                        {
                            currentPosition = -1;
                            _positionAmount = (int)Math.Truncate(results.TradingResultBalance / tickData.Close);
                            openPositionPrice = tickData.Close * _positionAmount;
                            results.TradingResultBalance += openPositionPrice;
                            openPositionDateTime = tickData.Date;
                        }
                        else if (currentPosition == 1)//Close Position
                        {
                            currentPosition = 0;
                            var price = tickData.Close * _positionAmount;
                            {
                                if (price > openPositionPrice)
                                    results.TotalSuccessfulTrades++;
                                openPositionPrice = 0;
                                hoursInDeal.Add((tickData.Date - openPositionDateTime).TotalHours);
                                openPositionDateTime = DateTime.MinValue;
                            }
                            results.TradingResultBalance += price;
                            _positionAmount = 1;
                            results.TotalTrades++;
                        }
                    }
                    break;
                default: break;
            }
            positionMarker = currentPosition;
            openPositionPriceOut = openPositionPrice;
            openPositionDateTimeOut = openPositionDateTime;
        }
    }
}
