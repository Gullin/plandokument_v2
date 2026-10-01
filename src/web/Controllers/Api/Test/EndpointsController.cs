using Microsoft.AspNetCore.Mvc;

namespace Plandokument.Controllers.Api.Test
{
    [Route("api/test/[controller]")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private readonly IEnumerable<EndpointDataSource> _endpointSources;

        public EndpointsController(IEnumerable<EndpointDataSource> endpointSources)
        {
            _endpointSources = endpointSources;
        }

        [HttpGet]
        public IActionResult Get()
        {
            //var endpoints = _endpointSources
            //    .SelectMany(source => source.Endpoints)
            //    //.Select(e => e.DisplayName ?? e.ToString());
            //    .Select(e => e.ToString());

            //return Ok(string.Join("\n", endpoints));
            var routes = _endpointSources
                            .SelectMany(s => s.Endpoints)
                            .OfType<RouteEndpoint>()
                            .Select(e =>
                            {
                                var raw = e.RoutePattern.RawText ?? "";
                                return raw.StartsWith("/") ? raw : "/" + raw;
                            })
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(r => r)
                            .ToArray();

            // För JSON-array, return Ok(routes);
            return Ok(string.Join("\n", routes));
        }
    }

}
