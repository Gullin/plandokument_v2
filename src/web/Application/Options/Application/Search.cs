namespace Plandokument.Application.Options.Application;

public class Search
{
    public string URLQueryStringSeparator { get; set; } = ",";
    public string URLParcelBlockUnitSign { get; set; } = "_";
    public string UrlParameterSearchString { get; set; } = "q";
    public string UrlParameterDocumentType { get; set; } = "dokument";
    public string UrlParameterSearchType { get; set; } = "begrepp";
}
