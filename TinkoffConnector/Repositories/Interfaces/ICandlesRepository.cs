using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TinkoffConnector.Model;

namespace TinkoffConnector.Repositories
{
    public interface ICandlesRepository : IAsyncRepository
    {
        public void Initialize(IContext context, string accountId);
        public Task<decimal?> GetCurrentInstrumentPrice(MarketInstrument instrument, DateTime? dateFor = null);
        public Task<List<CandleModel>> GetInstrumentCandles(MarketInstrument instrument,
            DateTime from, DateTime to, CandleInterval? interval = null, bool isUseHistory = true);
    }
}
