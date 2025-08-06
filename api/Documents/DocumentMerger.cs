using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GdPicture14;
using Microsoft.AspNetCore.WebUtilities;
using Scv.Api.Models.Document;
using Scv.Api.Services.Files;

namespace Scv.Api.Documents;

public class DocumentMerger(FilesService filesService) : IDocumentMerger
{
    private readonly FilesService _filesService = filesService;
    /// <summary>
    /// Merges multiple PDF documents into a single PDF document in base64 format.
    /// </summary>
    /// <param name="documentRequests">An array of document requests to merge documents from</param>
    /// <returns>The merge result</returns>
    public async Task<PdfDocumentResponse> MergeDocuments(PdfDocumentRequest[] documentRequests)
    {
        PdfDocumentResponse response = new();
        List<Stream> streamsToMerge = [];

        using GdPictureDocumentConverter gdpictureConverter = new();

        // Retrieve all document streams to merge
        foreach (var documentRequest in documentRequests)
        {
            var documentResponseStreamCopy = new MemoryStream();

            // Here we will call the correct strategy,
            // which then calls the appropriate api based off the documentRequest type
            var documentId = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(documentRequest.Data.DocumentId));
            var documentResponse = await _filesService.DocumentAsync(documentId, true, documentRequest.Data.FileId, documentRequest.Data.CorrelationId);
            await documentResponse.Stream.CopyToAsync(documentResponseStreamCopy);
            // Is this necessary?
            documentResponseStreamCopy.Position = 0;
            streamsToMerge.Add(documentResponseStreamCopy);
        }
        
        MemoryStream outputStream = new();

        var mergeResult = gdpictureConverter.CombineToPDF(streamsToMerge, outputStream, PdfConformance.PDF);
        if (mergeResult != GdPictureStatus.OK)
        {
            throw new Exception($"Failed to merge documents: {mergeResult}");
        }
        
        var pageCounts = new List<int>();
        foreach (var docStream in streamsToMerge)
        {
            using var pdf = new GdPicturePDF();
            pdf.LoadFromStream(docStream, true);
            //pdf.GetPageCount();
            pageCounts.Add(pdf.GetPageCount());
            pdf.CloseDocument();
        }

        // Calculate start and end page indices for each document
        var pageRanges = new List<(int Start, int End)>();
        int currentPage = 0;
        foreach (var count in pageCounts)
        {
            int start = currentPage;
            int end = currentPage + count;
            pageRanges.Add((start, end));
            currentPage = end;
        }

        outputStream.Position = 0;
        string base64Pdf;
        using var ms = new MemoryStream();
        await outputStream.CopyToAsync(ms);
        base64Pdf = Convert.ToBase64String(ms.ToArray());

        response.Base64Pdf = base64Pdf;
        response.PageRanges.AddRange(pageRanges);

        return response;
    }
}