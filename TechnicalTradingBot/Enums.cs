using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot
{
    public enum ExecutionStatus
    {
        Initialization,
        ExportStockMarketData,
        AnalyzingIndicators,
        Trade
    }

    public enum OrderExecutionStatus
    {
        Initialized,
        Executing,
        Executed,
        Cancelled
    }

    public enum OperationType
    {
        Buy,
        Sell
    }

    public enum Currency
    {
        Rub,
        Eur,
        Usd
    }
}
