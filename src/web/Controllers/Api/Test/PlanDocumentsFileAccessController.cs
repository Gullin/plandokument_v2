using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Plandokument.Application.Options.PlanDocumentFiles;

namespace Plandokument.Controllers.Api.Test;

[Route("api/test/[controller]")]
[ApiController]
public class PlanDocumentsFileAccessController : ControllerBase
{
    private readonly PlanDocumentFiles _options;

    public PlanDocumentsFileAccessController(IOptions<PlanDocumentFiles> options)
    {
        _options = options.Value;
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> Get()
    {
        //var _rootPath = _options.RootPath;
        //if (!System.IO.Directory.Exists(_rootPath))
        //    return NotFound();

        //var files = Directory.GetFiles(_rootPath, "*.pdf", SearchOption.AllDirectories);
        //return Ok(files.ToList());

        //var contentType = "application/pdf"; // ev. använd FileExtensionContentTypeProvider
        //return PhysicalFile(fullPath, contentType);
        throw new NotImplementedException();
    }
}
