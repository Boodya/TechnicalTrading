using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using TinkoffConnector.Connectors;

namespace TinkoffConnector.Factories
{
    public class ConnectionFactory
    {
        public static async Task<T> GetConnector<T>()
        {
            if (typeof(T) == typeof(SandboxConnector))
            {
                var instance = new SandboxConnector(StartupSettings.AppSettings.SandBoxAuthToken);
                await instance.RegisterAccount();
                await instance.SetRandomBalanceAsync(new List<Currency>()
                {
                    Currency.Usd, Currency.Rub, Currency.Eur
                });
                return (T)(object)instance;
            }
            else if (typeof(T) == typeof(LocalDBConnector))
            {
                var instance = new LocalDBConnector(StartupSettings.AppSettings.SandBoxAuthToken);
                await instance.RegisterAccount();
                await instance.SetRandomBalanceAsync(new List<Currency>()
                {
                    Currency.Usd, Currency.Rub, Currency.Eur
                });

                return (T)(object)instance;
            }
            else if(typeof(T) == typeof(StockConnector))
            {
                var instance = new SandboxConnector(StartupSettings.AppSettings.AuthToken);
                await instance.RegisterAccount();
                return (T)(object)instance;
            }
            else
            {
                throw new Exception("That type of connector not defined");
            }
        }
    }
}
