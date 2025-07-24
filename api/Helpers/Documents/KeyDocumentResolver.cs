using System;
using System.Collections.Generic;
using System.Linq;
using Scv.Api.Models.Criminal.Detail;

namespace Scv.Api.Helpers.Documents;

public class KeyDocumentResolver
{
    // Hardcoded for now, will be eventually replaced with values from PCSS
    private static readonly string[] _keyDocumentCategories = ["ROP", "INITIATING"];

    public static IEnumerable<CriminalDocument> GetCriminalKeyDocuments(IEnumerable<CriminalDocument> documents)
    {
        if (!documents.Any())
        {
            return default;
        }
        var nonBails = documents.Where(dmt => _keyDocumentCategories.Contains(dmt.Category?.ToUpper() ?? dmt.DocmClassification?.ToUpper()));
        var bails = documents
            .OrderBy(dmt =>
            {
                return DateTime.TryParse(dmt.IssueDate, out DateTime date) ? date : DateTime.MinValue;
            })
            .FirstOrDefault(dmt =>
                (
                    ((dmt.Category?.ToUpper() == "BAIL") || (dmt.DocmClassification?.ToUpper() == "BAIL")) &&
                    dmt.DocmDispositionDsc.Equals("Perfected", StringComparison.OrdinalIgnoreCase)
                )
            );
        return nonBails.Concat(bails is not null ? [bails] : []);
    }
}