using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Munters.Engines;

namespace Munters.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GifsController : ControllerBase
    {
        private readonly IGifDownloadEngine _engine;
        private readonly ILogger<GifsController> _logger;

        public GifsController(IGifDownloadEngine engine, ILogger<GifsController> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        [HttpGet]
        public Task<string[]> Trending()
        {
            _logger.LogInformation($"{nameof(Trending)} called");
            return _engine.GetTrendingAsync();
        }

        [HttpGet]
        [ResponseCache(VaryByQueryKeys = new []{"q"}, Duration = 10)]
        public Task<string[]> Search(string q)
        {
            _logger.LogInformation($"{nameof(Search)} called. q = {q}");
            return _engine.SearchAsync(q);
        }
    }
}
