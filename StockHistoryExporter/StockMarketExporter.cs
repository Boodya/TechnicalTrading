using System;
using System.Collections.Generic;
using TinkoffConnector.Connectors;
using TinkoffConnector.Factories;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffConnector;
using TinkoffConnector.History;
using System.Threading;
using TinkoffConnector.Model;
using System.Threading.Tasks;

namespace StockHistoryExporter
{
    public class StockMarketExporter
    {
        private int totalUpdatedHistoryCounter = 0;
        private SandboxConnector _connector;
        private StockHistoryRepository _history;
        private List<Currency> _availableCurrencies = new List<Currency>()
        {
            Currency.Rub,
            //Currency.Eur,
            //Currency.Usd
        };
        private List<Action<string>> _outputSubscribers = new List<Action<string>>();

        public StockMarketExporter()
        {

        }

        public void ExportToLocalDatabaseAsync(DateTime dateFrom, DateTime dateTo)
        {
            _history = new StockHistoryRepository(
                StartupSettings.AppSettings.StockDatabasePath);
            var downloadTask = DownloadHistoricalDataForStocks(dateFrom, dateTo);
            downloadTask.Wait();
            WriteOutput($"Execution completed. Total updated stocks history files is {totalUpdatedHistoryCounter}.");
        }

        public void SubscribeOnOutputMessage(Action<string> subscription)
        {
            _outputSubscribers.Add(subscription);
        }
        private void WriteOutput(string message)
        {
            foreach (var subscriber in _outputSubscribers)
            {
                subscriber.Invoke(message);
            }
        }

        private async Task DownloadHistoricalDataForStocks(DateTime dateStart, DateTime dateStop)
        {
            _connector = await ConnectionFactory.GetConnector<SandboxConnector>();
            foreach (var currency in _availableCurrencies)
            {
                foreach (var stock in _connector.Instruments.GetInstrumentsByCurrency(currency))
                {
                    if (stock.Ticker.Contains("_old") ||
                        StartupSettings.AppSettings
                        .TickersBlackList.Contains(stock.Ticker))
                        continue;
                    var stockCandles = await LoadInstrumentCandlesToHistory(stock, currency, dateStart, dateStop);
                    if (stockCandles.Count == 0)
                        WriteOutput($"History data for {stock.Ticker} was skipped.");
                    else
                    {
                        var savedFilePath = _history.SaveHistory(currency.ToString(), stock.Ticker, stockCandles);
                        WriteOutput($"History data for {stock.Ticker} has been saved to {savedFilePath}. [{DateTime.Now}]");
                        totalUpdatedHistoryCounter++;
                    }
                }
            }
            WriteOutput("Exporting data was succeeded.");
        }

        private async Task<List<CandleModel>> LoadInstrumentCandlesToHistory(MarketInstrument stock, Currency currency,
            DateTime dateStart, DateTime dateStop)
        {
            var stockCandles = new List<CandleModel>();
            var dateFrom = new DateTime(dateStart.Year, dateStart.Month,
                        dateStart.Day, dateStart.Hour, dateStart.Minute, dateStart.Second);
            var dateEnd = new DateTime(dateStop.Year, dateStop.Month,
                dateStop.Day, dateStop.Hour, dateStop.Minute, dateStop.Second);

            var lastUpdateDate = _history.GetLastNoteTimeForStock(currency.ToString(), stock.Ticker);
            if (lastUpdateDate != DateTime.MinValue)
            {
                if ((dateEnd - lastUpdateDate).TotalHours < 1)
                {
                    return stockCandles;
                }
                dateFrom = lastUpdateDate;
            }
            WriteOutput($"Retreiving history data for {stock.Ticker}({currency}) from {dateFrom.ToString()}" +
                $" to {dateEnd.ToString()}... [{DateTime.Now.ToString()}]");
            while (dateFrom < dateEnd)
            {
                try
                {
                    var nextDate = dateFrom.AddDays(1);
                    var candles = await _connector.Candles.GetInstrumentCandles(
                    stock, dateFrom, nextDate, CandleInterval.Minute, false);
                    if (candles.Count > 0)
                        stockCandles.AddRange(candles);
                    dateFrom = nextDate;
                }
                catch (Exception ex)
                {
                    TinkoffConnector.Logging.Logger.WriteErrorLog(ex);
                    Thread.Sleep(new TimeSpan(0, 0,
                        StartupSettings.AppSettings.TimeoutRetryTimeInSeconds));
                }
            }
            return stockCandles;
        }
    }
}
