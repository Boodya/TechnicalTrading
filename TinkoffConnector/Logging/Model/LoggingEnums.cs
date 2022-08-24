using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Logging.Model
{
    internal enum OperationType
    {
        Info = 0,
        Buy = 1,
        Sell = 2,
        CreateOrder = 3,
        CancelOrder = 4,
        MarketBuy = 5,
        MarketSell = 6,
    }

    internal enum LogType
    {
        Info = 0,
        Trading = 1,
        Processing = 2,
        Error = 3,
        TestsExecution = 4,
        TestsResult = 5
    }
}
