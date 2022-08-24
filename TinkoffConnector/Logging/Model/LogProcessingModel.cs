using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Logging.Model
{
    internal class LogProcessingModel : ILogModel
    {
        public DateTime Time { get; set; }
        public string TypeOfData { get; set; }
        public decimal ProcessedData { get; set; }
    }
}
