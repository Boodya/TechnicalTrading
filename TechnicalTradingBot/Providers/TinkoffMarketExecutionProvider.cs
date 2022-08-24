using System;
using System.Collections.Generic;
using System.Text;
using TechnicalTradingBot.Model;

namespace TechnicalTradingBot.Providers
{
    public class TinkoffMarketExecutionProvider : IStockMarketExecutionProvider
    {
        public void AddOrderToQueue(MarketOrderModel order)
        {
            throw new NotImplementedException();
        }

        public Guid AddOrderToQueue(string ticker, OperationType type,
            int Amount, Action<MarketOrderModel> onComplete)
        {
            throw new NotImplementedException();
        }

        public Guid AddOrderToQueue(string ticker, string currency, OperationType type, int Amount, Action<MarketOrderModel> onComplete)
        {
            throw new NotImplementedException();
        }

        public List<PositionDataModel> GetClosedPositionResults()
        {
            throw new NotImplementedException();
        }

        public decimal GetFreeBalance(string currency)
        {
            throw new NotImplementedException();
        }

        public PositionDataModel GetOpenedPosition(string ticker)
        {
            throw new NotImplementedException();
        }

        public decimal GetTotalBalance(string currency)
        {
            throw new NotImplementedException();
        }

        public void RemoveOrderFromQueue(Guid orderId)
        {
            throw new NotImplementedException();
        }

        private void SubscribeOnOrderExecuted(Guid orderId, Action<MarketOrderModel> orderResult)
        {
            throw new NotImplementedException();
        }
    }
}
