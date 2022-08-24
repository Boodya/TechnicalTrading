using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffConnector.Logging;
using TinkoffConnector.Model;

namespace TinkoffConnector.History
{
    public class StockHistoryRepository : IStockHistoryRepository
    {
        public List<string> Currencies;
        private Dictionary<string, List<string>> _tickers;
        private string _rootFolderPath = "StockDatabase";
        private object _locker = new object();
        private Dictionary<string, IEnumerable<CandleModel>> _fileReadBuffer;
        private bool _isCachedMode;

        public Dictionary<string, List<string>> Tickers { get { return _tickers; } }

        /// <summary>
        /// Start Repository instance in cached mode with all currencies data preloaded. Note initialization can takes time to init cache.
        /// </summary>
        public StockHistoryRepository(string dbFolderPath, DateTime startDate, DateTime endDate,
            string currency, CandleIntervals candleInterval)
        {
            _isCachedMode = true;
            Initialize(dbFolderPath);
            PrepareCache(currency, startDate, endDate, candleInterval);
        }

        public StockHistoryRepository(string dbFolderPath)
        {
            _isCachedMode = false;
            Initialize(dbFolderPath);
        }

        private void Initialize(string dbFolderPath)
        {
            _rootFolderPath = dbFolderPath;
            Currencies = new List<string>();
            _tickers = new Dictionary<string, List<string>>();
            _fileReadBuffer = new Dictionary<string, IEnumerable<CandleModel>>();
            var currenciesDir = Directory.GetDirectories(_rootFolderPath, "*", SearchOption.TopDirectoryOnly);
            foreach (string currencyDir in currenciesDir)
            {
                if (currencyDir == "Trading")
                    continue;
                var currency = currencyDir.Split('\\').Last();
                Currencies.Add(currency);
                _tickers.Add(currency, new List<string>());
                foreach (string tickerDir in Directory.GetFiles(currencyDir))
                {
                    if (tickerDir.Contains("AnalyzerCalculations"))
                        continue;
                    Tickers[currency].Add(tickerDir.Split('\\').Last().Replace(".csv", ""));
                }
            }
        }

        private void PrepareCache(string currency, DateTime startDate, DateTime endDate, CandleIntervals candleInterval)
        {
            foreach (var instrument in Tickers[currency])
            {
                GetStockHistory(currency, instrument, startDate, endDate, candleInterval);
            }
        }

        /// <summary>
        /// Returns candles for specified period of time. If it's not presented in history - returns empty list
        /// </summary>
        public IEnumerable<CandleModel> GetStockHistory(string currency, string ticker, DateTime from, DateTime to, string interval)
        {
            if (to < from)
                throw new Exception("Date FROM cannot be greater then date TO");
            var fullHistory = GetStockHistory(currency, ticker, from, to);
            return Helpers.ProcessInterval(fullHistory, interval.ToEnum<CandleIntervals>());
        }

