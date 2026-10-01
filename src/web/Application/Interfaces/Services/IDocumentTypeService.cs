using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Services;

public interface IDocumentTypeService
{
    Task<List<DocumentType>> GetAllAsync();
}