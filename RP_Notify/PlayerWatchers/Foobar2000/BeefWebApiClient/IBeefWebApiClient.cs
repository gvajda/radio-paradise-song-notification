using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RP_Notify.PlayerWatchers.Foobar2000.BeefWebApiClient
{
    public interface IBeefWebApiClient
    {
        string BaseUrl { get; set; }
        bool ReadResponseAsString { get; set; }

        Task<Response> GetPlayerStateAsync(IEnumerable<string> columns);
        Task<Response> GetPlayerStateAsync(IEnumerable<string> columns, CancellationToken cancellationToken);
    }
}