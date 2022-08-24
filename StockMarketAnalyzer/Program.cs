using System;

namespace StockMarketAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new AnalyzersProcessor();
            processor.SubscribeOnOutputMessage((message) =>
            {
                Console.WriteLine(message);
            });
            //processor.StartFullProcessing(DateTime.Now.AddMonths(-1), DateTime.Now);
            processor.RecalculateTradingIndicators(DateTime.Now);
        }
    }
}
