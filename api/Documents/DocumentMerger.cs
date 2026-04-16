using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GdPicture14;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Models.Document;

namespace Scv.Api.Documents;

public class DocumentMerger(
    IDocumentRetriever documentRetriever,
    ILogger<DocumentMerger> logger,
    IConfiguration configuration) : IDocumentMerger
{
    private const int DefaultRetrieveBatchSize = 10;
    private const string RetrieveBatchSizeKey = "DOCUMENT_RETRIEVAL_BATCH_SIZE";

    private readonly IDocumentRetriever documentRetriever = documentRetriever;
    private readonly ILogger<DocumentMerger> logger = logger;
    private readonly IConfiguration configuration = configuration;

    /// <summary>
    /// Merges multiple PDF documents into a single PDF document in base64 format.
    /// </summary>
    /// <param name="documentRequests">An array of document requests to merge documents from</param>
    /// <returns>The merge result</returns>
    public async Task<PdfDocumentResponse> MergeDocuments(PdfDocumentRequest[] documentRequests)
    {
        using GdPictureDocumentConverter gdpictureConverter = new();

        var streamsToMerge = await RetrieveStreamsInBatches(documentRequests);

        try
        {
            // Validate all streams before attempting to merge
            for (int i = 0; i < streamsToMerge.Length; i++)
            {
                var stream = streamsToMerge[i];
                if (stream == null)
                {
                    logger.LogError("Stream {Index} is null for document type {Type}", 
                        i, documentRequests[i].Type);
                    throw new InvalidOperationException($"Document stream {i} is null");
                }

                if (stream.Length == 0)
                {
                    logger.LogError("Stream {Index} is empty for document type {Type}", 
                        i, documentRequests[i].Type);
                    throw new InvalidOperationException($"Document stream {i} is empty");
                }

                logger.LogInformation("Stream {Index} validated - Type: {Type}, Size: {Size} bytes", 
                    i, documentRequests[i].Type, stream.Length);
            }

            var pageRanges = new List<PageRange>();
            var streamsForMerge = new List<MemoryStream>(streamsToMerge.Length);
            int currentPage = 0;

            try
            {
                for (int i = 0; i < streamsToMerge.Length; i++)
                {
                    var docStream = streamsToMerge[i];
                    docStream.Position = 0;
                    using var pdf = new GdPicturePDF();

                    var loadStatus = pdf.LoadFromStream(docStream, true);
                    if (loadStatus != GdPictureStatus.OK)
                    {
                        logger.LogError(
                            "GdPicture failed to load stream {Index} (Type: {Type}) with status: {Status}",
                            i, documentRequests[i].Type, loadStatus);
                        throw new InvalidOperationException(
                            $"Failed to load document stream {i} (Type: {documentRequests[i].Type}): {loadStatus}");
                    }

                    int pageCount = pdf.GetPageCount();
                    if (pageCount <= 0)
                    {
                        logger.LogError(
                            "Stream {Index} (Type: {Type}) reported {PageCount} pages after loading",
                            i, documentRequests[i].Type, pageCount);
                        throw new InvalidOperationException(
                            $"Document stream {i} (Type: {documentRequests[i].Type}) contains no pages");
                    }

                    pageRanges.Add(new PageRange { Start = currentPage, End = currentPage + pageCount });
                    currentPage += pageCount;

                    var flattenStatus = pdf.FlattenFormFields();
                    if (flattenStatus != GdPictureStatus.OK)
                    {
                        logger.LogWarning(
                            "FlattenFormFields on stream {Index} (Type: {Type}) returned status: {Status} — continuing",
                            i, documentRequests[i].Type, flattenStatus);
                    }

                    var flattenOcgStatus = pdf.FlattenVisibleOCGs();
                    if (flattenOcgStatus != GdPictureStatus.OK)
                    {
                        logger.LogWarning(
                            "FlattenVisibleOCGs on stream {Index} (Type: {Type}) returned status: {Status} — continuing",
                            i, documentRequests[i].Type, flattenOcgStatus);
                    }

                    var flattenedDocStream = new MemoryStream();
                    var saveStatus = pdf.SaveToStream(flattenedDocStream);
                    if (saveStatus != GdPictureStatus.OK)
                    {
                        flattenedDocStream.Dispose();
                        logger.LogError(
                            "GdPicture failed to save flattened stream {Index} (Type: {Type}) with status: {Status}",
                            i, documentRequests[i].Type, saveStatus);
                        throw new InvalidOperationException(
                            $"Failed to save flattened document stream {i} (Type: {documentRequests[i].Type}): {saveStatus}");
                    }

                    flattenedDocStream.Position = 0;
                    streamsForMerge.Add(flattenedDocStream);

                    pdf.CloseDocument();
                }

                MemoryStream outputStream = new();
                var mergeResult = gdpictureConverter.CombineToPDF([.. streamsForMerge], outputStream, PdfConformance.PDF);
                if (mergeResult != GdPictureStatus.OK)
                {
                    logger.LogError("GdPicture merge failed with status: {Status}. Document count: {Count}",
                        mergeResult, streamsForMerge.Count);
                    throw new InvalidOperationException($"Failed to merge documents: {mergeResult}");
                }

                logger.LogInformation("Merged {TotalCount} documents.", streamsForMerge.Count);

                outputStream.Position = 0;

                return new PdfDocumentResponse
                {
                    Base64Pdf = Convert.ToBase64String(outputStream.ToArray()),
                    PageRanges = pageRanges
                };
            }
            finally
            {
                // Ensure to dispose all flattened streams created during merge preparation
                var disposeFlattenedTasks = streamsForMerge
                    .Select(s => s.DisposeAsync().AsTask());

                await Task.WhenAll(disposeFlattenedTasks);
            }
        }
        finally
        {
            // Ensure all retrieved streams are disposed
            var disposeTasks = streamsToMerge
                .Where(x => x is not null)
                .Select(stream => stream.DisposeAsync().AsTask());

            await Task.WhenAll(disposeTasks);
        }
    }

    private async Task<MemoryStream[]> RetrieveStreamsInBatches(PdfDocumentRequest[] documentRequests)
    {
        var streams = new MemoryStream[documentRequests.Length];
        var configuredValue = configuration.GetValue<string>(RetrieveBatchSizeKey);
        var batchSize = int.TryParse(configuredValue, out var parsedBatchSize) && parsedBatchSize > 0
            ? parsedBatchSize
            : DefaultRetrieveBatchSize;
        
        for (var batchStart = 0; batchStart < documentRequests.Length; batchStart += batchSize)
        {
            var batchCount = Math.Min(batchSize, documentRequests.Length - batchStart);

            logger.LogInformation(
                "Retrieving document batch starting at index {BatchStart} with {BatchCount} documents.",
                batchStart,
                batchCount);

            var retrieveTasks = Enumerable
                .Range(batchStart, batchCount)
                .Select(index => RetrieveStream(index, documentRequests[index]));

            var batchResults = await Task.WhenAll(retrieveTasks);

            foreach (var (index, stream) in batchResults)
            {
                streams[index] = stream;
            }
        }

        return streams;
    }

    private async Task<(int Index, MemoryStream Stream)> RetrieveStream(int index, PdfDocumentRequest documentRequest)
    {
        var stream = await documentRetriever.Retrieve(documentRequest);
        return (index, stream);
    }
}