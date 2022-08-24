using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffConnector.Repositories
{
    public interface IOrdersRepository : IAsyncRepository
    {
        public void Initialize(IContext context, string accountId);
        public Task<List<Order>> GetOpenedOrders();
        public Task<PlacedMarketOrder> PlaceMarketOrder(string Figi, uint quantity, OperationType type);
        public Task<PlacedLimitOrder> PlaceLimitOrder(string Figi, uint quantity, OperationType type, decimal price);
        public Task CancelOrder(Order order);
        public Task WaitWhileOrderWillBeCompleted(string orderId);
    }
}
