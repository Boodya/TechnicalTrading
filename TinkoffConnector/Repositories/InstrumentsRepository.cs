using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffConnector.Repositories
{
    public class InstrumentsRepository : IInstrumentsRepository
    {
        private static readonly Random _rand = new Random();
        private IContext _context;
        private List<MarketInstrument> _allInstruments;

        public InstrumentsRepository(IContext context)
        {
            Initialize(context);
        }
        public void Initialize(IContext context)
        {
            if (context == null)
                throw new Exception("Unable to Initialize repo: Empty context.");
            _context = context;
        }
        public async Task<List<MarketInstrument>> GetAllInstruments()
        {
            var stock = await _context.MarketStocksAsync();
            return stock.Instruments;
        }
        public async Task InitializeInstruments()
        {
            _allInstruments = await GetAllInstruments();
        }
        public void SetInstruments(List<MarketInstrument> instruments)
        {
            _allInstruments = instruments;
        }

        public MarketInstrument GetInstrumentByTicker(string ticker)
        {
            return _allInstruments.FirstOrDefault(x => x.Ticker == ticker);
        }

        public MarketInstrument GetInstrumentByName(string name)
        {
            return _allInstruments.FirstOrDefault(x => x.Name == name);
        }

        public MarketInstrument GetRandomInstrument()
        {
            return _allInstruments[_rand.Next(_allInstruments.Count)];
        }

        public MarketInstrument GetRandomInstrumentByCurrency(Currency currency)
        {
            var availableInstruments = _allInstruments.FindAll(x => x.Currency.ToString() == currency.ToString());
            return availableInstruments[_rand.Next(availableInstruments.Count)];
        }

        public List<MarketInstrument> GetInstrumentsByCurrency(Currency currency)
        {
            return _allInstruments.FindAll(x => x.Currency.ToString() == currency.ToString());
        }

        public MarketInstrument GetInstrumentByFigi(string figi)
        {
            return _allInstruments.FirstOrDefault(x => x.Figi == figi);
        }
    }
}
