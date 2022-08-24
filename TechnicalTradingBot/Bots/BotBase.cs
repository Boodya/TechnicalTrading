using System;
using System.Collections.Generic;
using System.Text;
using TechnicalTradingBot.Model;

namespace TechnicalTradingBot.Bots
{
    public enum BotOutputCategory
    {
        BotOperation,
        Importing,
        Analyzing,
        Full
    }
    public abstract class BotBase
    {
        public ExecutionStatus Status { get; protected set; }
        protected readonly BotSettingsModel _settings;
        protected Dictionary<BotOutputCategory, List<Action<string>>> _categorizedOutputSubscribers
            = new Dictionary<BotOutputCategory, List<Action<string>>>();
        protected bool _isExecute;
        protected DateTime _currentDateTime;

        public BotBase(BotSettingsModel settings)
        {
            _settings = settings;
            _isExecute = false;
            _categorizedOutputSubscribers.Add(BotOutputCategory.Full, new List<Action<string>>());
        }

        protected abstract void Initialize();
        protected abstract void Execute(CandlesUpdateModel update);
        protected abstract void TimeUpdateOperation(DateTime currentTime);

        public void SubscribeOnOutputMessage(Action<string> subscription, BotOutputCategory category)
        {
            if(!_categorizedOutputSubscribers.ContainsKey(category))
                _categorizedOutputSubscribers.Add(category, new List<Action<string>>());
            _categorizedOutputSubscribers[category].Add(subscription);
            _categorizedOutputSubscribers[BotOutputCategory.Full].Add(subscription);
        }

        public void StartExecution()
        {
            _isExecute = true;
        }

        public void StopExecution()
        {
            _isExecute = false;
        }

        protected void WriteOutput(string message, BotOutputCategory category)
        {
            if(_categorizedOutputSubscribers.ContainsKey(category))
                _categorizedOutputSubscribers[category]
                    .ForEach(subscriber => subscriber.Invoke($"{_currentDateTime}: {message}"));
            /*_categorizedOutputSubscribers[BotOutputCategory.Full]
                .ForEach(subscriber => subscriber.Invoke(message));*/
        }
    }
}
