using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Services;

public interface IPlanGeometriesAllAsGeoJSONService
{
    Task<IEnumerable<PlanGeoJSON>> GetAllAsync();
}
