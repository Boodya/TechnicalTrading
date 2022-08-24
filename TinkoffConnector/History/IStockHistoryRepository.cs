using System;
using System.Collections.Generic;
using TinkoffConnector.Model;

namespace TinkoffConnector.History
{
    public interface IStockHistoryRepository
    {
        Dictionary<string, List<string>> Tickers { get; }
        DateTime GetLastNoteTimeForStock(string currency, string ticker);
        bool CheckHistoryExist(string currency, string ticker);
        string SaveHistory(string currency, string stockTitle, List<CandleModel> candles,
            string specialPath = "", bool isOverwrite = false);
        IEnumerable<CandleModel> GetStockHistory(string currency, string ticker, DateTime? from = null, DateTime? to = null, CandleIntervals? intervals = null);
    }
}
