using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Logging.Model
{
    internal class LogInfoModel : ILogModel
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
}
