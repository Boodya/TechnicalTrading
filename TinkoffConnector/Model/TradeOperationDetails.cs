using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffConnector.Models
{
    public class TradeOperationDetails
    {
        public DateTime OperationDate { get; set; }
        public MarketInstrument Instrument { get; set; }
        public decimal Price { get; set; }
        public uint Amount { get; set; }
        public OperationType OperationType { get; internal set; }
        public decimal CurrentBalance { get; set; }

        public TradeOperationDetails(DateTime dt, MarketInstrument instrument, decimal price, uint amount, OperationType type)
        {
            if (dt == null || instrument == null)
            {
                throw new ArgumentNullException("Unable to instantiate object with null DateTime or MarketInstrument");
            }
            OperationDate = dt;
            Instrument = instrument;
            Price = price;
            Amount = amount;
            OperationType = type;
        }

        public string AsString()
        {
            return $"Date: {OperationDate}, Instrument: {Instrument.Name}({Instrument.Ticker}), " +
                $"Operation: {OperationType}, Amount: {Amount}, Price: {Price}.";
        }
    }
}
