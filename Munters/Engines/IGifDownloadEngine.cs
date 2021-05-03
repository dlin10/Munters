using System.Threading.Tasks;

namespace Munters.Engines
{
    public interface IGifDownloadEngine
    {
        Task<string[]> GetTrendingAsync();

        Task<string[]> SearchAsync(string query);
    }
}
