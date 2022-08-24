using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StockMarketAnalyzer.History
{
    public class AnalyzerHistoryRepository
    {
        private readonly string _rootFolderPath = "StockDatabase";
        private readonly string _calculationCsvName = "AnalyzerCalculations.csv";

        public AnalyzerHistoryRepository(string stockHistoryFolderPath)
        {
            _rootFolderPath = stockHistoryFolderPath;
        }

        public void SaveAnalyzerResults(string currency, List<TradingSimulationResults> results, string filePath = null)
        {
            if (results == null || results.Count == 0)
                return;
            var fullFilePath = filePath ?? Path.Combine(_rootFolderPath, currency, _calculationCsvName);
            var textContent = GenerateCsvContent(results);
            if (!string.IsNullOrEmpty(textContent))
            {
                File.WriteAllText(fullFilePath, textContent);
            }
        }

        public void SaveAnalyzerResults(string currency, TradingSimulationResults results, string filePath = null)
        {
            if (results == null)
                return;
            var fullFilePath = filePath ?? Path.Combine(_rootFolderPath, currency, _calculationCsvName);
            var resultNotes = new List<TradingSimulationResults>();
            if(File.Exists(fullFilePath))
            {
                resultNotes = LoadSimulationResults(currency);
            }
            var existingResult = resultNotes.Where(r => r.AnalyzerType == results.AnalyzerType 
                && r.InstrumentTicker == results.InstrumentTicker).FirstOrDefault();
            if (existingResult != null)
            {
                resultNotes.Remove(existingResult);
            }
            resultNotes.Add(results);
            resultNotes = resultNotes.OrderByDescending(r => r.TotalSuccessTradesConversionPercent).ToList();
            var textContent = GenerateCsvContent(resultNotes);
            if (!string.IsNullOrEmpty(textContent))
            {
                File.WriteAllText(fullFilePath, textContent);
            }
        }

        public List<TradingSimulationResults> LoadSimulationResults(string currency, string filePath = null)
        {
            var results = new List<TradingSimulationResults>();
            var fullFilePath = filePath ?? Path.Combine(_rootFolderPath, currency, _calculationCsvName);
            if (!File.Exists(fullFilePath))
                return results;
            using (var reader = new StreamReader(File.OpenRead(fullFilePath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values[0] == "InstrumentTicker")
                        continue;

                    try
                    {
                        results.Add(new TradingSimulationResults()
                        {
                            InstrumentTicker = values[0],
                            //values[1] skipped
                            TradingStartBalance = decimal.Parse(values[2]),
                            TradingResultBalance = decimal.Parse(values[3]),
                            TradingSimulationStartDateTime = DateTime.Parse(values[4]),
                            TradingSimulationEndDateTime = DateTime.Parse(values[5]),
                            TotalTrades = int.Parse(values[6]),
                            AnalyzerType = values[7],
                            TimeFrame = values[8],
                            TotalSuccessfulTrades = int.Parse(values[9]),
                            AverageHoursStayedInDeal = double.Parse(values[10]),
                            TotalSuccessTradesConversionPercent = double.Parse(values[11]),
                            LastDecision = values[12],
                        });
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            return results;
        }

        private string GenerateCsvContent(List<TradingSimulationResults> results, bool generateHeader = true)
        {
            if (results.Count == 0)
                return string.Empty;
            StringBuilder csv = new StringBuilder();
            if (generateHeader)
            {
                csv.AppendLine("InstrumentTicker,TradingResultInPercent,TradingStartBalance," +
                    "TradingResultBalance,TradingSimulationStartDateTime,TradingSimulationEndDateTime," +
                    "TotalTrades,AnalyzerType,TimeFrame,TotalSuccessfulTrades,AverageHoursStayedInDeal," +
                    "TotalSuccessTradesConversionPercent,LastDecision");
            }
            foreach (var c in results)
            {
                csv.AppendLine($"{c.InstrumentTicker},{c.TradingResultInPercent},{c.TradingStartBalance},{c.TradingResultBalance}," +
                    $"{c.TradingSimulationStartDateTime},{c.TradingSimulationEndDateTime},{c.TotalTrades},{c.AnalyzerType},{c.TimeFrame}," +
                    $"{c.TotalSuccessfulTrades},{c.AverageHoursStayedInDeal},{c.TotalSuccessTradesConversionPercent},{c.LastDecision}");
            }
            return csv.ToString();
        }
    }
}
