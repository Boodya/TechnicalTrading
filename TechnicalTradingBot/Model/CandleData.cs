﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot.Model
{
    public class CandleData
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
}
