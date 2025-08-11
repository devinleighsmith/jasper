using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Scv.Api.Documents.Strategies;
using Scv.Api.Models.Document;

namespace Scv.Api.Documents;

public class DocumentRetriever(IEnumerable<IDocumentStrategy> strategies) : IDocumentRetriever
{
    private readonly IEnumerable<IDocumentStrategy> strategies = strategies;

    /// <summary>
    /// Retrieves a PDF stream from a given source based off the document-request type.
    /// </summary>
    /// <param name="documentRequest">The document request containing the source information</param>
    /// <returns>The retrieved PDF document stream</returns>
    public Task<MemoryStream> Retrieve(PdfDocumentRequest documentRequest)
    {
        if (!Enum.TryParse<DocumentType>(documentRequest.Type, ignoreCase: true, out var type))
            throw new ArgumentException($"Invalid document type: {documentRequest.Type}");

        return strategies.FirstOrDefault(strat => strat.Type == type)?.Invoke(documentRequest.Data)
            ?? throw new InvalidOperationException();
    }
}