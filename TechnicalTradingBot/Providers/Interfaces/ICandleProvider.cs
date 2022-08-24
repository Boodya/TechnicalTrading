using System;
using TechnicalTradingBot.Model;

namespace TechnicalTradingBot.Providers
{
    public interface ICandleProvider
    {
        public decimal? GetLastPriceFromTicker(string ticker);
        public void SubscribeOnUpdate(string ticker, Action<CandlesUpdateModel> onUpdate);
        public void Unsubscribe(string ticker, Action<CandlesUpdateModel> execution);
    }
}
