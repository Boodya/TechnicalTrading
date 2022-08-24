//Orders simulation class
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Connectors;

namespace TinkoffConnector.Repositories
{
    public class LocalOrdersRepository : IOrdersRepository, IDisposable
    {
        private LocalDBConnector _connector;
        private string _accountId;

        private List<Order> _openedOrders;
        private List<Order> _completedOrders;
        private int _orderIdNumber = 0;
        private Thread _orderProcessorThread;

        public LocalOrdersRepository(IContext context, string accountId)
        {
        }

        public void SetConnector(LocalDBConnector connector)
        {
            _connector = connector;
            Initialize(null, null);
        }

        public void Initialize(IContext context, string accountId)
        {
            _openedOrders = new List<Order>();
            _completedOrders = new List<Order>();
            _orderProcessorThread = new Thread(OrdersProcessorThread);
            _orderProcessorThread.Start();
        }

        public async Task<List<Order>> GetOpenedOrders()
        {
            return _openedOrders;
        }

        public async Task<PlacedMarketOrder> PlaceMarketOrder(string Figi, uint quantity, OperationType type)
        {
            _orderIdNumber++;
            var currentInstrumentPrice = await 
                _connector.Candles.GetCurrentInstrumentPrice(_connector.Instruments.GetInstrumentByFigi(Figi));
            var order = new Order(_orderIdNumber.ToString(),
                Figi, type, OrderStatus.New, (int)quantity, 0, OrderType.Limit, currentInstrumentPrice ?? 0M);
            _openedOrders.Add(order);
            return new PlacedMarketOrder(order.OrderId, order.Operation,
                order.Status, "", order.RequestedLots, order.ExecutedLots, null);
        }
        public async Task<PlacedLimitOrder> PlaceLimitOrder(string Figi, uint quantity, OperationType type, decimal price)
        {
            _orderIdNumber++;
            var order = new Order(_orderIdNumber.ToString(),
                Figi, type, OrderStatus.New, (int)quantity, 0, OrderType.Limit, price);
            _openedOrders.Add(order);
            return new PlacedLimitOrder(order.OrderId, order.Operation, 
                order.Status, "", order.RequestedLots, order.ExecutedLots, null);
        }

        public async Task CancelOrder(Order order)
        {
            _openedOrders.Remove(order);
        }

        public async Task WaitWhileOrderWillBeCompleted(string orderId)
        {
            while(true)
            {
                var order = _openedOrders.Where(x => x.OrderId == orderId).FirstOrDefault();
                if (order != null && order.Status != OrderStatus.Fill) Thread.Sleep(500);
                else break;
            }
        }
        private async void OrdersProcessorThread()
        {
            try
            {
                while (true)
                {
                    if(_openedOrders.Count > 0)
                    {
                        foreach (var order in _openedOrders)
                        {
                            var totalPrice = order.RequestedLots * order.Price;
                            //IMPLEMENT THE BALANCE DECREASING
                            //IMPLEMENT THE PROVISION OF BOUGHT STOCKS TO PORTFOLIO
                            _completedOrders.Add(new Order(order.OrderId,
                                order.Figi, order.Operation, OrderStatus.Fill,
                                order.RequestedLots, order.RequestedLots, order.Type, order.Price));
                        }
                        _openedOrders.Clear();
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
            }
        }

        public void Dispose()
        {
            _orderProcessorThread.Interrupt();
        }

        public Task GetExecutionOrdersHistory()
        {
            throw new NotImplementedException();
        }
    }
}
