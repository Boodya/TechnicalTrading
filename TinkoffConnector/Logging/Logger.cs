using TinkoffConnector.Logging.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TinkoffConnector.Logging
{
    public static class Logger
    {
        private static readonly string _rootFolderName = "Logs";
        private static string _logFolder = string.Empty;

        public static void WriteInfoLog(string message)
        {
            WriteToLog(LogType.Info, new LogInfoModel()
            {
                Time = DateTime.Now,
                Message = message
            }, GetLogFileFullPath(LogType.Info));
        }

        public static void WriteTestsResultLog(string message)
        {
            WriteToLog(LogType.TestsResult, new LogTestsModel()
            {
                Message = message
            }, GetLogFileFullPath(LogType.TestsResult));
        }

        public static void WriteTestsExecutionLog(string message)
        {
            WriteToLog(LogType.TestsExecution, new LogTestsModel()
            {
                Message = message
            }, GetLogFileFullPath(LogType.TestsExecution));
        }

        public static void ExportProcessedData(string typeOfData, double[] dataToSave)
        {
            var decimalData = new List<decimal>();
            foreach (var doubl in dataToSave)
                decimalData.Add((decimal)doubl);
            ExportProcessedData(typeOfData, decimalData.ToArray());
        }

        public static void ExportProcessedData(string typeOfData, decimal[] dataToSave)
        {
            CheckLogDirectory();
            List<LogProcessingModel> logData = new List<LogProcessingModel>();
            foreach (var data in dataToSave)
                logData.Add(new LogProcessingModel()
                {
                    Time = DateTime.Now,
                    TypeOfData = typeOfData,
                    ProcessedData = data
                });
            WriteToLog(LogType.Processing, logData, Path.Join(_logFolder,
                $"ProcessingData_{typeOfData}_{DateTime.Now.ToString("dd-MM-yy_HH-mm-ss")}.csv"));
        }

        public static void WriteProcessingLog(string typeOfData, decimal calculation)
        {
            WriteToLog(LogType.Processing, new LogProcessingModel()
            {
                Time = DateTime.Now,
                TypeOfData = typeOfData,
                ProcessedData = calculation
            }, GetLogFileFullPath(LogType.Processing));
        }

        public static void WriteErrorLog(Exception ex)
        {
            WriteToLog(LogType.Error, new LogErrorModel()
            {
                Time = DateTime.Now,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            }, GetLogFileFullPath(LogType.Error));
        }

        private static void WriteToLog(LogType type, IEnumerable<ILogModel> logs, string fileFullPath)
        {
            StringBuilder strb = new StringBuilder(
                CreateLogHeaders(type));
            foreach (var log in logs)
            {
                strb.Append(BuildStringLineForLogFile(type, log));
            }
            File.AppendAllText(fileFullPath, strb.ToString());
        }

        private static void WriteToLog(LogType type, ILogModel log, string fileFullPath)
        {
            if (log == null || (type == LogType.Error && !StartupSettings.AppSettings.IsWriteErrorLogs))
                return;
            var content = BuildStringLineForLogFile(type, log);
            File.AppendAllText(fileFullPath, content);
        }

        private static string BuildStringLineForLogFile(LogType type, ILogModel log)
        {
            var actualLogType = Helpers.GetLogTypeFromEnum(type);
            var typedLog = Convert.ChangeType(log, actualLogType);
            PropertyInfo[] properties = actualLogType.GetProperties();
            var logValues = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                var propVal = property.GetValue(typedLog, null);
                if (propVal != null)
                    propVal = propVal.ToString();
                else propVal = string.Empty;
                logValues.Add(propVal as string);
            }
            return new StringBuilder().AppendLine(
                string.Join(',', logValues)).ToString();
        }

        private static void CheckLogFileExist(LogType type)
        {
            CheckLogDirectory();
            var fileFullPath = GetLogFileFullPath(type);
            if(!File.Exists(fileFullPath))
            {
                using (StreamWriter stream = new StreamWriter(File.Create(fileFullPath)))
                    stream.Write(CreateLogHeaders(type));
            }
        }

        private static void CheckLogDirectory()
        {
            if (string.IsNullOrEmpty(_logFolder))
            {
                var logFolder = Path.Combine(Directory.GetCurrentDirectory(),
                    _rootFolderName, DateTime.Now.ToString("dd-MM-yy_HH-mm-ss"));
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);
                _logFolder = logFolder;
            }
        }

        private static string CreateLogHeaders(LogType type)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PropertyInfo[] properties = Helpers.GetLogTypeFromEnum(type).GetProperties();           
            return new StringBuilder().AppendLine(string.Join(',', properties.Select(x => x.Name)
                .ToList())).ToString();
        }

        private static string GetLogFileFullPath(LogType type)
        {
            CheckLogDirectory();
            var fileName = string.Empty;
            switch (type)
            {
                case LogType.Info: fileName = "InformationLogs.csv"; break;
                case LogType.Processing: fileName = "ProcessingLogs.csv"; break;
                case LogType.Error: fileName = "ErrorLogs.csv"; break;
                case LogType.Trading: fileName = "TradingOperationsLog.csv"; break;
                case LogType.TestsExecution: fileName = "TestsExecutionLog.csv"; break;
                case LogType.TestsResult: fileName = "TestsResultLog.csv"; break;
            }
            return Path.Combine(_logFolder, fileName);
        }
    }
}
