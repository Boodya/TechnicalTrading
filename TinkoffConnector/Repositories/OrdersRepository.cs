using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffConnector.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private IContext _context;
        private string _accountId;
        private List<Order> _ordersHistory;

        public OrdersRepository(IContext context, string accountId)
        {
            Initialize(context, accountId);
        }

        public void Initialize(IContext context, string accountId)
        {
            if (context == null)
                throw new Exception("Unable to Initialize repo: Empty conext.");
            if (accountId == null)
                throw new Exception("Unable to Initialize repo: Empty account ID.");
            this._context = context;
            this._accountId = accountId;
            _ordersHistory = new List<Order>();
        }

        public async Task<List<Order>> GetOpenedOrders()
        {   
            return await _context.OrdersAsync(_accountId);
        }

        public async Task<PlacedMarketOrder> PlaceMarketOrder(string Figi, uint quantity, OperationType type)
        {
            var order = await _context.PlaceMarketOrderAsync(new MarketOrder(Figi, (int)quantity, type, _accountId));
            return order;
        }
        public async Task<PlacedLimitOrder> PlaceLimitOrder(string Figi, uint quantity, OperationType type, decimal price)
        {
            var order = await _context.PlaceLimitOrderAsync(new LimitOrder(Figi, (int)quantity, type, price, _accountId));
            return order;
        }

        public async Task CancelOrder(Order order)
        {
            await _context.CancelOrderAsync(order.OrderId, _accountId);
        }

        public async Task WaitWhileOrderWillBeCompleted(string orderId)
        {
            while(true)
            {
                var allOrders = await GetOpenedOrders();
                if (allOrders.FirstOrDefault(x => x.OrderId == orderId) != null)
                    await Task.Delay(500);
                else return;
            }
        }
    }
}
