using TinkoffConnector.Logging.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkoffConnector.Logging
{
    internal static class Helpers
    {
        internal static Type GetLogTypeFromEnum(LogType type)
        {
            switch(type)
            {
                case LogType.TestsExecution: return typeof(LogTestsModel);
                case LogType.TestsResult: return typeof(LogTestsModel);
                case LogType.Info: return typeof(LogInfoModel);
                case LogType.Processing: return typeof(LogProcessingModel);
                case LogType.Error: return typeof(LogErrorModel);
                case LogType.Trading: return typeof(LogTradingModel);
            }
            return null;
        }
    }
}
