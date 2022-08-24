using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffConnector.Extensions;
using TinkoffConnector.Model;

namespace TinkoffConnector
{
    internal static class Mapper
    {
        internal static List<CandleModel> MapCandles(List<CandlePayload> inpCandles)
        {
            var mappedCandles = new List<CandleModel>();
            foreach(var inpCandle in inpCandles)
            {
                mappedCandles.Add(MapCandle(inpCandle));
            }
            return mappedCandles;
        }
        internal static CandleModel MapCandle(CandlePayload inpCandle)
        {
            return new CandleModel()
            {
                Open = inpCandle.Open,
                Close = inpCandle.Close,
                High = inpCandle.High,
                Low = inpCandle.Low,
                AveragePrice = inpCandle.GetAveragePrice(),
                Volume = inpCandle.Volume,
                Time = inpCandle.Time,
                Interval = inpCandle.Interval.ToString(),
                Figi = inpCandle.Figi
            };
        }
    }
}
