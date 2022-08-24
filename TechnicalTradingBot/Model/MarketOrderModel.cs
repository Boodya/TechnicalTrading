using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot.Model
{
    public class MarketOrderModel
    {
        public OrderExecutionStatus Status { get; set; }
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public OperationType OperationType { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }

        public MarketOrderModel()
        {
            Status = OrderExecutionStatus.Initialized;
            Id = Guid.NewGuid();
        }
    }
}
