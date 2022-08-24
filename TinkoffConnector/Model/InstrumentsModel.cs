using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffConnector.Model
{
    public class InstrumentsModel
    {
        public enum Currency
        {
            Rub = 0,
            Usd = 1,
            Eur = 2
        }

        public MarketInstrument Instrument { get; set; }
        public List<CandleModel> Candles { get; set; }

        public decimal? CurrentPrice { get; set; }
    }
}
