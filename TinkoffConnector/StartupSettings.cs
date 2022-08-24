using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TinkoffConnector.Model;

namespace TinkoffConnector
{
    public static class StartupSettings
    {
        public static SettingsModel AppSettings = Load();
        private static SettingsModel Load()
        {
            var myJsonString = File.ReadAllText("appsettings.json");
            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy" };
            var asppSettingsContent = JObject.Parse(myJsonString);
            var settings = new SettingsModel()
            {
                AuthToken = asppSettingsContent.SelectToken("authToken").Value<string>(),
                SandBoxAuthToken = asppSettingsContent.SelectToken("sandBoxAuthToken").Value<string>(),
                StockDatabasePath = asppSettingsContent.SelectToken("stockDatabasePath").Value<string>(),
                IsWriteErrorLogs = asppSettingsContent.SelectToken("isWriteErrorLogs").Value<bool>(),
                TimeoutRetryTimeInSeconds = asppSettingsContent.SelectToken("timeoutRetryTimeInSeconds").Value<int>(),
                StockMarketHolidays = JsonConvert.DeserializeObject<List<StockMarketHoliday>>(
                    asppSettingsContent.SelectToken("stockMarketHolidays").ToString(), dateTimeConverter),
                TickersBlackList = JsonConvert.DeserializeObject<List<string>>(
                    asppSettingsContent.SelectToken("tickersBlackList").ToString())
            };
            return settings;
        }
    }
}
