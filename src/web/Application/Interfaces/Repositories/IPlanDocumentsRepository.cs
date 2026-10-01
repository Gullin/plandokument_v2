using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanDocumentsRepository
{
    Task<IEnumerable<PlanDocuments>> GetPlanDocumentsAsync(CancellationToken cancellationToken = default);
}