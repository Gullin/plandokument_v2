using Plandokument.Domain.Entities;
using Plandokument.Domain.Search;

namespace Plandokument.Application.Interfaces.Services;

public interface IPlanDocumentsService
{
    Task<IEnumerable<PlanDocuments>> GetAllDocumentsAsync();

    Task<IEnumerable<PlanFileResult>> GetAllDocumentsWithPlansAsync();

    Task<IEnumerable<PlanFileResult>> GetDocumentsBySpecificPlansAsync(List<string> planIds, string documentType = "dokument");
}
