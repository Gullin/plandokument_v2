using Plandokument.Domain.Entities;

namespace Plandokument.Domain.Interfaces
{
    public interface IDocumentTypeRepository
    {
        Task<List<DocumentType>> GetDocumentTypesAsync();
    }
}
