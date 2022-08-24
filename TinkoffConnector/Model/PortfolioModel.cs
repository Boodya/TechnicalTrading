using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffConnector.Model
{
    public class PortfolioModel
    {
        public Portfolio FullPortfolio { get; set; }
        public List<PortfolioCurrencies.PortfolioCurrency> Currencies { get; set; }
        public List<Portfolio.Position> Stocks { get; set; }
        public string StringRepresentation { get; internal set; }
    }
}
