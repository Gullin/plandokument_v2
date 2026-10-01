using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api
{
    [Route("api/documents")]
    [ApiController]
    public class PlanDocumentsController : ControllerBase
    {
        private readonly IPlanDocumentsService _planDocumentsService;

        public PlanDocumentsController(IPlanDocumentsService planDocumentsService)
        {
            _planDocumentsService = planDocumentsService;
        }


        /// <summary>
        /// Returnerar alla plandokument.
        /// </summary>
        /// <returns>Alla plandokument som lista av FileInfor för resp. rootkatalog</returns>
        [HttpGet("all")]
        public async Task<ActionResult> GetAllDocuments()
        {
            var result = await _planDocumentsService.GetAllDocumentsAsync();
            return Ok(result);
        }


        /// <summary>
        /// Returnerar alla planers dokument med planinformation.
        /// </summary>
        /// <returns>Lista med planer och dess dokument</returns>
        [HttpGet("with-plans")]
        public async Task<ActionResult> GetAllDocumentsWithPlans()
        {
            var result = await _planDocumentsService.GetAllDocumentsWithPlansAsync();
            return Ok(result);
        }


        /// <summary>
        /// Returnerar sökta planers dokument med planinformaton samt begränsar till ev dokumenttyp.
        /// </summary>
        /// <param name="planIds">Kommaseparerad lista med plannycklar</param>
        /// <param name="documentType">Dokumenttyper. Dokument som standard om inget värde anges. Handling ger alla planhandlingar.</param>
        /// <returns>Lista med planer och dess dokument</returns>
        // 2:a utvärderingen: Tillåten route, men Swagger UI presenterar den som obligatorisk. Därför två alternativa routes
        //[Route("{documentType?}/by-plans/{planIds}")]
        // 1:a utvärderingen: Borde fungera men trodde inte den frivilliga routen accepterades p.g.a. att routen var definierad i HttpGet-attributet
        //[HttpGet("{documentType?}/by-plans/{planIds}")]
        [HttpGet("by-plans/{planIds}")]
        [HttpGet("{documentType}/by-plans/{planIds}")]
        public async Task<ActionResult> GetDocumentsBySpecificPlans(string planIds, string? documentType = "dokument")
        {
            var planIdList = planIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = await _planDocumentsService.GetDocumentsBySpecificPlansAsync(planIdList, documentType);
            return Ok(result);
        }
    }
}
