using System;

namespace StockHistoryExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new StockMarketExporter();
            processor.SubscribeOnOutputMessage((message) =>
            {
                Console.WriteLine(message);
            });
            processor.ExportToLocalDatabaseAsync(new DateTime(2015, 01, 01, 00, 00, 00), DateTime.Now);
        }
    }
}
