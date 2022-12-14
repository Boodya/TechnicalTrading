using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Network;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using System.Linq;

namespace TinkoffConnector.Connectors
{
    public class SandboxConnector : TinkoffConnectorBase, IAsyncDisposable
    {
        private static readonly Random _rand = new Random();
        private readonly SandboxContext _actualContext;

        public SandboxConnector(string authToken) : base(authToken)
        {
            _actualContext = (SandboxContext)context;
        }

        public override async Task RegisterAccount()
        {
            if (context == null)
                throw new Exception("Context not initialized");
            if (!string.IsNullOrEmpty(accountId))
                throw new Exception("Account is already registered");
            var sandboxAccount = await _actualContext.RegisterAsync(BrokerAccountType.Tinkoff);
            accountId = sandboxAccount.BrokerAccountId;
            InitializeRepositories();
        }

        public async Task SetBalance(Currency curr, decimal quantity)
        {
            if (context == null)
                throw new Exception("Context not initialized");
            else if (accountId == null)
                throw new Exception("Account ID is not set");
            await _actualContext.SetCurrencyBalanceAsync(curr, quantity, accountId);
        }

        public async Task SetRandomBalanceAsync(List<Currency> currencies)
        {
            foreach(var curr in currencies)
            {
                await SetBalance(curr, _rand.Next(1, 1000000));
            }
        }
        /// <summary>
        /// Sells all positions except currencies
        /// </summary>
        /// <returns></returns>
        public async Task ClearifyPortfolio()
        {
            var portfolio = await Portfolio.GetPortfolio();
            foreach(var position in portfolio.Positions)
            {
                if(position.InstrumentType != InstrumentType.Currency)
                {
                    await Orders.WaitWhileOrderWillBeCompleted((
                        await Orders.PlaceMarketOrder(position.Figi, 
                            (uint)position.Lots, OperationType.Sell)).OrderId);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (accountId != null)
                await _actualContext.RemoveAsync(accountId);
        }
    }
}
