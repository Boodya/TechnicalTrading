using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarketAnalyzer.Models
{
    public class TradingSimulationResults
    {
        public string InstrumentTicker { get; set; }
        public decimal TradingResultInPercent 
        { 
            get 
            { 
                return (TradingResultBalance - TradingStartBalance) / TradingStartBalance * 100; 
            } 
        }
        public decimal TradingStartBalance { get; set; }
        public decimal TradingResultBalance { get; set; }
        public DateTime TradingSimulationStartDateTime { get; set; }
        public DateTime TradingSimulationEndDateTime { get; set; }
        public int TotalTrades  { get; set; }
        public string AnalyzerType { get; set; }
        public string TimeFrame { get; set; }
        public int TotalSuccessfulTrades { get; set; }
        public double AverageHoursStayedInDeal { get; set; }
        public double TotalSuccessTradesConversionPercent { get; set; }
        public string LastDecision { get; set; }
        public string UniqId
        {
            get
            {
                return $"{InstrumentTicker}_{AnalyzerType}";
            }
        }
        [BsonId]
        public ObjectId Id { get; set; }
    }
}
