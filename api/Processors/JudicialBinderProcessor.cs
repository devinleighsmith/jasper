using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using JCCommon.Clients.FileServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using MapsterMapper;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure;
using Scv.Api.Models;
using Scv.Db.Contants;
using LazyCache;
using System;
using Scv.Db.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Scv.Api.Processors;

public class JudicialBinderProcessor : BinderProcessorBase
{
    private readonly FileServicesClient _filesClient;
    private readonly IAppCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public JudicialBinderProcessor(
        FileServicesClient filesClient,
        ClaimsPrincipal currentUser,
        IValidator<BinderDto> basicValidator,
        BinderDto dto,
        IAppCache cache,
        IConfiguration configuration,
        IMapper mapper) : base(currentUser, dto, basicValidator)
    {
        _filesClient = filesClient;
        _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
        _configuration = configuration;
        _cache = cache;
        _mapper = mapper;
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

    public override async Task<OperationResult> ProcessAsync()
    {
        if (this.Binder.Id != null)
        {
            var documents = await GetDocuments();
            Binder.Documents = documents;
            return OperationResult.Success();
        }
        return OperationResult.Failure("Binder does not exist.");
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

        // Validate that all document ids from Dto exist in Civil Case Detail documents or reference documents
        var docIdsFromDto = this.Binder.Documents.Select(d => d.DocumentId);
        if (!docIdsFromDto.All(id => courtSummaryIds.Concat(civilDocIds).Concat(referenceDocIds).Contains(id)))
        {
            errors.Add("Found one or more invalid Document IDs.");
        }

        return errors.Count != 0
            ? OperationResult.Failure([.. errors])
            : OperationResult.Success();
    }

    private async Task<List<BinderDocumentDto>> GetDocuments()
    {
        var fileId = this.Binder.Labels.GetValue(LabelConstants.PHYSICAL_FILE_ID);
        var participantId = this.Binder.Labels.GetValue(LabelConstants.PARTICIPANT_ID);

        async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilGetAsync(
            this.CurrentUser.AgencyCode(),
            this.CurrentUser.ParticipantId(),
            _configuration.GetNonEmptyValue("Request:ApplicationCd"),
            fileId);
        async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(CurrentUser.AgencyCode(), CurrentUser.ParticipantId(), _configuration.GetNonEmptyValue("Request:ApplicationCd"), null, null, null, null, fileId);
        var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}-{CurrentUser.AgencyCode()}", FileContent);
        var fileDetailsTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{CurrentUser.AgencyCode()}", FileDetails);
        var fileDetails = await fileDetailsTask;
        var fileContent = await fileContentTask;

        // Pass existing binder documents if available, otherwise use empty list
        var existingBinderDocs = Binder.Documents ?? [];
        var csrDocs = PopulateDetailCsrsDocuments([.. fileDetails.Appearance.Where(a => existingBinderDocs.Select(doc => doc.DocumentId).Contains(a.AppearanceId))]);
        var referenceDocs = PopulateDetailReferenceDocuments([.. fileDetails.ReferenceDocument.Where(rd => existingBinderDocs.Select(doc => doc.DocumentId).Contains(rd.ReferenceDocumentId))]);
        var documents = fileContent.CivilFile.SelectMany(cf => cf.Document);
        var matchingDocuments = documents.Where(docContent => existingBinderDocs.Any(bd => bd.DocumentId == docContent.DocumentId)).ToList();
        
        var fileContentCivilFile = fileContent.CivilFile?.First(cf => cf.PhysicalFileID == fileId);
        if(fileContentCivilFile != null)
        {
            Binder.Labels.TryAdd(LabelConstants.COURT_LEVEL_CD, fileContentCivilFile.CourtLevelCd);
            Binder.Labels.TryAdd(LabelConstants.COURT_CLASS_CD, fileContentCivilFile.CourtClassCd);
        }

        return _mapper.Map<List<BinderDocumentDto>>(matchingDocuments.Concat(csrDocs).Concat(referenceDocs));
    }

    private static IEnumerable<CvfcDocument> PopulateDetailReferenceDocuments(ICollection<CvfcRefDocument3> referenceDocuments)
    {
        //Add in Reference Documents.
        return referenceDocuments.Select(referenceDocument => new CvfcDocument()
        {
            DocumentTypeCd = DocumentCategory.REFERENCE,
            DocumentTypeDescription = referenceDocument.ReferenceDocumentTypeDsc,
            DocumentId = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(referenceDocument.ObjectGuid)),
            ImageId = referenceDocument.ObjectGuid,
        });
    }

    private static IEnumerable<CvfcDocument> PopulateDetailCsrsDocuments(ICollection<CvfcAppearance> appearances)
    {
        //Add in CSRs.
        return appearances.Select(appearance => new CvfcDocument()
        {
            DocumentTypeCd = DocumentCategory.CSR,
            DocumentTypeDescription = "Court Summary",
            DocumentId = appearance.AppearanceId,
            ImageId = appearance.AppearanceId,
        });
    }
}
