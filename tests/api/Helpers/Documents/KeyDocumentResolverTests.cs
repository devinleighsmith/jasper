using System.Collections.Generic;
using System.Linq;
using Scv.Api.Helpers.Documents;
using Scv.Api.Models.Criminal.Detail;
using Xunit;

namespace tests.api.Helpers.Documents;

public class KeyDocumentResolverTest
{
    [Fact]
    public void GetCriminalKeyDocuments_ReturnsEmpty_WhenNoDocuments()
    {
        // Arrange
        var documents = Enumerable.Empty<CriminalDocument>();

        // Act
        var result = KeyDocumentResolver.GetCriminalKeyDocuments(documents);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCriminalKeyDocuments_ReturnsKeyDocuments_ByCategory()
    {
        // Arrange
        var documents = new List<CriminalDocument>
        {
            new() { Category = "ROP", IssueDate = "2024-01-01" },
            new() { Category = "INITIATING", IssueDate = "2024-01-02" },
            new() { Category = "OTHER", IssueDate = "2024-01-03" }
        };

        // Act
        var result = KeyDocumentResolver.GetCriminalKeyDocuments(documents);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, d => d.Category == "ROP");
        Assert.Contains(resultList, d => d.Category == "INITIATING");
    }

    [Fact]
    public void GetCriminalKeyDocuments_ReturnsPerfectedBailDocument()
    {
        // Arrange
        var documents = new List<CriminalDocument>
        {
            new() { Category = "BAIL", DocmDispositionDsc = "Perfected", IssueDate = "2024-01-05" },
            new() { Category = "BAIL", DocmDispositionDsc = "Not Perfected", IssueDate = "2024-01-04" }
        };

        // Act
        var result = KeyDocumentResolver.GetCriminalKeyDocuments(documents);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Perfected", resultList[0].DocmDispositionDsc);
    }

    [Fact]
    public void GetCriminalKeyDocuments_ReturnsKeyDocumentsAndPerfectedBail()
    {
        // Arrange
        var documents = new List<CriminalDocument>
        {
            new() { Category = "ROP", IssueDate = "2024-01-01" },
            new() { Category = "BAIL", DocmDispositionDsc = "Perfected", IssueDate = "2024-01-02" },
            new() { Category = "OTHER", IssueDate = "2024-01-03" }
        };

        // Act
        var result = KeyDocumentResolver.GetCriminalKeyDocuments(documents);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, d => d.Category == "ROP");
        Assert.Contains(resultList, d => d.Category == "BAIL" && d.DocmDispositionDsc == "Perfected");
    }

    [Fact]
    public void GetCriminalKeyDocuments_HandlesNullCategoryAndClassification()
    {
        // Arrange
        var documents = new List<CriminalDocument>
        {
            new() { Category = null, DocmClassification = "ROP", IssueDate = "2024-01-01" },
            new() { Category = null, DocmClassification = "BAIL", DocmDispositionDsc = "Perfected", IssueDate = "2024-01-02" }
        };

        // Act
        var result = KeyDocumentResolver.GetCriminalKeyDocuments(documents);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, d => d.DocmClassification == "ROP");
        Assert.Contains(resultList, d => d.DocmClassification == "BAIL" && d.DocmDispositionDsc == "Perfected");
    }
}