        public decimal GetLastPriceForInstrument(string currency, string ticker)
        {
            var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),
                _rootFolderPath, currency), $"{ticker}.csv");
            return BuildCandleModelFromString(
                File.ReadAllLines(filePath).Last()
                .Split(',')).Close;
        }

        /// <summary>
        /// Returns null if there are no historical data for stock
        /// </summary>
        public DateTime GetLastNoteTimeForStock(string currency, string ticker)
        {
            DateTime maxDate = DateTime.MinValue;
            var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),
                _rootFolderPath, currency), $"{ticker}.csv");
            var allFileLines = File.ReadAllLines(filePath);
            var dtString = allFileLines[allFileLines.Length - 1].Split(',')[1];
            return DateTime.Parse(dtString);
        }

        /// <summary>
        /// Returns null if there are no historical data for stock
        /// </summary>
        public DateTime GetFirstNoteTimeForStock(string currency, string ticker)
        {
            DateTime minDate = DateTime.MaxValue;
            var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),
                _rootFolderPath, currency), $"{ticker}.csv");
            var allFileLines = File.ReadAllLines(filePath);
            var dtString = allFileLines[1].Split(',')[1];
            return DateTime.Parse(dtString);
        }

        public string GenerateStockHistoryCsv(List<CandleModel> candles, bool generateHeader)
        {
            if (candles.Count == 0)
                return string.Empty;
            StringBuilder csv = new StringBuilder();
            if (generateHeader)
            {
                csv.AppendLine("Figi,Time,AveragePrice,Low,High,Open,Close,Volume,Interval");
            }
            foreach (var c in candles)
            {
                csv.AppendLine($"{c.Figi},{c.Time},{c.AveragePrice},{c.Low},{c.High},{c.Open},{c.Close},{c.Volume},{c.Interval}");
            }
            return csv.ToString();
        }

        public bool CheckHistoryExist(string currency, string ticker)
        {
            var dirPath = Path.Combine(Directory.GetCurrentDirectory(), _rootFolderPath, currency);
            return File.Exists(Path.Combine(dirPath, $"{ticker}.csv"));
        }

        public string SaveHistory(string currency, string stockTitle, List<CandleModel> candles,
            string specialPath = "", bool isOverwrite = false)
        {
            string savePath, dirPath;
            if (string.IsNullOrEmpty(specialPath))
                dirPath = Path.Combine(Directory.GetCurrentDirectory(), _rootFolderPath, currency);
            else
                dirPath = Path.Combine(Directory.GetCurrentDirectory(), _rootFolderPath, specialPath);

            Directory.CreateDirectory(dirPath);
            savePath = Path.Combine(dirPath, $"{stockTitle}.csv");
            var generateHeader = true;
            if (File.Exists(savePath))
            {
                var lastDate = GetLastNoteTimeForStock(currency, stockTitle);
                candles.RemoveAll(x => x.Time <= lastDate);
                generateHeader = false;
            }
            var textContent = GenerateStockHistoryCsv(candles, generateHeader);
            if (!string.IsNullOrEmpty(textContent))
            {
                lock (_locker)
                {
                    if (isOverwrite)
                        File.WriteAllText(savePath, textContent);
                    else File.AppendAllText(savePath, textContent);
                }
            }
            return savePath;
        }

        public IEnumerable<CandleModel> GetStockHistory(string currency, string ticker,
            DateTime? from = null, DateTime? to = null, CandleIntervals? interval = null)
        {
            var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),
                _rootFolderPath, currency), $"{ticker}.csv");

            if (!_isCachedMode)
            {
                return CutCandlesByDate(from, to,
                    ReadHistoryFromFile(filePath));
            }

            if (!_fileReadBuffer.ContainsKey(filePath))
            {
                var candles = ReadHistoryFromFile(filePath);
                if (from.HasValue && to.HasValue)
                {
                    candles = CutCandlesByDate(from, to, candles);
                    if (interval.HasValue)
                        candles = Helpers.ProcessInterval(candles, interval.Value);
                }
                _fileReadBuffer.Add(filePath, candles);
            }
            return _fileReadBuffer[filePath];
        }

        private IEnumerable<CandleModel> CutCandlesByDate(DateTime? from, DateTime? to, IEnumerable<CandleModel> allCandles)
        {
            if (from.HasValue && to.HasValue)
                allCandles = allCandles.Where(c => c.Time >= from && c.Time <= to);
            else if (from.HasValue)
                allCandles = allCandles.Where(c => c.Time >= from);
            else if (to.HasValue)
                allCandles = allCandles.Where(c => c.Time <= to);
            return allCandles;
        }

        private IEnumerable<CandleModel> ReadHistoryFromFile(string filePath)
        {
            List<CandleModel> candles = new List<CandleModel>();
            if (File.Exists(filePath))
            {
                lock (_locker)
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        var line = sr.ReadLine();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            try
                            {
                                var elems = line.Split(',');
                                candles.Add(
                                    BuildCandleModelFromString(elems));
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteErrorLog(ex);
                                throw;
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
            }
            return candles;
        }

        private CandleModel BuildCandleModelFromString(string[] csvValues)
        {
            return new CandleModel()
            {
                Figi = csvValues[0],
                Time = DateTime.Parse(csvValues[1]),
                AveragePrice = decimal.Parse(csvValues[2], CultureInfo.InvariantCulture),
                Low = decimal.Parse(csvValues[3], CultureInfo.InvariantCulture),
                High = decimal.Parse(csvValues[4], CultureInfo.InvariantCulture),
                Open = decimal.Parse(csvValues[5], CultureInfo.InvariantCulture),
                Close = decimal.Parse(csvValues[6], CultureInfo.InvariantCulture),
                Volume = decimal.Parse(csvValues[7], CultureInfo.InvariantCulture),
                Interval = csvValues[8]
            };
        }
    }
}
