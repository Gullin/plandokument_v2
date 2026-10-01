using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api;

/// <summary>
/// Listar giltiga dokumenttyper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DocumentTypesController : ControllerBase
{
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentTypesController(IDocumentTypeService documentTypeService)
    {
        _documentTypeService = documentTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentType>>> Get()
    {
        var result = await _documentTypeService.GetAllAsync();
        return Ok(result);
    }
}
