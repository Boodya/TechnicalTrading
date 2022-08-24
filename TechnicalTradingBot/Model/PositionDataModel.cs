using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot.Model
{
    public class PositionDataModel
    {
        public int Amount { get; set; }
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public decimal OpeningPrice { get; set; }
        public decimal ClosedPrice { get; set; }
        public DateTime PositionOpenedDate { get; set; }
        public DateTime PositionClosedDate { get; set; }
        public decimal Profit
        {
            get
            {
                var result = (ClosedPrice - OpeningPrice) / OpeningPrice;
                return Amount > 0 ? result * 100 : result * -100;
            }
        }
    }
}
