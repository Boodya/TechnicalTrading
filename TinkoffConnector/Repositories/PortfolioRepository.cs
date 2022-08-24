using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Model;

namespace TinkoffConnector.Repositories
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private IContext _context;
        private string _accountId;

        public PortfolioRepository(IContext context, string accountId)
        {
            if (context == null)
                throw new Exception("Unable to Initialize repo: Empty context.");
            if (accountId == null)
                throw new Exception("Unable to Initialize repo: Empty account ID.");
            this._context = context;
            this._accountId = accountId;
        }
        public async Task<PortfolioModel> GetFullPortfolio()
        {
            var portfolio = await _context.PortfolioAsync(_accountId);
            var portfolioModel = new PortfolioModel()
            {
                FullPortfolio = portfolio,
                Currencies = (await GetBalance()).Currencies,
                Stocks = portfolio.Positions.Where(x => x.InstrumentType == InstrumentType.Stock).ToList()
            };
            portfolioModel.StringRepresentation = BuildPortfolioInfoString(portfolioModel);
            return portfolioModel;
        }
        public async Task<Portfolio> GetPortfolio()
        {
            return await _context.PortfolioAsync(_accountId);
        }
        public async Task<PortfolioCurrencies> GetBalance()
        {
            return await _context.PortfolioCurrenciesAsync(_accountId);
        }

        public async Task<decimal> GetCurrencyBalance(Currency curr)
        {
            return (await GetBalance()).Currencies
                .Where(x => x.Currency == curr).FirstOrDefault().Balance;
        }

        private string BuildPortfolioInfoString(PortfolioModel portfolio)
        {
            var resultSB = new StringBuilder($"Currencies: ");
            foreach(var curr in portfolio.Currencies)
            {
                resultSB.Append($"{curr.Currency.ToString()} " +
                    $"{curr.Balance} (Blocked {curr.Blocked}); ");
            }

            if(portfolio.Stocks.Count > 0)
                resultSB.AppendLine("Stocks: ");
            foreach(var stock in portfolio.Stocks)
            {
                resultSB.Append($"{stock.Name}({stock.Ticker}) " +
                    $"Lots {stock.Lots}, " +
                    $"Amount of stocks {stock.Balance}, PriceAverage {stock.AveragePositionPrice}, Blocked {stock.Blocked});    ");
            }
            return resultSB.ToString();
        }
    }
}
