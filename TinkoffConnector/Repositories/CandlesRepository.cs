using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.History;
using TinkoffConnector.Model;
/*  Available candle intervals and limitations about amount of days:
            - 1min [1 minute, 1 day]
            - 2min [2 minutes, 1 day]
            - 3min [3 minutes, 1 day]
            - 5min [5 minutes, 1 day]
            - 10min [10 minutes, 1 day]
            - 15min [15 minutes, 1 day]
            - 30min [30 minutes, 1 day]
            - hour [1 hour, 7 days]
            - day [1 day, 1 year]
            - week [7 days, 2 years]
            - month [1 month, 10 years]
        */
namespace TinkoffConnector.Repositories
{
    public class CandlesRepository : ICandlesRepository
    {
        private IContext _context;
        private string _accountId;
        private StockHistoryRepository _historyData;

        public CandlesRepository(IContext context, string accountId)
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
            _historyData = new StockHistoryRepository(
                StartupSettings.AppSettings.StockDatabasePath);
        }
        
        public async Task<List<CandleModel>> GetInstrumentCandles(MarketInstrument instrument, DateTime from, DateTime to, CandleInterval? interval = null, bool isUseHistory = true)
        {
            var actualInterval = interval.HasValue ? interval.Value : GetIntervalForCandles(from, to);
            if (isUseHistory)
            {
                var updateDate = _historyData.GetLastNoteTimeForStock(instrument.Currency.ToString(), instrument.Ticker);
                if (updateDate != null && from < updateDate && to < updateDate)
                {
                    return _historyData.GetStockHistory(instrument.Currency.ToString(),
                        instrument.Ticker, from, to,
                        actualInterval.ToString()).ToList();
                }
            }
            var obj = await _context.MarketCandlesAsync(instrument.Figi, from, to, actualInterval);
            return Mapper.MapCandles(obj.Candles);
        }

        public async Task<decimal?> GetCurrentInstrumentPrice(MarketInstrument instrument, DateTime? dateFor = null)
        {
            dateFor = dateFor == null ? DateTimeHelpers.CurrentDate : dateFor;
            var dateTo = dateFor.Value.GetLastBusinessDateTimeForInstrument(instrument);
            var dateFrom = dateTo.AddDays(-1);
            var candles = await _context.MarketCandlesAsync(instrument.Figi, dateFrom, dateTo, CandleInterval.Minute);
            if (candles.Candles.Count == 0)
                return null;
            var lastCandle = candles.Candles.OrderByDescending(x => x.Time).ToList()[0];
            return lastCandle.Close;
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
    }
}
