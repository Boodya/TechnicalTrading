using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Model;

namespace TinkoffConnector.Repositories
{
    public interface IPortfolioRepository : IAsyncRepository
    {
        public Task<PortfolioModel> GetFullPortfolio();
        public Task<Portfolio> GetPortfolio();
        public Task<PortfolioCurrencies> GetBalance();
        public Task<decimal> GetCurrencyBalance(Currency curr);
    }
}
