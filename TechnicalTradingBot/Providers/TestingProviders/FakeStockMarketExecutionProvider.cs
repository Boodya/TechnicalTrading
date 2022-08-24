using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TechnicalTradingBot.Model;

namespace TechnicalTradingBot.Providers
{
    public class FakeStockMarketExecutionProvider : IStockMarketExecutionProvider, IDisposable
    {
        private Dictionary<Guid, Action<MarketOrderModel>> _subscribers;
        private DateTime _currentDate;

        private List<MarketOrderModel> _ordersToExecute;
        private List<MarketOrderModel> _completedOrders;
        private List<PositionDataModel> _openedPositions;
        private List<PositionDataModel> _closedPositions;

        private readonly object _orderQueueOperationLocker = new object();
        private Dictionary<string,decimal> _freeBalance;
        private Thread _orderExecutionThread;
        private bool _isExecuting = true;
        private ICandleProvider _candleProvider;
        private readonly decimal _marketCommissionPercent = 0.04m;

        public FakeStockMarketExecutionProvider(IExecutionOrchestrator dtProvider, 
            ICandleProvider candleProvider, Dictionary<string, decimal> startBalance)
        {
            _ordersToExecute = new List<MarketOrderModel>();
            _completedOrders = new List<MarketOrderModel>();
            _openedPositions = new List<PositionDataModel>();
            _closedPositions = new List<PositionDataModel>();
            _subscribers = new Dictionary<Guid, Action<MarketOrderModel>>();
            _freeBalance = new Dictionary<string, decimal>();
            foreach (var balance in startBalance)
                _freeBalance.Add(balance.Key, balance.Value);

            _candleProvider = candleProvider;
            dtProvider.SubscribeOnUpdate((DateTime dt) =>
            {
                _currentDate = dt;
                OrderExecution();
            });
        }

        private void OrderExecution()
        {
            lock (_orderQueueOperationLocker)
            {
                foreach(var order in _ordersToExecute)
                {
                    ExecuteOrder(order);
                    order.Status = OrderExecutionStatus.Executed;
                    _completedOrders.Add(order);
                }
                _ordersToExecute.Clear();
            }
        }

        public Guid AddOrderToQueue(string ticker, string currency,
            OperationType type, int Amount, Action<MarketOrderModel> onComplete)
        {
            Guid orderId = Guid.Empty;
            lock(_orderQueueOperationLocker)
            {
                var order = new MarketOrderModel()
                {
                    Amount = Amount,
                    OperationType = type,
                    Ticker = ticker,
                    Currency = currency,
                };
                _ordersToExecute.Add(order);
                _subscribers.Add(order.Id, onComplete);
                orderId = order.Id;
            }
            return orderId;
        }

        public decimal GetFreeBalance(string currency)
        {
            if(_freeBalance.ContainsKey(currency))
                return _freeBalance[currency];
            return 0;
        }

        public PositionDataModel GetOpenedPosition(string ticker)
        {
            return _openedPositions.Where(p => p.Ticker == ticker).FirstOrDefault();
        }

        public decimal GetTotalBalance(string currency)
        {
            throw new NotImplementedException();
        }

        public void RemoveOrderFromQueue(Guid orderId)
        {
            var existingOrder = _ordersToExecute.FirstOrDefault(o => o.Id == orderId);
        }

        private void SubscribeOnOrderExecuted(Guid orderId, Action<MarketOrderModel> orderResult)
        {
            _subscribers.Add(orderId, orderResult);
        }

        private void ExecuteOrder(MarketOrderModel order)
        {
            var lastPrice = _candleProvider.GetLastPriceFromTicker(order.Ticker);
            if (lastPrice == null)
                throw new Exception($"Unable to fetch last price for {order.Ticker} on {_currentDate}.");

            var factPrice = lastPrice.Value * order.Amount;
            var openedPosition = _openedPositions.FirstOrDefault(p => p.Ticker == order.Ticker);
            if (openedPosition != null)
            {
                if (openedPosition.Amount > 0)
                {
                    if (order.OperationType == OperationType.Buy)//Extend
                    {
                        OperateBalance(order.Currency, -factPrice);
                        openedPosition.Amount += order.Amount;
                        throw new NotImplementedException("Extending position not implemented");
                    }
                    else
                    {
                        OperateBalance(order.Currency, factPrice);
                        openedPosition.PositionClosedDate = _currentDate;
                        openedPosition.ClosedPrice = lastPrice.Value;
                        _openedPositions.Remove(openedPosition);
                        _closedPositions.Add(openedPosition);
                    }
                }
                else
                {
                    if (order.OperationType == OperationType.Sell)//Extend
                    {
                        OperateBalance(order.Currency, factPrice);
                        openedPosition.Amount -= order.Amount;
                        throw new NotImplementedException("Extending position not implemented");
                    }
                    else
                    {
                        OperateBalance(order.Currency, -factPrice);
                        openedPosition.PositionClosedDate = _currentDate;
                        openedPosition.ClosedPrice = lastPrice.Value;
                        _openedPositions.Remove(openedPosition);
                        _closedPositions.Add(openedPosition);
                    }
                }
            }
            else
            {
                factPrice = lastPrice.Value * order.Amount;
                var positionAmount = order.OperationType == OperationType.Buy ?
                    order.Amount : order.Amount * -1;

                if (order.OperationType == OperationType.Buy)
                    OperateBalance(order.Currency, -factPrice);
                else OperateBalance(order.Currency, factPrice);

                _openedPositions.Add(new PositionDataModel()
                {
                    Amount = positionAmount,
                    Price = lastPrice.Value,
                    Currency = order.Currency,
                    Ticker = order.Ticker,
                    OpeningPrice = lastPrice.Value,
                    PositionOpenedDate = _currentDate,
                });
            }
        }

        private void OperateBalance(string currency, decimal operation)
        {
            _freeBalance[currency] += operation;
            _freeBalance[currency] -= operation / 100 * _marketCommissionPercent;
        }

        public void Dispose()
        {
            _isExecuting = false;
        }

        public List<PositionDataModel> GetClosedPositionResults()
        {
            return _closedPositions;
        }
    }
}
