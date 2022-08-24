using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Model
{
    public class SettingsModel
    {
        public string AuthToken { get; set; }
        public string SandBoxAuthToken { get; set; }
        public string StockDatabasePath { get; set; }
        public bool IsWriteErrorLogs { get; set; }
        public int TimeoutRetryTimeInSeconds { get; set; }
        public List<StockMarketHoliday> StockMarketHolidays { get; set; }
        public List<string> TickersBlackList { get; set; }
    }

    public class StockMarketHoliday
    {
        public string StockMarket { get; set; }
        public DateTime DateOff { get; set; }
    }
}
