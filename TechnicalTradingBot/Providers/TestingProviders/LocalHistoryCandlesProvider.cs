using OoplesFinance.StockIndicators.Models;
using StockMarketAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using TechnicalTradingBot.Model;
using TinkoffConnector;
using TinkoffConnector.History;

namespace TechnicalTradingBot.Providers
{
    public class LocalHistoryCandlesProvider : TinkoffMarketCandlesProvider
    {
        public LocalHistoryCandlesProvider(IExecutionOrchestrator dtProvider, string currency)
            : base(dtProvider, currency)
        {

        }

        protected override List<TickerData> FetchCandles(string ticker, DateTime dateFrom, DateTime dateTo)
        {
            return Mapper.ToTickerData(_history.GetStockHistory(_currency, ticker,
                    dateFrom, dateTo,
                    CandleIntervals.FiveMinutes.ToString()))
                    .OrderBy(c => c.Date).ToList();
        }

        protected override decimal? GetLastInstrumentPrice(string ticker)
        {
            var lastCandle = _history.GetStockHistory(_currency, ticker,
                     _lastRequestOperationDate.AddDays(-10), _lastRequestOperationDate,
                     CandleIntervals.Day.ToString()).LastOrDefault();
            return lastCandle?.Close;
        }
    }
}
