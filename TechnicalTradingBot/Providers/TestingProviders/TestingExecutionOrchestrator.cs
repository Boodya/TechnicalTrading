using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TechnicalTradingBot.Extensions;

namespace TechnicalTradingBot.Providers
{
    public class TestingExecutionOrchestrator : LiveExecutionOrchestrator
    {
        private DateTime _startDt;
        private DateTime _endDt;

        public TestingExecutionOrchestrator(DateTime startDt, DateTime endDt) : base()
        {
            _startDt = startDt;
            _endDt = endDt;
        }

        protected override void Execute()
        {
            DateTime dt = _startDt.GetCopy();
            while (dt < _endDt && _isExecuting)
            {
                dt = CalculateNextDate(dt);
                lock (_lock)
                {
                    _subscribers.ForEach(s => s.Invoke(dt.GetCopy()));
                }
            }

            _isExecuting = false;
            _onExecutionCompleted.Invoke();
        }

        private DateTime CalculateNextDate(DateTime dt)
        {
            dt = dt.AddHours(1);
            if(dt.TimeOfDay.Hours >= 19)
            {
                if(dt.DayOfWeek == DayOfWeek.Friday)
                    dt= dt.AddDays(3);
                else dt = dt.AddDays(1);
                return new DateTime(dt.Year, dt.Month, dt.Day, 9, 50, 0);
            }
            return dt;
        }
    }
}
