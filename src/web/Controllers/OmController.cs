using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Services;
using Plandokument.Models;

namespace Plandokument.Controllers;

public class OmController : BasController
{
    private readonly IDocumentTypeService _documentTypeService;

    public OmController(GeneralInfoService generalInfoService, IDocumentTypeService documentTypeService) : base(generalInfoService) { 
        _documentTypeService = documentTypeService;
    }


    [HttpGet("dokument/om", Name = "Om")]
    [HttpGet("dokument/help")]
    [HttpGet("dokument/hjalp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Index()
    {
        result.DocumentTypeInfo = new DocumentTypeInfo
        {
            DocumentTypes = await _documentTypeService.GetAllAsync(),
            PropertyCount = _documentTypeService.GetPropertyCount(),
            PropertyDescriptions = _documentTypeService.GetPropertyDescriptions(),
            DocumentTypeCount = await _documentTypeService.GetDocumentTypeCountAsync()
        };
        return View("Views/Om.cshtml", result);
    }
}
