using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Scv.Api.Documents;
using Scv.Api.Documents.Strategies;
using Scv.Api.Models.Document;
using Xunit;

namespace tests.api.Documents;

public class DocumentRetrieverTest
{
    [Fact]
    public async Task Retrieve_ValidType_StrategyInvoked_ReturnsMemoryStream()
    {
        var expectedStream = new MemoryStream();
        var requestData = new PdfDocumentRequestDetails
        {
        };
        var request = new PdfDocumentRequest
        {
            Type = DocumentType.File.ToString(),
            Data = requestData
        };

        var strategyMock = new Mock<IDocumentStrategy>();
        strategyMock.SetupGet(s => s.Type).Returns(DocumentType.File);
        strategyMock.Setup(s => s.Invoke(requestData)).ReturnsAsync(expectedStream);

        var retriever = new DocumentRetriever([strategyMock.Object]);

        var result = await retriever.Retrieve(request);

        Assert.Same(expectedStream, result);
        strategyMock.Verify(s => s.Invoke(requestData), Times.Once);
    }

    [Fact]
    public async Task Retrieve_InvalidType_ThrowsArgumentException()
    {
        var request = new PdfDocumentRequest
        {
            Type = "NonExistentType",
            Data = new PdfDocumentRequestDetails { }
        };

        var retriever = new DocumentRetriever(Array.Empty<IDocumentStrategy>());

        await Assert.ThrowsAsync<ArgumentException>(() => retriever.Retrieve(request));
    }

    [Fact]
    public async Task Retrieve_NoMatchingStrategy_ThrowsInvalidOperationException()
    {
        var request = new PdfDocumentRequest
        {
            Type = DocumentType.File.ToString(),
            Data = new PdfDocumentRequestDetails { }
        };

        var strategyMock = new Mock<IDocumentStrategy>();
        strategyMock.SetupGet(s => s.Type).Returns((DocumentType)99); // Not matching

        var retriever = new DocumentRetriever([strategyMock.Object]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => retriever.Retrieve(request));
    }
}