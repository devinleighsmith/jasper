using System.Collections.Generic;

namespace Scv.Api.Models.Document;

public class PdfDocumentResponse
{
    public string Base64Pdf { get; set; }
    public List<(int, int)> PageRanges { get; set; } = [];
}