using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Services;

public interface IDocumentTypeService
{
    Task<List<DocumentType>> GetAllAsync();

    int GetPropertyCount();

    Dictionary<string, string?> GetPropertyDescriptions();

    Task<int> GetDocumentTypeCountAsync();
}