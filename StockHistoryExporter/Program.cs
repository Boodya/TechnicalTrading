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
            processor.ExportToLocalDatabaseAsync(DateTime.Now.AddMonths(-6), DateTime.Now);
            //processor.MigrateDataToDBFromFileHistory();
        }
    }
}
