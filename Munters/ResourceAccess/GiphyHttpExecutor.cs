using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Munters.Dto;

namespace Munters.ResourceAccess
{
    /// <summary>
    /// <see cref="HttpExecutor"/> extension for Giphy REST API 
    /// </summary>
    internal class GiphyHttpExecutor : HttpExecutor, IGiphyResourceAccess
    {
        private readonly IOptions<GifDownloadOptions> _gifDownloadOptions;

        public GiphyHttpExecutor(IOptions<GifDownloadOptions> gifDownloadOptions)
        {
            _gifDownloadOptions = gifDownloadOptions;
        }

        protected override string BaseAddress => _gifDownloadOptions.Value.BaseAddress;

        public Task<GiphyResponseRoot> GetTrendingAsync()
        {
            return Get<GiphyResponseRoot>(_gifDownloadOptions.Value.TrendingUrl, ApiKeyQueryParam);
        }

        public Task<GiphyResponseRoot> SearchAsync(string query)
        {
            var queryParam = new KeyValuePair<string, string>("q", query);
            return Get<GiphyResponseRoot>(_gifDownloadOptions.Value.SearchUrl, queryParam, ApiKeyQueryParam);
        }

        private KeyValuePair<string, string> ApiKeyQueryParam => new KeyValuePair<string, string>("api_key", _gifDownloadOptions.Value.ApiKey);
    }
}