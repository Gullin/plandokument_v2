using System.Text;
using Plandokument.Domain.Entities;
using Plandokument.Domain.Interfaces;


namespace Plandokument.Infrastructure.Repositories;

public class FileDocumentTypeRepository : IDocumentTypeRepository
{
    private readonly string _filePath;

    public FileDocumentTypeRepository(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "resources", "dokumenttyper.csv");
    }

    public async Task<List<DocumentType>> GetDocumentTypesAsync()
    {
        var documentTypes = new List<DocumentType>();

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Kunde ej hitta sökt fil: {_filePath}");
        }

        var lines = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8);
        var felrader = new List<int>();
        var felraderLogiskDatatyp = new List<int>();

        for (int i = 0; i < lines.Length; i++)
        {
            var lineParts = lines[i].Split(';');
            if (lineParts.Length != 5)
            {
                felrader.Add(i);
                continue;
            }

            if (!bool.TryParse(lineParts[4].Trim(), out var isPlanhandling))
            {
                felraderLogiskDatatyp.Add(i);
                continue;
            }

            documentTypes.Add(new DocumentType
            {
                Type = lineParts[0].Trim(),
                UrlFilter = lineParts[1].Trim(),
                Suffix = lineParts[2].Trim(),
                Description = lineParts[3].Trim(),
                IsPlanhandling = isPlanhandling
            });
        }

        if (felrader.Any())
        {
            throw new Exception($"Fel antal kolumner på raderna: {string.Join(", ", felrader)}");
        }

        if (felraderLogiskDatatyp.Any())
        {
            throw new Exception($"Kolumn 5 är inte boolean på raderna: {string.Join(", ", felraderLogiskDatatyp)}");
        }

        return documentTypes;
    }
}
