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
    public class TinkoffMarketCandlesProvider : ICandleProvider
    {
        protected IStockHistoryRepository _history;
        protected Dictionary<string, List<Action<CandlesUpdateModel>>> _subscribers;
        protected readonly object _locker = new object();
        protected Dictionary<string, List<TickerData>> _tickersBuff;
        protected TimeSpan _tickersBufferTailPeriod;
        protected TimeSpan _candlesInterval;
        protected DateTime _lastRequestOperationDate = DateTime.MinValue;
        protected string _currency;

        public TinkoffMarketCandlesProvider(IExecutionOrchestrator dtProvider, string currency)
        {
            _history = new LiteDBHistoryRepository(
                StartupSettings.AppSettings.StockDatabasePath);
            _subscribers = new Dictionary<string, List<Action<CandlesUpdateModel>>>();
            _tickersBuff = new Dictionary<string, List<TickerData>>();
            //Fetch candles every 5 minutes
            _candlesInterval = new TimeSpan(0, 5, 0);
            _tickersBufferTailPeriod = new TimeSpan(-30, 0, 0, 0);
            _currency = currency;
            dtProvider.SubscribeOnUpdate(OnCurrentDateTimeUpdate);
        }

        protected virtual List<TickerData> FetchCandles(string ticker, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public void OnCurrentDateTimeUpdate(DateTime dt)
        {
            if (dt - _lastRequestOperationDate > _candlesInterval)
            {
                lock (_locker)
                {
                    ProcessSubscribers(
                        ProcessTickerBuffer(dt));
                }
                _lastRequestOperationDate = dt;
            }
        }
        public decimal? GetLastPriceFromTicker(string ticker)
        {
            if (_tickersBuff.ContainsKey(ticker))
            {
                var lastCandle = _tickersBuff[ticker].LastOrDefault();
                if (lastCandle != null)
                    return lastCandle.Close;
            }
            return GetLastInstrumentPrice(ticker);
        } 

        protected virtual decimal? GetLastInstrumentPrice(string ticker)
        {
            throw new NotImplementedException();
        }

        protected void ProcessSubscribers(List<string> tickers)
        {
            foreach (var ticker in tickers)
            {
                if (_subscribers.ContainsKey(ticker))
                {
                    _subscribers[ticker].ForEach(s =>
                    {
                        s.Invoke(new CandlesUpdateModel()
                        {
                            Ticker = ticker,
                            Candles = _tickersBuff[ticker]
                        });
                    });
                }
            }
        }

        protected List<string> ProcessTickerBuffer(DateTime dt)
        {
            var changedTickers = new List<string>();
            foreach (var tickerCandles in _tickersBuff)
            {
                DateTime dateBound = dt.Add(_tickersBufferTailPeriod);
                DateTime dateFrom = tickerCandles.Value.Count == 0 ?
                    dateBound :
                    tickerCandles.Value.Last().Date;

                var data = FetchCandles(tickerCandles.Key, dateFrom, dt);
                if (data != null && data.Count > 0)
                {
                    tickerCandles.Value.AddRange(data);
                    var removedCount = tickerCandles.Value.RemoveAll(c => c.Date < dateBound);
                    changedTickers.Add(tickerCandles.Key);
                }
            }
            return changedTickers;
        }

        public void SubscribeOnUpdate(string ticker, Action<CandlesUpdateModel> onUpdate)
        {
            lock (_locker)
            {
                if (_subscribers.ContainsKey(ticker))
                    _subscribers[ticker].Add(onUpdate);
                else _subscribers.Add(ticker, new List<Action<CandlesUpdateModel>>() { onUpdate });

                if (!_tickersBuff.ContainsKey(ticker))
                    _tickersBuff.Add(ticker, new List<TickerData>());
            }
        }

        public void Unsubscribe(string ticker, Action<CandlesUpdateModel> execution)
        {
            lock (_locker)
            {
                if (_subscribers.ContainsKey(ticker))
                    _subscribers[ticker].Remove(execution);
                if (_tickersBuff.ContainsKey(ticker))
                    _tickersBuff.Remove(ticker);
            }
        }
    }
}
