using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using JCCommon.Clients.FileServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Documents;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure;
using Scv.Api.Models;
using Scv.Api.Services;
using Scv.Db.Contants;

namespace Scv.Api.Processors;

public class JudicialBinderProcessor : BinderProcessorBase
{
    private readonly FileServicesClient _filesClient;
    private readonly IConfiguration _configuration;
    private readonly IDarsService _darsService;

    public JudicialBinderProcessor(
        FileServicesClient filesClient,
        ClaimsPrincipal currentUser,
        IValidator<BinderDto> basicValidator,
        BinderDto dto,
        IConfiguration configuration,
        IDarsService darsService) : base(currentUser, dto, basicValidator)
    {
        _filesClient = filesClient;
        _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
        _configuration = configuration;
        _darsService = darsService;
    }

    public override async Task PreProcessAsync()
    {
        await base.PreProcessAsync();

        var fileId = this.Binder.Labels.GetValue(LabelConstants.PHYSICAL_FILE_ID);
        var fileDetail = await _filesClient.FilesCivilGetAsync(
            this.CurrentUser.AgencyCode(),
            this.CurrentUser.ParticipantId(),
            _configuration.GetNonEmptyValue("Request:ApplicationCd"),
            fileId);

        // Add labels specific to Judicial Binder
        this.Binder.Labels.Add(LabelConstants.COURT_CLASS_CD, fileDetail.CourtClassCd.ToString());
        this.Binder.Labels.Add(LabelConstants.JUDGE_ID, this.CurrentUser.UserId());
    }

    public override async Task<OperationResult> ValidateAsync()
    {
        var result = await base.ValidateAsync();
        if (!result.Succeeded)
        {
            return result;
        }

        var errors = new List<string>();

        // Validate current user is accessing own binder
        var judgeId = this.Binder.Labels.GetValue(LabelConstants.JUDGE_ID);
        if (judgeId != this.CurrentUser.UserId())
        {
            errors.Add("Current user does not have access to this binder.");
            return OperationResult.Failure([.. errors]);
        }

        var fileId = this.Binder.Labels.GetValue(LabelConstants.PHYSICAL_FILE_ID);
        var fileDetail = await _filesClient.FilesCivilGetAsync(
            this.CurrentUser.AgencyCode(),
            this.CurrentUser.ParticipantId(),
            _configuration.GetNonEmptyValue("Request:ApplicationCd"),
            fileId);

        var courtSummaryIds = fileDetail.Appearance.Select(a => a.AppearanceId);
        var civilDocIds = fileDetail.Document.Select(d => d.CivilDocumentId);
        var referenceDocIds = fileDetail.ReferenceDocument.Select(r => r.ReferenceDocumentId);

        var transcriptDocs = this.Binder.Documents
            .Where(d => d.DocumentType == DocumentType.Transcript)
            .ToList();
        if (transcriptDocs.Count > 0)
        {
            try
            {
                var transcriptsResponse = await _darsService.GetCompletedDocuments(
                    physicalFileId: fileId,
                    mdocJustinNo: null,
                    returnChildRecords: true);

                if (transcriptsResponse == null)
                {
                    errors.Add("Unable to retrieve transcripts for validation.");
                }
                else
                {
                    var completedTranscripts = transcriptsResponse.ToList();

                    foreach (var transcriptDoc in transcriptDocs)
                    {
                        if (string.IsNullOrWhiteSpace(transcriptDoc.OrderId))
                        {
                            errors.Add($"Transcript document {transcriptDoc.DocumentId} is missing OrderId.");
                            continue;
                        }

                        if (!int.TryParse(transcriptDoc.OrderId, out var orderId))
                        {
                            errors.Add($"Invalid OrderId format for transcript document {transcriptDoc.DocumentId}.");
                            continue;
                        }

                        if (!int.TryParse(transcriptDoc.DocumentId, out var transcriptId))
                        {
                            errors.Add($"Invalid DocumentId format for transcript {transcriptDoc.DocumentId}.");
                            continue;
                        }

                        var matchingTranscript = completedTranscripts.FirstOrDefault(t =>
                            t.OrderId == orderId && t.Id == transcriptId);

                        if (matchingTranscript == null)
                        {
                            errors.Add($"Transcript with OrderId {orderId} and DocumentId {transcriptId} not found in completed transcripts.");
                        }
                    }
                }
            }
            catch (DARSCommon.Clients.TranscriptsServices.ApiException ex)
            {
                errors.Add($"Error validating transcripts: {ex.Message}");
            }
        }

        // Get non-transcript document IDs
        var nonTranscriptDocIds = this.Binder.Documents
            .Where(d => d.DocumentType != DocumentType.Transcript)
            .Select(d => d.DocumentId);

        // Validate that all non-transcript document ids exist in Civil Case Detail documents or reference documents
        if (nonTranscriptDocIds.Any() &&
            !nonTranscriptDocIds.All(id => courtSummaryIds.Concat(civilDocIds).Concat(referenceDocIds).Contains(id)))
        {
            errors.Add("Found one or more invalid Document IDs.");
        }

        return errors.Count != 0
            ? OperationResult.Failure([.. errors])
            : OperationResult.Success();
    }
}
