using Plandokument.Domain.Entities;

namespace Plandokument.Models
{
    public class DocumentTypeInfo
    {
        public List<DocumentType> DocumentTypes { get; set; } = new();
        public int PropertyCount { get; set; }
        public Dictionary<string, string?>? PropertyDescriptions { get; set; }
        public int DocumentTypeCount { get; set; }
    }
}
