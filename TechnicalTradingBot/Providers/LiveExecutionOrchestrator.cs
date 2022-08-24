using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TechnicalTradingBot.Extensions;

namespace TechnicalTradingBot.Providers
{
    public class LiveExecutionOrchestrator : IExecutionOrchestrator
    {
        protected List<Action<DateTime>> _subscribers;
        protected Thread _mainThread;
        protected bool _isExecuting = true;
        protected object _lock = new object();
        protected Action _onExecutionCompleted;

        public LiveExecutionOrchestrator()
        {
            _subscribers = new List<Action<DateTime>>();
        }

        protected virtual void Execute()
        {
            while (_isExecuting)
            {
                var dt = DateTime.Now;
                lock (_lock)
                {
                    /*_subscribers.ForEach(s => new Thread(() =>
                    {
                        s.Invoke(dt);
                    }).Start());*/
                    _subscribers.ForEach(s => s.Invoke(dt.GetCopy()));
                }
                Thread.Sleep(1000);
            }
            _onExecutionCompleted.Invoke();
        }

        public void SubscribeOnUpdate(Action<DateTime> onUpdate)
        {
            lock (_lock)
            {
                _subscribers.Add(onUpdate);
            }
        }

        public void StartExecution(Action onExecutionCompleted)
        {
            _onExecutionCompleted = onExecutionCompleted;
            _mainThread = new Thread(Execute);
            _mainThread.Start();
            _isExecuting = true;
        }

        public void Stop()
        {
            _isExecuting=false;
        }

        public bool IsExecuting()
        {
            return _isExecuting;
        }
    }
}
