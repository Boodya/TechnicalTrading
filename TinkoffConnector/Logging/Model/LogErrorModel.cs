using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Logging.Model
{
    internal class LogErrorModel : ILogModel
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
