using System.Threading.Tasks;
using Munters.Dto;

namespace Munters.ResourceAccess
{
    public interface IGiphyResourceAccess
    {
        Task<GiphyResponseRoot> GetTrendingAsync();

        Task<GiphyResponseRoot> SearchAsync(string query);
    }
}