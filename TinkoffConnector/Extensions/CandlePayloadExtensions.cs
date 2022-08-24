using Tinkoff.Trading.OpenApi.Models;

namespace TinkoffConnector.Extensions
{
    public static class CandlePayloadExtensions
    {
        public static decimal GetAveragePrice(this CandlePayload c)
        {
            return (c.High + c.Low) / 2;
        }
    }
}
