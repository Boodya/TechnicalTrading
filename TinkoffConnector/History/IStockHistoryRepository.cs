using System;
using System.Collections.Generic;
using TinkoffConnector.Model;

namespace TinkoffConnector.History
{
    public interface IStockHistoryRepository
    {
        IEnumerable<CandleModel> GetStockHistory(string currency, string ticker, DateTime from, DateTime to, string interval);
        DateTime GetLastNoteTimeForStock(string currency, string ticker);
        string GenerateStockHistoryCsv(List<CandleModel> candles, bool generateHeader);
        bool CheckHistoryExist(string currency, string ticker);
        string SaveHistory(string currency, string stockTitle, List<CandleModel> candles,
            string specialPath = "", bool isOverwrite = false);
        IEnumerable<CandleModel> GetStockHistory(string currency, string ticker, DateTime? from = null, DateTime? to = null, CandleIntervals? intervals = null);
    }
}
