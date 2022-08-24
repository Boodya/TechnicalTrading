using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using TinkoffConnector.Model;

namespace TinkoffConnector.History
{
    public class LiteDBHistoryRepository : IStockHistoryRepository
    {
        public Dictionary<string, List<string>> Tickers { get; private set; }
        private string _dbPath;

        public LiteDBHistoryRepository(string dbPath)
        {
            _dbPath = dbPath;
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

            using (var db = new LiteDatabase(_dbPath))
            {
                // Get customer collection
                var col = db.GetCollection<CandleModel>(GetDbName(currency, ticker));
                if (from.HasValue && to.HasValue)
                    result = col.Find(c => c.Time >= from && c.Time <= to);
                else if (from.HasValue)
                    result = col.Find(c => c.Time >= from);
                else if (to.HasValue)
                    result = col.Find(c => c.Time <= to);
                else result = col.FindAll();

                if (interval.HasValue)
                    return Helpers.ProcessInterval(result, interval.Value);
            }
            return result;
        }

        public string SaveHistory(string currency, string stockTitle, List<CandleModel> candles,
            string specialPath = "", bool isOverwrite = false)
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                // Get customer collection
                var col = db.GetCollection<CandleModel>(
                    GetDbName(currency, stockTitle));
                // Create unique index in Time field
                col.EnsureIndex(x => x.Time, true);

                candles.ForEach(candle =>
                {
                    var isFound = false;
                    foreach(var existingCandle in 
                        col.Find(dbCandle => dbCandle.Time == candle.Time))
                    {
                        isFound = true;
                        break;
                    }
                    if (!isFound)
                        col.Insert(candle);   
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
