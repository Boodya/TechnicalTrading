using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinkoffConnector.History;
using TinkoffConnector.Model;

namespace TinkoffConnector
{
    public static class Helpers
    {
        public static IEnumerable<CandleModel> ProcessInterval(IEnumerable<CandleModel> candles, CandleIntervals interval)
        {
            switch (interval)
            {
                case CandleIntervals.Minute:
                    return candles;
                case CandleIntervals.TwoMinutes:
                    return ConcatCandlesByMinutes(candles, 2, interval);
                case CandleIntervals.ThreeMinutes:
                    return ConcatCandlesByMinutes(candles, 3, interval);
                case CandleIntervals.FiveMinutes:
                    return ConcatCandlesByMinutes(candles, 5, interval);
                case CandleIntervals.TenMinutes:
                    return ConcatCandlesByMinutes(candles, 10, interval);
                case CandleIntervals.QuarterHour:
                    return ConcatCandlesByMinutes(candles, 20, interval);
                case CandleIntervals.HalfHour:
                    return ConcatCandlesByMinutes(candles, 30, interval);
                case CandleIntervals.Hour:
                    return ConcatCandlesByMinutes(candles, 60, interval);
                case CandleIntervals.Day:
                    return ConcatCandlesByMinutes(candles, 0, interval);
                case CandleIntervals.Week:
                    return ConcatCandlesByMinutes(candles, 0, interval);
                case CandleIntervals.Month:
                    return ConcatCandlesByMinutes(candles, 0, interval);
            }
            throw new Exception("Unfamiliar type of candle interval");
        }

        public static IEnumerable<CandleModel> ConcatCandlesByMinutes(IEnumerable<CandleModel> inp_candles, int minutesAmount, CandleIntervals interval)
        {
            var processedCandles = new List<CandleModel>();
            var candles = inp_candles.ToArray();
            for (var i = 0; i < candles.Length;)
            {
                var firstCandle = candles[i];
                var resultCandle = CandleModel.Clone(firstCandle);
                var endCandleTime = firstCandle.Time.AddMinutes(minutesAmount);
                if (interval == CandleIntervals.Day)
                    endCandleTime = firstCandle.Time.AddDays(1);
                else if (interval == CandleIntervals.Week)
                    endCandleTime = firstCandle.Time.AddDays(7);
                else if (interval == CandleIntervals.Month)
                    endCandleTime = firstCandle.Time.AddMonths(1);
                var timeCounter = resultCandle.Time;
                while (timeCounter < endCandleTime)
                {
                    i++;
                    if (i >= candles.Length)
                        break;
                    var candle = candles[i];
                    {
                        resultCandle.Close = candle.Close;
                        resultCandle.High = resultCandle.High > candle.High ? resultCandle.High : candle.High;
                        resultCandle.Low = resultCandle.Low < candle.Low ? resultCandle.Low : candle.Low;
                        resultCandle.Volume += candle.Volume;
                    }
                    i++;
                    if (i >= candles.Length)
                        break;
                    timeCounter = candles[i].Time;
                }
                resultCandle.Interval = interval.ToString();
                resultCandle.AveragePrice = (resultCandle.High + resultCandle.Low) / 2;
                processedCandles.Add(resultCandle);
            }
            return processedCandles;
        }
    }
}
