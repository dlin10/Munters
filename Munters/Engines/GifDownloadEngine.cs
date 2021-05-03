using System.Linq;
using System.Threading.Tasks;
using Munters.Dto;
using Munters.ResourceAccess;

namespace Munters.Engines
{
    internal class GifDownloadEngine : IGifDownloadEngine
    {
        private readonly IGiphyResourceAccess _giphyResourceAccess;

        public GifDownloadEngine(IGiphyResourceAccess giphyResourceAccess)
        {
            _giphyResourceAccess = giphyResourceAccess;
        }

        public async Task<string[]> GetTrendingAsync()
        {
            var giphyResponse = await _giphyResourceAccess.GetTrendingAsync();
            return ExtractUrls(giphyResponse);
        }

        public async Task<string[]> SearchAsync(string query)
        {
            var giphyResponse = await _giphyResourceAccess.SearchAsync(query);
            return ExtractUrls(giphyResponse);
        }

        private static string[] ExtractUrls(GiphyResponseRoot giphyResponse)
        {
            return giphyResponse.data.Select(d => d.images.original.url).ToArray();
        }
    }
}