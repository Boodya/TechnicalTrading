using System;

namespace TinkoffConnector.Model
{
    public class CandleModel
    {
        public decimal Open;
        public decimal Close;
        public decimal High;
        public decimal Low;
        public decimal AveragePrice;
        public decimal Volume;
        public DateTime Time;
        public string Interval;
        public string Figi;

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
