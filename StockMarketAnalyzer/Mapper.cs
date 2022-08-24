using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TinkoffConnector.Model;

namespace StockMarketAnalyzer
{
    public static class Mapper
    {
        public static List<TickerData> ToTickerData(IEnumerable<CandleModel> candles)
        {
            var result = new List<TickerData>();
            if(candles != null)
            {
                foreach (var candle in candles)
                {
                    result.Add(new TickerData()
                    {
                        Close = candle.Close,
                        Date = candle.Time,
                        High = candle.High,
                        Low = candle.Low,
                        Open = candle.Open,
                        Volume = candle.Volume,
                    });
                }
            }
            return result;
        }

        public static StockData GetIntersectionForStockDataSignals(List<StockData> data)
        {
            var signalsTotalCount = data[0].Count;
            var result = new StockData(data[0].OpenPrices, data[0].HighPrices, data[0].LowPrices, data[0].ClosePrices,
                data[0].Volumes, data[0].Dates, data[0].InputName);
            result.SignalsList = new List<Signal>();

            for (var i = 0 ; i < signalsTotalCount; i++)
            {
                var mergedSygnal = Signal.None;
                foreach(var dataNode in data)
                {
                    if(mergedSygnal == Signal.None)
                        mergedSygnal = dataNode.SignalsList[i];
                    else if(mergedSygnal != dataNode.SignalsList[i])
                    {
                        mergedSygnal = Signal.None;
                        break;
                    }
                }
                result.SignalsList.Add(mergedSygnal);
            }
            return result;
        }
    }
}
