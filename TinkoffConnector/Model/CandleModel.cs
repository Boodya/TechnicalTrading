using System;

namespace TinkoffConnector.Model
{
    public class CandleModel
    {
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Volume { get; set; }
        public DateTime Time { get; set; }
        public string Interval { get; set; }
        public string Figi { get; set; }

        internal static CandleModel Clone(CandleModel c)
        {
            return new CandleModel()
            {
                Open = c.Open,
                Close = c.Close,
                High = c.High,
                Low = c.Low,
                AveragePrice = c.AveragePrice,
                Volume = c.Volume,
                Time = c.Time,
                Interval = c.Interval,
                Figi = c.Figi
            };
        }
    }
}
