using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Model
{
    public class CachedCandlesModel
    {
        public DateTime RequestedFromDate { get; set; }
        public DateTime RequestedToDate { get; set;}
        public List<CandleModel> Candles { get; set; }
    }
}
