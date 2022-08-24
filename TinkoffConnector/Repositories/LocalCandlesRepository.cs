using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.History;
using TinkoffConnector.Model;

namespace TinkoffConnector.Repositories
{
    public class LocalCandlesRepository : ICandlesRepository
    {
        private IContext _context;
        private string _accountId;
        private IStockHistoryRepository _historyData;
        private List<CandleModel> _cachedCandles;

        public LocalCandlesRepository(IContext context, string accountId)
        {
            Initialize(context, accountId);
        }

        public void Initialize(IContext context, string accountId)
        {
            if (context == null)
                throw new Exception("Unable to Initialize repo: Empty context.");
            if (accountId == null)
                throw new Exception("Unable to Initialize repo: Empty account ID.");
            this._context = context;
            this._accountId = accountId;
            _historyData = new LiteDBHistoryRepository(
                StartupSettings.AppSettings.StockDatabasePath);
        }

        public async Task<List<CandleModel>> GetInstrumentCandles(MarketInstrument instrument, DateTime from, DateTime to, CandleInterval? interval = null, bool isUseHistory = true)
        {
            var actualInterval = interval.HasValue ? interval.Value : GetIntervalForCandles(from, to);
            var updateDate = _historyData.GetLastNoteTimeForStock(instrument.Currency.ToString(), instrument.Ticker);
            if (_cachedCandles != null)
            {
                var candlesFromCash = _cachedCandles.Where(x => x.Time >= from && x.Time <= to).ToList();
                if (candlesFromCash.Count != 0)
                    return candlesFromCash;
            }
            if (updateDate != null && from < updateDate && to < updateDate)
            {
                return _historyData.GetStockHistory(instrument.Currency.ToString(),
                    instrument.Ticker, from, to,
                    actualInterval.ToString().ToEnum<CandleIntervals>()).ToList();
            }
            return null;
        }

        public async void LoadCandlesToCache(MarketInstrument instrument, DateTime from, DateTime to, CandleInterval? interval = null, bool isUseHistory = true)
        {
            _cachedCandles = (await GetInstrumentCandles(instrument, from, to, interval, isUseHistory));
        }

        private CandleInterval GetIntervalForCandles(DateTime from, DateTime to)
        {
            var span = (to - from);
            if (span.TotalDays <= 1)
                return CandleInterval.FiveMinutes;
            else if (span.TotalDays <= 7)
                return CandleInterval.Hour;
            else if (span.TotalDays <= 365)
                return CandleInterval.Day;
            else if (span.TotalDays <= 730)
                return CandleInterval.Week;
            else if (span.TotalDays <= 3650)
                return CandleInterval.Month;
            return CandleInterval.Minute;
        }

        public async Task<decimal?> GetCurrentInstrumentPrice(MarketInstrument instrument, DateTime? dateFor = null)
        {
            dateFor = dateFor == null ? DateTimeHelpers.CurrentDate : dateFor;
            var dateTo = dateFor.Value.GetLastBusinessDateTimeForInstrument(instrument);
            var dateFrom = dateTo.AddDays(-1);
            var candles = await GetInstrumentCandles(instrument, dateFrom, dateTo, CandleInterval.Minute);
            if (candles == null || candles.Count == 0)
                return null;
            var lastCandle = candles.OrderByDescending(x => x.Time).ToList()[0];
            return lastCandle.Close;
        }
    }
}
