using System;
using System.Collections.Generic;
using System.Text;

namespace TechnicalTradingBot.Providers
{
    public interface IExecutionOrchestrator
    {
        public void SubscribeOnUpdate(Action<DateTime> onUpdate);
        public void StartExecution(Action onExecutionCompleted);
        public void Stop();
        public bool IsExecuting();
    }
}
