using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffConnector
{
    public static class DateTimeHelpers
    {
        private static object _locker = new object();
        private static DateTime? _customCurrentDate;
        /// <summary>
        /// Returns custom current date value if it was set. To set value use SetCurrentDate method
        /// </summary>
        public static DateTime CurrentDate
        {
            get
            {
                lock (_locker)
                {
                    return _customCurrentDate ?? DateTime.Now;
                }
            }
        }
        private static readonly TimeSpan _stockMarketStartTime = new TimeSpan(10, 00, 00);
        private static readonly TimeSpan _rubStockMarketEndTime = new TimeSpan(23, 50, 00);
        private static readonly TimeSpan _otherStockMarketEndTime = new TimeSpan(01, 45, 00);

        public static void SetCurrentDate(DateTime customDate)
        {
            lock(_locker)
            {
                _customCurrentDate = customDate;
            }
        }
        public static bool IsAMStockMarketOpened(this DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday ||
                IsStockHoliday(date, Consts.UsdMarket))
            {
                return false;
            }
            return true;
        }
        public static bool IsRusStockMarketOpened(this DateTime date)
        {
            if(date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday ||
                IsStockHoliday(date, Consts.RubMarket))
            {
                return false;
            }
            return true;
        }

        public static bool IsStockMarketOpened(this DateTime date, MarketInstrument instrument)
        {
            var validDate = GetLastBusinessDateTimeForInstrument(date, instrument);
            return date == validDate;
        }

        public static DateTime GetLastBusinessDateTimeForInstrument(this DateTime date, MarketInstrument instrument, TimeSpan? setTime = null)
        {
            if (setTime != null)
                return ValidateDaysOfWeek(new DateTime(date.Year, date.Month,
                    date.Day, setTime.Value.Hours,
                    setTime.Value.Minutes, setTime.Value.Seconds),instrument);
            return ValidateTime(ValidateDaysOfWeek(date, instrument), instrument, date);
        }

        private static DateTime ValidateTime(DateTime dayValidatedDate, MarketInstrument instrument, DateTime actualDate)
        {
            var rD = dayValidatedDate;
            if(instrument.Currency == Currency.Rub)
            {
                if(dayValidatedDate.TimeOfDay >= _rubStockMarketEndTime || dayValidatedDate < actualDate)
                {
                    rD = new DateTime(rD.Year, rD.Month, rD.Day,
                        _rubStockMarketEndTime.Hours,
                        _rubStockMarketEndTime.Minutes,
                        _rubStockMarketEndTime.Seconds);
                }
                if(dayValidatedDate.TimeOfDay <= _stockMarketStartTime)
                {
                    rD = ValidateDaysOfWeek(dayValidatedDate.AddDays(-1), instrument);
                    rD = new DateTime(rD.Year, rD.Month, rD.Day,
                        _rubStockMarketEndTime.Hours,
                        _rubStockMarketEndTime.Minutes,
                        _rubStockMarketEndTime.Seconds);
                }
            }
            else
            {
                if((dayValidatedDate.TimeOfDay >= _otherStockMarketEndTime && 
                    dayValidatedDate.TimeOfDay <= _stockMarketStartTime) || dayValidatedDate < actualDate)
                {
                    rD = new DateTime(rD.Year, rD.Month, rD.Day,
                        _otherStockMarketEndTime.Hours,
                        _otherStockMarketEndTime.Minutes,
                        _otherStockMarketEndTime.Seconds);
                }
            }
            return rD;
        }
        private static DateTime ValidateDaysOfWeek(DateTime date, MarketInstrument instrument)
        {
            var resultDate = date;
            var currentMarketEndTime = instrument.Currency == Currency.Rub ? _rubStockMarketEndTime : _otherStockMarketEndTime;
            var marketTicker = instrument.Currency == Currency.Rub ? Consts.RubMarket :
                instrument.Currency == Currency.Usd ? Consts.UsdMarket : "";
            while(true)
            {
                if (resultDate.DayOfWeek == DayOfWeek.Saturday && resultDate.TimeOfDay != currentMarketEndTime)
                {
                    resultDate = resultDate.AddDays(-1);
                    continue;
                }
                else if (resultDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    resultDate = resultDate.AddDays(-1);
                    continue;
                }

                if(resultDate.IsStockHoliday(marketTicker) ||
                    (resultDate.TimeOfDay == currentMarketEndTime &&
                    resultDate.AddDays(-1).IsStockHoliday(marketTicker)))
                {
                    resultDate = resultDate.AddDays(-1);
                    continue;
                }                
                return resultDate;
            }
        }

        public static bool IsStockHoliday(this DateTime date, string stockMarketTicker)
        {
            var holidayEntity = StartupSettings.AppSettings.StockMarketHolidays.Where(
                        x => x.StockMarket == stockMarketTicker && x.DateOff == date.Date).FirstOrDefault();
            return holidayEntity != null;
        }
    }
}
