using LiteDB;
using StockMarketAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinkoffConnector.Model;

namespace StockMarketAnalyzer.History
{
    public class AnalyzerHistoryRepository
    {
        private readonly string _stockHistoryDBPath = "StockDatabase";
        private readonly string _tableName = "TradingSimulationResults";

        public AnalyzerHistoryRepository(string stockHistoryFolderPath)
        {
            _stockHistoryDBPath = stockHistoryFolderPath;
        }

        public void SaveAnalyzerResults(List<TradingSimulationResults> results)
        {
            using (var db = new LiteDatabase(_stockHistoryDBPath))
            {
                var col = db.GetCollection<TradingSimulationResults>(_tableName);
                col.EnsureIndex(x => x.UniqId, true);

                results.ForEach(result =>
                {
                    var existingItem = col
                        .Find(dbCandle => dbCandle.UniqId == result.UniqId)
                        .FirstOrDefault();
                    if (existingItem != null)
                    {
                        result.Id = existingItem.Id;
                        col.Update(result);
                    }
                    else col.Insert(result);     
                });
            }
        }

        public List<TradingSimulationResults> LoadSimulationResults()
        {
            using (var db = new LiteDatabase(_stockHistoryDBPath))
            {
                if (!db.CollectionExists(_tableName))
                    return new List<TradingSimulationResults>();

                var col = db.GetCollection<TradingSimulationResults>(_tableName);
                return col.FindAll()
                    .OrderByDescending(r => r.TradingResultInPercent)
                    .ToList();
            }
        }
    }
}
