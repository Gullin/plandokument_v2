using Plandokument.Domain.Entities;

namespace Plandokument.Models
{
    public class DocumentTypeInfo
    {
        public IEnumerable<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
        public int PropertyCount { get; set; }
        public Dictionary<string, string?>? PropertyDescriptions { get; set; }
        public int DocumentTypeCount { get; set; }
    }
}
