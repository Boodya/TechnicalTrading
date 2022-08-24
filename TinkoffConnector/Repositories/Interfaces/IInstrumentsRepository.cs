using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffConnector.Repositories
{
    public interface IInstrumentsRepository : IAsyncRepository
    {
        public void Initialize(IContext context);
        public Task<List<MarketInstrument>> GetAllInstruments();
        public Task InitializeInstruments();
        public void SetInstruments(List<MarketInstrument> instruments);
        public MarketInstrument GetInstrumentByTicker(string ticker);
        public MarketInstrument GetInstrumentByName(string name);
        public MarketInstrument GetInstrumentByFigi(string figi);
        public MarketInstrument GetRandomInstrument();
        public MarketInstrument GetRandomInstrumentByCurrency(Currency currency);
        public List<MarketInstrument> GetInstrumentsByCurrency(Currency currency);
    }
}
