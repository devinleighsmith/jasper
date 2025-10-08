using System;
using System.Collections.Generic;
using System.Linq;
using Scv.Api.Models.Criminal.Detail;
using Scv.Db.Models;

namespace Scv.Api.Helpers.Documents;

/// <summary>
/// Provides methods to resolve key criminal documents based on specific categories and dispositions.
/// </summary>
public static class KeyDocumentResolver
{
    private static readonly string _perfected = "PERFECTED";
    private static readonly string _bail = "BAIL";

    /// <summary>
    /// Retrieves key criminal documents from the provided collection based on predefined categories and perfected bail documents.
    /// </summary>
    /// <param name="documents">An enumerable collection of <see cref="CriminalDocument"/> objects to filter.</param>
    /// <returns>
    /// An <see cref="IEnumerable{CriminalDocument}"/> containing documents that match the key categories ("ROP", "INITIATING")
    /// and the most recent perfected bail document, if available. Returns <c>default</c> if the input collection is empty.
    /// </returns>
    public static IEnumerable<CriminalDocument> GetCriminalKeyDocuments(IEnumerable<CriminalDocument> documents)
    {
        if (!documents.Any())
        {
            return [];
        }

        // Get PSR document (most recent)
        var psrDoc = documents
            .Where(d => (d.Category?.ToUpper() ?? d.DocmClassification?.ToUpper()) == DocumentCategory.PSR)
            .OrderByDescending(d => DateTime.TryParse(d.IssueDate, out var date) ? date : DateTime.MinValue)
            .FirstOrDefault();

        // Get other key documents (excluding PSR)
        var otherKeyDocs = documents
            .Where(d =>
            DocumentCategory.KEY_DOCUMENT_CATEGORIES.Contains(d.Category?.ToUpper() ?? d.DocmClassification?.ToUpper()) &&
            (d.Category?.ToUpper() ?? d.DocmClassification?.ToUpper()) != DocumentCategory.PSR);

        // Get most recent perfected bail document
        var bailDoc = documents
            .Where(d =>
            ((d.Category?.ToUpper() == _bail) || (d.DocmClassification?.ToUpper() == _bail)) &&
            d.DocmDispositionDsc.Equals(_perfected, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => DateTime.TryParse(d.IssueDate, out var date) ? date : DateTime.MinValue)
            .FirstOrDefault();

        return new[] { psrDoc }
            .Where(d => d != null)
            .Concat(otherKeyDocs)
            .Concat(bailDoc != null ? [bailDoc] : Array.Empty<CriminalDocument>());
    }
}