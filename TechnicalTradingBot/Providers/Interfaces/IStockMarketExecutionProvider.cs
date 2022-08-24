using System;
using System.Collections.Generic;
using System.Text;
using TechnicalTradingBot.Model;

namespace TechnicalTradingBot.Providers
{
    public interface IStockMarketExecutionProvider
    {
        public decimal GetFreeBalance(string currency);
        public decimal GetTotalBalance(string currency);
        public PositionDataModel GetOpenedPosition(string ticker);
        public Guid AddOrderToQueue(string ticker, string currency, OperationType type, int Amount, Action<MarketOrderModel> onComplete);
        public void RemoveOrderFromQueue(Guid orderId);

        public List<PositionDataModel> GetClosedPositionResults();
    }
}
