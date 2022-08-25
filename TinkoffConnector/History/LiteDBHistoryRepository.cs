using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinkoffConnector.Model;

namespace TinkoffConnector.History
{
    public class LiteDBHistoryRepository : IStockHistoryRepository
    {
        private readonly string[] _nonTickersTables = new string[]
        {
            "TradingSimulationResults"
        };
        public Dictionary<string, List<string>> Tickers { get; private set; }
        private string _dbPath;
        private Dictionary<string, CachedCandlesModel> _candlesCache { get; set; }
        private readonly int _cacheSize = 100000;

        public LiteDBHistoryRepository(string dbPath)
        {
            _dbPath = dbPath;
            _candlesCache = new Dictionary<string, CachedCandlesModel>();
            InitAvailableTickers();
        }

        private void InitAvailableTickers()
        {
            Tickers = new Dictionary<string, List<string>>();
            using (var db = new LiteDatabase(_dbPath))
            {
                var allTables = db.GetCollectionNames();
                foreach (var table in allTables)
                {
                    if (_nonTickersTables.Contains(table)) 
                        continue;
                    var nameParts = table.Split('_');
                    var currency = nameParts[0];
                    var ticker = nameParts[1];
                    var tickerExt = nameParts.Length > 2 ? nameParts[2] : "";
                    ticker += tickerExt;
                    if (Tickers.ContainsKey(currency))
                        Tickers[currency].Add(ticker);
                    else Tickers.Add(currency, new List<string>() { ticker });
                }
            }
        }

        public bool CheckHistoryExist(string currency, string ticker)
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                return db.CollectionExists(GetDbName(currency, ticker));
            }
        }

        public DateTime GetLastNoteTimeForStock(string currency, string ticker)
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                if (db.CollectionExists(GetDbName(currency, ticker)))
                {
                    var coll = db.GetCollection<CandleModel>(GetDbName(currency, ticker));
                    if(coll.Count() > 0)
                        return coll.Max(c => c.Time);
                }
            }
            return DateTime.MinValue;
        }

        public IEnumerable<CandleModel> GetStockHistory(string currency, string ticker,
            DateTime? from = null, DateTime? to = null, CandleIntervals? interval = null)
        {
            IEnumerable<CandleModel> result = new List<CandleModel>(); 

            if(!CheckHistoryExist(currency, ticker))
                return result;
            var cachedCandles = GetCandlesFromCache(ticker, from, to);
            if(cachedCandles.Count == 0)
            {
                using (var db = new LiteDatabase(_dbPath))
                {
                    var col = db.GetCollection<CandleModel>(GetDbName(currency, ticker));
                    if (from.HasValue && to.HasValue)
                        result = col.Find(c => c.Time >= from && c.Time <= to);
                    else if (from.HasValue)
                        result = col.Find(c => c.Time >= from);
                    else if (to.HasValue)
                        result = col.Find(c => c.Time <= to);
                    else result = col.FindAll();
                    var requestedCandles = new List<CandleModel>(result);
                    if(requestedCandles.Count > 0)
                        SaveCandlesToCache(ticker, requestedCandles
                            .OrderBy(c => c.Time).ToList(), from, to);
                    result = requestedCandles;
                }
                if (interval.HasValue)
                    return Helpers.ProcessInterval(result, interval.Value);
                return result;
            }
            if (interval.HasValue)
                return Helpers.ProcessInterval(cachedCandles, interval.Value);
            return cachedCandles;
        }

        private List<CandleModel> GetCandlesFromCache(string ticker,
            DateTime? from = null, DateTime? to=null)
        {
            if (!_candlesCache.ContainsKey(ticker) ||
                _candlesCache[ticker].Candles.Count == 0)
                return new List<CandleModel>();

            if((from != null && _candlesCache[ticker].RequestedFromDate != from) ||
                (to != null && _candlesCache[ticker].RequestedToDate != to))
                return new List<CandleModel>();

            return _candlesCache[ticker].Candles;
        }

        private void SaveCandlesToCache(string ticker, List<CandleModel> items,
            DateTime? from, DateTime? to)
        {
            if (from == null)
                from = items.First().Time;
            if (to == null)
                to = items.Last().Time;

            if (!_candlesCache.ContainsKey(ticker))
                _candlesCache.Add(ticker, new CachedCandlesModel()
                {
                    RequestedFromDate = from.Value,
                    RequestedToDate = to.Value,
                    Candles = items
                });
        }

        public string SaveHistory(string currency, string stockTitle, List<CandleModel> candles,
            string specialPath = "", bool isOverwrite = false)
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                var col = db.GetCollection<CandleModel>(
                    GetDbName(currency, stockTitle));
                col.EnsureIndex(x => x.Time, true);

                candles.ForEach(candle =>
                {
                    var existingItem = col
                        .Find(dbCandle => dbCandle.Time == candle.Time)
                        .FirstOrDefault();
                    if (existingItem != null)
                    {
                        candle.Id = existingItem.Id;
                        col.Update(candle);
                    }
                    else col.Insert(candle);  
                });
            }
            return _dbPath;
        }

        private string GetDbName(string currency, string ticker)
        {
            return $"{currency}_{ticker.Replace('-', '_')}";
        }
    }
}
