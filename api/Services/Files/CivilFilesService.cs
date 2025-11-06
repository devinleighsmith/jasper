using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.Civil.AppearanceDetail;
using Scv.Api.Models.Civil.Appearances;
using Scv.Api.Models.Civil.CourtList;
using Scv.Api.Models.Civil.Detail;
using Scv.Api.Models.Search;
using Scv.Db.Contants;
using Scv.Db.Models;
using CivilAppearanceDetail = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceDetail;
using CivilAppearanceMethod = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceMethod;

namespace Scv.Api.Services.Files
{
    public class CivilFilesService
    {
        #region Variables

        private readonly ILogger<CivilFilesService> _logger;
        private readonly IBinderService _binderService;
        private readonly IAppCache _cache;
        private readonly FileServicesClient _filesClient;
        private readonly IMapper _mapper;
        private readonly LookupService _lookupService;
        private readonly LocationService _locationService;
        private readonly string _applicationCode;
        private readonly string _requestAgencyIdentifierId;
        private readonly string _requestPartId;
        private readonly List<string> _filterOutDocumentTypes;
        private readonly ClaimsPrincipal _currentUser;

        #endregion Variables

        #region Constructor

        public CivilFilesService(IConfiguration configuration,
            FileServicesClient filesClient,
            IMapper mapper,
            LookupService lookupService,
            LocationService locationService,
            IAppCache cache,
            ClaimsPrincipal user,
            ILogger<CivilFilesService> logger,
            IBinderService binderService)
        {
            _filesClient = filesClient;
            _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            _lookupService = lookupService;
            _locationService = locationService;
            _mapper = mapper;
            _cache = cache;
            _applicationCode = user?.ApplicationCode() ?? configuration.GetNonEmptyValue("Request:ApplicationCd");
            _requestAgencyIdentifierId = user?.AgencyCode() ?? configuration.GetNonEmptyValue("Request:AgencyIdentifierId");
            _requestPartId = user?.ParticipantId() ?? configuration.GetNonEmptyValue("Request:PartId");

            _logger = logger;
            this._binderService = binderService;
            _filterOutDocumentTypes = configuration.GetNonEmptyValue("ExcludeDocumentTypeCodesForCounsel").Split(",").ToList();
            _currentUser = user;
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Search for Civil Small Claims, Motor Vehicle Accidents and Enforcement/Legislated Statute civil files. This is necessary because the class applied is inconsistent as per JASPER-394.
        /// </summary>
        /// <param name="fcq">Civil File Search Parameters</param>
        /// <returns>Combined FileSearchResponse of all 3 classes</returns>
        public async Task<FileSearchResponse> SearchSmallClaimsAsync(FilesCivilQuery fcq)
        {
            // Pass correct CourtClass
            async Task<FileSearchResponse> GetSmallClaimsTask() => await this.SearchAsync(fcq);

            var mvaQuery = _mapper.Map<FilesCivilQuery>(fcq);
            var enforcementQuery = _mapper.Map<FilesCivilQuery>(fcq);

            mvaQuery.CourtClass = CourtClassCd.M;
            enforcementQuery.CourtClass = CourtClassCd.L;

            async Task<FileSearchResponse> GetMVATask() =>
                await this.SearchAsync(mvaQuery);
            async Task<FileSearchResponse> GetEnforcementTask() =>
                await this.SearchAsync(enforcementQuery);

            // Query all 3 in parallel
            var searchFilesTasks = new List<Task<FileSearchResponse>>
            {
                GetSmallClaimsTask(),
                GetMVATask(),
                GetEnforcementTask()
            };
            var result = await searchFilesTasks.WhenAll();

            // Consolidate result
            var totalCount = result
                .Select(r => int.TryParse(r.RecCount, out var val) ? val : 0)
                .Sum();
            var message = totalCount > 100
                ? "More than 100 records matched the search criteria, only the first 100 are returned"
                : null;
            var combinedFileDetails = result
                .SelectMany(r => r.FileDetail)
                .Take(totalCount > 100 ? 100 : totalCount);

            var finalResult = new FileSearchResponse
            {
                RecCount = totalCount.ToString(),
                FileDetail = combinedFileDetails.ToList(),
                ResponseMessageTxt = message
            };

            return finalResult;
        }

        public async Task<FileSearchResponse> SearchAsync(FilesCivilQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?

            // CourtLevel = "S"  Supreme court data, CourtLevel = "P" - Province.
            // Only Provincial files can be accessed in JASPER
            return await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId,
                _applicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumber, fcq.FilePrefix,
                fcq.FilePermissions, fcq.FileSuffixNumber, fcq.MDocReferenceTypeCode, fcq.CourtClass, CourtLevelCd.P,
                fcq.NameSearchType, fcq.LastName, fcq.OrgName, fcq.GivenName, fcq.Birth?.ToString("yyyy-MM-dd"),
                fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly, fcq.SearchByCrownFileDesignation,
                fcq.MdocJustinNumberSet, fcq.PhysicalFileIdSet);
        }

        public async Task<List<RedactedCivilFileDetailResponse>> GetFilesByAgencyIdCodeAndFileNumberText(string location,
            string fileNumber, CourtLevelCd courtLevelCd)
        {
            var fileDetails = new List<RedactedCivilFileDetailResponse>();
            CourtClassCd courtClass = CourtClassCd.A;
            var courtClassSet = fileNumber.Contains("-") && Enum.TryParse(fileNumber.Split("-")[0], out courtClass);
            fileNumber = fileNumber.Contains("-") ? fileNumber.Split("-")[1] : fileNumber;

            var fileSearchResponse = await SearchAsync(new FilesCivilQuery
            {
                FileHomeAgencyId = location,
                FileNumber = fileNumber,
                SearchMode = SearchMode.FILENO,
                CourtLevel = courtLevelCd
            });

            if (fileSearchResponse.ResponseCd != "0")
                _logger.LogInformation("Civil search returned responseCd != 0");

            ValidUserHelper.CheckIfValidUser(fileSearchResponse.ResponseMessageTxt);

            var fileIdAndAppearanceDate = fileSearchResponse?.FileDetail?.Where(fd => !courtClassSet || fd.CourtClassCd == courtClass)
                                                           .SelectToList(fd => new { fd.PhysicalFileId, fd.NextApprDt });

            if (fileIdAndAppearanceDate == null || fileIdAndAppearanceDate.Count == 0)
                return fileDetails;

            //Return the basic entry without doing a lookup.
            if (fileIdAndAppearanceDate.Count == 1)
                return new List<RedactedCivilFileDetailResponse> { new RedactedCivilFileDetailResponse { PhysicalFileId = fileIdAndAppearanceDate.First().PhysicalFileId } };

            var fileDetailTasks = new List<Task<CivilFileDetailResponse>>();
            foreach (var fileId in fileIdAndAppearanceDate)
            {
                async Task<CivilFileDetailResponse> FileDetails() =>
                    await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, fileId.PhysicalFileId);
                fileDetailTasks.Add(_cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{_requestAgencyIdentifierId}", FileDetails));
            }

            var fileDetailResponses = await fileDetailTasks.WhenAll();
            fileDetails = fileDetailResponses.SelectToList(fdr => _mapper.Map<RedactedCivilFileDetailResponse>(fdr));

            foreach (var fileDetail in fileDetails)
                fileDetail.NextApprDt = fileIdAndAppearanceDate.First(fa => fa.PhysicalFileId == fileDetail.PhysicalFileId)
                    .NextApprDt;

            return fileDetails;
        }

        public async Task<RedactedCivilFileDetailResponse> FileIdAsync(string fileId, bool isVcUser, bool isStaff)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, null, null, null, null, fileId);
            async Task<CivilAppearanceResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN.Y, HistoryYN.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{_requestAgencyIdentifierId}", FileDetails);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}-{_requestAgencyIdentifierId}", FileContent);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}-{_requestAgencyIdentifierId}", Appearances);

            await Task.WhenAll(appearancesTask, fileContentTask, fileDetailTask);

            ValidUserHelper.CheckIfValidUser(fileDetailTask.Result.ResponseMessageTxt);
            if (fileDetailTask.Result?.PhysicalFileId == null)
                return null;

            var detail = _mapper.Map<RedactedCivilFileDetailResponse>(fileDetailTask.Result);
            foreach (var document in PopulateDetailCsrsDocuments(fileDetailTask.Result.Appearance))
                if (!isVcUser)
                    detail.Document.Add(document);

            detail = await PopulateBaseDetail(detail);
            detail.Appearances = appearancesTask.Result;

            ICollection<ClParty> courtListParties = [];
            if ((detail.CourtClassCd == CivilFileDetailResponseCourtClassCd.C
                || detail.CourtClassCd == CivilFileDetailResponseCourtClassCd.M
                || detail.CourtClassCd == CivilFileDetailResponseCourtClassCd.L
                || detail.CourtClassCd == CivilFileDetailResponseCourtClassCd.F)
                && detail.Appearances != null
                && detail.Appearances.ApprDetail.Count != 0)
            {
                // Call CourtList to get Party alias for Small Claims when case detail has an appearance.
                // Division Code, File Number and CourtLevel params appears to be not working and may introduce performance issues
                // because the endpoint returns all court list data.
                var latestApprearance = detail.Appearances.ApprDetail.OrderByDescending(a => a.AppearanceDt).FirstOrDefault();
                if (latestApprearance != null)
                {
                    var agencyId = await _locationService.GetLocationAgencyIdentifier(latestApprearance.CourtAgencyId);
                    var courtList = await _filesClient.FilesCourtlistAsync(
                        _requestAgencyIdentifierId,
                        _requestPartId,
                        _applicationCode,
                        agencyId,
                        latestApprearance.CourtRoomCd,
                        latestApprearance.AppearanceDt,
                        "CV",
                        detail.FileNumberTxt);
                    var civilCourtListFileDetail = courtList.CivilCourtList.FirstOrDefault(c => c.PhysicalFile.PhysicalFileID == detail.PhysicalFileId);
                    courtListParties = civilCourtListFileDetail?.Parties ?? [];
                }
            }

            var fileContentCivilFile = fileContentTask.Result?.CivilFile?.First(cf => cf.PhysicalFileID == fileId);
            var partyTask = PopulateDetailParties(detail.Party, courtListParties);
            var documentTask = PopulateDetailDocuments(detail.Document, fileContentCivilFile, isVcUser, isStaff);
            var hearingRestrictionTask = PopulateDetailHearingRestrictions(fileDetailTask.Result.HearingRestriction);

            await Task.WhenAll(partyTask, documentTask, hearingRestrictionTask);

            detail.Party = partyTask.Result;
            detail.Document = documentTask.Result;
            detail.HearingRestriction = hearingRestrictionTask.Result;
            if (isVcUser)
            {
                //SCV-266 - Disable comments for VC Users.
                foreach (var document in detail.Document)
                    document.CommentTxt = "";
                detail.HearingRestriction = [];
            }

            return detail;
        }

        public async Task<CivilAppearanceDetail> DetailedAppearanceDocuments(string fileId, string appearanceId, bool isVcUser = false, bool includeJudicialBinder = false)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, null, null, null, null, fileId);
            //async Task<CivilFileAppearancePartyResponse> AppearanceParty() => await _filesClient.FilesCivilAppearancePartiesAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, appearanceId);
            //async Task<CivilFileAppearanceApprMethodResponse> AppearanceMethods() => await _filesClient.FilesCivilAppearanceAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, appearanceId);
            //async Task<CivilAppearanceResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN.Y, HistoryYN.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{_requestAgencyIdentifierId}", FileDetails);
            // var appearancePartyTask = _cache.GetOrAddAsync($"CivilAppearanceParty-{fileId}-{appearanceId}-{_requestAgencyIdentifierId}", AppearanceParty);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}-{_requestAgencyIdentifierId}", FileContent);
            //var appearanceMethodsTask = _cache.GetOrAddAsync($"CivilAppearanceMethods-{fileId}-{appearanceId}-{_requestAgencyIdentifierId}", AppearanceMethods);
            //var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}-{_requestAgencyIdentifierId}", Appearances);

            
            
            //var targetAppearance = appearancesTask.Result?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);

            //if (targetAppearance == null || fileDetailTask.Result == null)
                //return null;

            //Sometimes we can have a bogus location, querying court list wont work.
            //ClCivilCourtList civilCourtList = null;
            //var agencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            // This is HUGE
            // if (agencyId != null)
            // {
            //     async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CV", detail.FileNumberTxt);
            //     var courtListTask = _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}-{_requestAgencyIdentifierId}", CourtList);
            //     var courtList = await courtListTask;
            //     civilCourtList = courtList.CivilCourtList.FirstOrDefault(cl => cl.AppearanceId == appearanceId);
            // }

            //await Task.WhenAll(appearanceMethodsTask);
            //var appearanceMethods = appearanceMethodsTask.Result.AppearanceMethod;

            //var appearanceDetail = appearancesTask.Result?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            await Task.WhenAll(fileDetailTask, fileContentTask);
            var fileDetailDocuments = fileDetailTask.Result.Document.Where(doc => doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId)).ToList();
            var fileContentCivilFile = fileContentTask.Result?.CivilFile?.FirstOrDefault(cf => cf.PhysicalFileID == fileId);
            //var previousAppearance = fileContentCivilFile?.PreviousAppearance.FirstOrDefault(pa => pa?.AppearanceId == appearanceId);
            var binderDocumentsTask = includeJudicialBinder ? PopulateBinderDocuments(fileDetailTask.Result, fileContentCivilFile) : Task.FromResult<IList<CivilDocument>>(new List<CivilDocument>());
            var documentsTask = PopulateDetailedAppearanceDocuments(fileDetailDocuments, isVcUser);
            var agencyIdTask = _locationService.GetLocationAgencyIdentifier(fileDetailTask.Result.HomeLocationAgenId);
            await Task.WhenAll(documentsTask, binderDocumentsTask, agencyIdTask);
            // if (isVcUser)
            // {
            //     foreach (var document in documents)
            //         document.CommentTxt = "";
            // }

            // Extract awaits to tasks
            
            //var appearanceReasonDescTask = _lookupService.GetCivilAppearanceReasonsDescription(appearanceDetail?.AppearanceReasonCd);
            //var appearanceResultDescTask = _lookupService.GetCivilAppearanceResultsDescription(appearanceDetail?.AppearanceResultCd);
            //var appearanceMethodTask = PopulateAppearanceMethods(appearanceMethods);
            //var partyTask = PopulateDetailedAppearancePartiesAsync(appearancePartyTask.Result.Party, civilCourtList?.Parties, previousAppearance, appearanceMethods);
            // var adjudicatorTask = PopulateDetailedAppearanceAdjudicator(previousAppearance, appearanceMethods);
            

            // await Task.WhenAll(
            //     //agencyIdTask,
            //     //appearanceReasonDescTask,
            //     //appearanceResultDescTask,
            //     //appearanceMethodTask,
            //     //partyTask,
            //     //adjudicatorTask,
            //     binderDocumentsTask
            // );

            var detailedAppearance = new CivilAppearanceDetail
            {
                //PhysicalFileId = fileId,
                AgencyId = agencyIdTask.Result,
                AppearanceId = appearanceId,
                //AppearanceResultCd = appearanceDetail?.AppearanceResultCd,
                //AppearanceReasonCd = appearanceDetail?.AppearanceReasonCd,
                //AppearanceReasonDesc = appearanceReasonDescTask.Result,
                //AppearanceResultDesc = appearanceResultDescTask.Result,
                //CourtRoomCd = targetAppearance.CourtRoomCd,
                FileNumberTxt = fileDetailTask.Result.FileNumberTxt,
                //AppearanceDt = targetAppearance.AppearanceDt,
                //AppearanceMethod = appearanceMethodTask.Result,
                //Party = [],
                Document = documentsTask.Result, // needed
                //Adjudicator = adjudicatorTask.Result,
                //AdjudicatorComment = previousAppearance?.AdjudicatorComment,
                CourtLevelCd = fileDetailTask.Result.CourtLevelCd,
                BinderDocuments = binderDocumentsTask.Result
            };

            return detailedAppearance;
        }

        public async Task<CivilAppearanceDetailParties> DetailedAppearanceParties(string fileId, string appearanceId)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, fileId);
            //async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, null, null, null, null, fileId);
            async Task<CivilFileAppearancePartyResponse> AppearanceParty() => await _filesClient.FilesCivilAppearancePartiesAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, appearanceId);
            async Task<CivilAppearanceResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN.Y, HistoryYN.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{_requestAgencyIdentifierId}", FileDetails);
            var appearancePartyTask = _cache.GetOrAddAsync($"CivilAppearanceParty-{fileId}-{appearanceId}-{_requestAgencyIdentifierId}", AppearanceParty);
            //var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}-{_requestAgencyIdentifierId}", FileContent);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}-{_requestAgencyIdentifierId}", Appearances);

            await Task.WhenAll(fileDetailTask, appearancesTask, appearancePartyTask);
            var detail = fileDetailTask.Result;
            var targetAppearance = appearancesTask.Result?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);

            if (targetAppearance == null)
                return null;

            //Sometimes we can have a bogus location, querying court list wont work.
            ClCivilCourtList civilCourtList = null;
            var agencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            // This is HUGE
            if (agencyId != null)
            {
                async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CV", detail.FileNumberTxt);
                var courtListTask = _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}-{_requestAgencyIdentifierId}", CourtList);
                var courtList = await courtListTask;
                civilCourtList = courtList.CivilCourtList.FirstOrDefault(cl => cl.AppearanceId == appearanceId);
            }

            //var fileContentCivilFile = fileContentTask.Result?.CivilFile?.FirstOrDefault(cf => cf.PhysicalFileID == fileId);
            //var previousAppearance = fileContentCivilFile?.PreviousAppearance.FirstOrDefault(pa => pa?.AppearanceId == appearanceId);

            var parties = await PopulateDetailedAppearancePartiesAsync(appearancePartyTask.Result.Party, civilCourtList?.Parties);

            var detailedParties = new CivilAppearanceDetailParties
            {
                AppearanceId = appearanceId,
                Party = parties,
            };

            return detailedParties;
        }

        public async Task<CivilAppearanceDetail> DetailedAppearanceMethods(string fileId, string appearanceId)
        {
            //async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilGetAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, fileId);
            //async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, null, null, null, null, fileId);
            //async Task<CivilFileAppearancePartyResponse> AppearanceParty() => await _filesClient.FilesCivilAppearancePartiesAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, appearanceId);
            async Task<CivilFileAppearanceApprMethodResponse> AppearanceMethods() => await _filesClient.FilesCivilAppearanceAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, appearanceId);
            //async Task<CivilAppearanceResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN.Y, HistoryYN.Y, fileId);

            //var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}-{_requestAgencyIdentifierId}", FileDetails);
            // var appearancePartyTask = _cache.GetOrAddAsync($"CivilAppearanceParty-{fileId}-{appearanceId}-{_requestAgencyIdentifierId}", AppearanceParty);
            //var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}-{_requestAgencyIdentifierId}", FileContent);
            var appearanceMethods = await _cache.GetOrAddAsync($"CivilAppearanceMethods-{fileId}-{appearanceId}-{_requestAgencyIdentifierId}", AppearanceMethods);
            //var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}-{_requestAgencyIdentifierId}", Appearances);

            //var appearanceMethod = await appearanceMethodsTask;
            //var detail = fileDetailTask.Result;
            //var targetAppearance = appearancesTask.Result?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);

            //if (targetAppearance == null || fileDetailTask.Result == null)
                //return null;

            //Sometimes we can have a bogus location, querying court list wont work.
            //ClCivilCourtList civilCourtList = null;
            //var agencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            // This is HUGE
            // if (agencyId != null)
            // {
            //     async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CV", detail.FileNumberTxt);
            //     var courtListTask = _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}-{_requestAgencyIdentifierId}", CourtList);
            //     var courtList = await courtListTask;
            //     civilCourtList = courtList.CivilCourtList.FirstOrDefault(cl => cl.AppearanceId == appearanceId);
            // }

            //var appearanceMethods = appearanceMethod.AppearanceMethod;

            //var appearanceDetail = appearancesTask.Result?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            //var fileDetailDocuments = fileDetailTask.Result.Document.Where(doc => doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId)).ToList();
            //var fileContentCivilFile = fileContentTask.Result?.CivilFile?.FirstOrDefault(cf => cf.PhysicalFileID == fileId);
            //var previousAppearance = fileContentCivilFile?.PreviousAppearance.FirstOrDefault(pa => pa?.AppearanceId == appearanceId);

            // var documents = await PopulateDetailedAppearanceDocuments(fileDetailDocuments, isVcUser);
            // if (isVcUser)
            // {
            //     foreach (var document in documents)
            //         document.CommentTxt = "";
            // }

            // Extract awaits to tasks
            //var agencyIdTask = _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            //var appearanceReasonDescTask = _lookupService.GetCivilAppearanceReasonsDescription(appearanceDetail?.AppearanceReasonCd);
            //var appearanceResultDescTask = _lookupService.GetCivilAppearanceResultsDescription(appearanceDetail?.AppearanceResultCd);
            //var appearanceMethodTask =;
            //var partyTask = PopulateDetailedAppearancePartiesAsync(appearancePartyTask.Result.Party, civilCourtList?.Parties, previousAppearance, appearanceMethods);
            // var adjudicatorTask = PopulateDetailedAppearanceAdjudicator(previousAppearance, appearanceMethods);
            //var binderDocumentsTask = includeJudicialBinder ? PopulateBinderDocuments(detail, fileContentCivilFile) : Task.FromResult<IList<CivilDocument>>([]);

            //await Task.WhenAll(
                //agencyIdTask,
                //appearanceReasonDescTask,
                //appearanceResultDescTask,
                //appearanceMethodTask,
                //partyTask,
                //adjudicatorTask,
                //binderDocumentsTask
            //);

            var detailedAppearance = new CivilAppearanceDetail
            {
                //PhysicalFileId = fileId,
                //AgencyId = agencyIdTask.Result,
                //AppearanceId = appearanceId,
                //AppearanceResultCd = appearanceDetail?.AppearanceResultCd,
                //AppearanceReasonCd = appearanceDetail?.AppearanceReasonCd,
                //AppearanceReasonDesc = appearanceReasonDescTask.Result,
                //AppearanceResultDesc = appearanceResultDescTask.Result,
                //CourtRoomCd = targetAppearance.CourtRoomCd,
                //FileNumberTxt = detail.FileNumberTxt,
                //AppearanceDt = targetAppearance.AppearanceDt,
                AppearanceMethod =  await PopulateAppearanceMethods(appearanceMethods.AppearanceMethod),
                //Party = [],
                //Document = documents, // needed
                //Adjudicator = adjudicatorTask.Result,
                //AdjudicatorComment = previousAppearance?.AdjudicatorComment,
                //CourtLevelCd = detail.CourtLevelCd,
                //BinderDocuments = binderDocumentsTask.Result
            };

            return detailedAppearance;
        }

        public async Task<JustinReportResponse> CourtSummaryReportAsync(string appearanceId, string reportName)
        {
            var justinReportResponse = await _filesClient.FilesCivilCourtsummaryreportAsync(_requestAgencyIdentifierId,
                _requestPartId, _applicationCode, appearanceId, reportName);
            return justinReportResponse;
        }

        public async Task<CivilFileContent> FileContentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string physicalFileId)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _filesClient.FilesCivilFilecontentAsync(_requestAgencyIdentifierId,
                _requestPartId, _applicationCode, agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
        }

        #endregion Methods

        #region Helpers

        #region Civil Details

        private async Task<CivilAppearanceResponse> PopulateDetailAppearancesAsync(FutureYN? future, HistoryYN? history, string fileId)
        {
            var civilFileAppearancesResponse = await _filesClient.FilesCivilAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, _applicationCode, future, history,
                fileId);
            if (civilFileAppearancesResponse == null)
                return null;

            var civilAppearances = _mapper.Map<CivilAppearanceResponse>(civilFileAppearancesResponse);
            var tasks = new List<Task>();
            foreach (var appearance in civilAppearances.ApprDetail)
            {
                tasks.Add(Task.Run(async () =>
                {
                    appearance.AppearanceReasonDsc = await _lookupService.GetCivilAppearanceReasonsDescription(appearance.AppearanceReasonCd);
                    appearance.AppearanceResultDsc = await _lookupService.GetCivilAppearanceResultsDescription(appearance.AppearanceResultCd);
                    appearance.AppearanceStatusDsc = await _lookupService.GetCivilAppearanceStatusDescription(appearance.AppearanceStatusCd.ToString());
                    appearance.CourtLocationId = await _locationService.GetLocationAgencyIdentifier(appearance.CourtAgencyId);
                    appearance.CourtLocation = await _locationService.GetLocationName(appearance.CourtAgencyId);
                    appearance.DocumentTypeDsc = await _lookupService.GetDocumentDescriptionAsync(appearance.DocumentTypeCd);
                }));
            }
            await Task.WhenAll(tasks);
            return civilAppearances;
        }

        private static IEnumerable<CivilDocument> PopulateDetailCsrsDocuments(ICollection<CvfcAppearance> appearances)
        {
            //Add in CSRs.
            return appearances.Select(appearance => new CivilDocument()
            {
                Category = "CSR",
                DocumentTypeDescription = "Court Summary",
                CivilDocumentId = appearance.AppearanceId,
                ImageId = appearance.AppearanceId,
                DocumentTypeCd = "CSR",
                LastAppearanceDt = appearance.AppearanceDate,
                FiledDt = appearance.AppearanceDate,
            });
        }

        private async Task<RedactedCivilFileDetailResponse> PopulateBaseDetail(RedactedCivilFileDetailResponse detail)
        {
            var homeLocationAgencyCodeTask = _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            var homeLocationAgencyNameTask = _locationService.GetLocationName(detail.HomeLocationAgenId);
            var courtClassDescriptionTask = _lookupService.GetCourtClassDescription(detail.CourtClassCd.ToString());
            var courtLevelDescriptionTask = _lookupService.GetCourtLevelDescription(detail.CourtLevelCd.ToString());
            var activityClassCdTask = _lookupService.GetActivityClassCdLong(detail.CourtClassCd.ToString());
            var activityClassDescTask = _lookupService.GetActivityClassCdShort(detail.CourtClassCd.ToString());
            var regionNameTask =  _locationService.GetRegionName(detail.HomeLocationAgencyCode);

            await Task.WhenAll(homeLocationAgencyCodeTask, homeLocationAgencyNameTask, courtClassDescriptionTask, courtLevelDescriptionTask, activityClassCdTask, activityClassDescTask, regionNameTask);

            detail.HomeLocationAgencyCode = homeLocationAgencyCodeTask.Result;
            detail.HomeLocationAgencyName = homeLocationAgencyNameTask.Result;
            detail.HomeLocationRegionName = regionNameTask.Result;
            detail.CourtClassDescription = courtClassDescriptionTask.Result;
            detail.CourtLevelDescription = courtLevelDescriptionTask.Result;
            detail.ActivityClassCd = activityClassCdTask.Result;
            detail.ActivityClassDesc = activityClassDescTask.Result;
            //Some lookups have LongDesc and ShortDesc the same. 
            if (detail.ActivityClassCd == detail.ActivityClassDesc)
                detail.ActivityClassCd = detail?.CourtClassCd.ToString();
            return detail;
        }

        private async Task<IList<CivilDocument>> PopulateDetailDocuments(IList<CivilDocument> documents, CvfcCivilFile civilFileContent, bool isVcUser, bool isStaff)
        {
            //Populate extra fields for document.
            documents = documents.WhereToList(doc => !isVcUser || !_filterOutDocumentTypes.Contains(doc.DocumentTypeCd));
            // Precompute all needed lookups outside the foreach for performance
            var nonCsrDocuments = documents.Where(doc => doc.Category != "CSR").ToList();

            // Prepare lookup tasks for document categories and descriptions
            var categoryTasks = nonCsrDocuments.Select(doc => _lookupService.GetDocumentCategory(doc.DocumentTypeCd)).ToList();
            var descriptionTasks = nonCsrDocuments.Select(doc => _lookupService.GetDocumentDescriptionAsync(doc.DocumentTypeCd)).ToList();

            // Prepare lookup tasks for issues
            var issueTypeTasks = nonCsrDocuments
                .SelectMany(doc => doc.Issue)
                .Select(issue => _lookupService.GetCivilDocumentIssueType(issue.IssueTypeCd))
                .ToList();

            // Await all lookups in parallel
            var categories = await Task.WhenAll(categoryTasks);
            var descriptions = await Task.WhenAll(descriptionTasks);
            var issueTypeDescs = await Task.WhenAll(issueTypeTasks);

            // Map issueTypeDescs back to issues
            int issueIdx = 0;

            for (int i = 0; i < nonCsrDocuments.Count; i++)
            {
                var document = nonCsrDocuments[i];
                var documentFromFileContent = civilFileContent?.Document?.FirstOrDefault(doc => doc.DocumentId == document.CivilDocumentId);
                document.FiledBy = documentFromFileContent?.FiledBy;
                document.Category = categories[i];
                document.DocumentTypeDescription = descriptions[i];
                document.ImageId = (document.SealedYN == "Y" && isStaff) ? null : document.ImageId;
                document.NextAppearanceDt = document.Appearance?.Where(app => DateTime.TryParse(app?.AppearanceDate, out DateTime appearanceDate) && appearanceDate >= DateTime.Today).FirstOrDefault()?.AppearanceDate;
                document.Appearance = null;
                document.SwornByNm = documentFromFileContent?.SwornByNm;
                document.AffidavitNo = documentFromFileContent?.AffidavitNo;

                foreach (var issue in document.Issue)
                {
                    issue.IssueTypeDesc = issueTypeDescs[issueIdx++];
                }
            }
            return documents;
        }

        private async Task<ICollection<CivilParty>> PopulateDetailParties(ICollection<CivilParty> parties, ICollection<ClParty> courtListParties)
        {
            //Populate extra fields for party.
            foreach (var party in parties)
            {
                var courtListParty = courtListParties.FirstOrDefault(clp => clp.PartyId == party.PartyId);
                if (courtListParty != null)
                {
                    party.BirthDate = courtListParty.BirthDate;
                    party.Aliases = [.. courtListParty.PartyName.Where(p => p.NameTypeCd != "CUR")];
                }
            }

            var roleTypeDescTasks = parties.Select(p => _lookupService.GetCivilRoleTypeDescription(p.RoleTypeCd)).ToList();
            var roleTypeDescs = await Task.WhenAll(roleTypeDescTasks);

            for (int i = 0; i < parties.Count; i++)
            {
                parties.ElementAt(i).RoleTypeDescription = roleTypeDescs[i];
            }

            return parties;
        }

        private async Task<ICollection<CivilHearingRestriction>> PopulateDetailHearingRestrictions(ICollection<CvfcHearingRestriction2> hearingRestrictions)
        {
            // Assigned restriction is only included if current user is assigned to any of the role
            var includeAssigned = _currentUser
                .HasRoles(
                [
                    Role.ADMIN,
                    Role.ACJ_CHIEF_JUDGE,
                    Role.RAJ,
                    Role.PO_MANAGER
                ], true);

            var restrictionTypeOrder = new List<CvfcHearingRestriction2HearingRestrictionTypeCd>
            {
                CvfcHearingRestriction2HearingRestrictionTypeCd.S,
                CvfcHearingRestriction2HearingRestrictionTypeCd.G,
                CvfcHearingRestriction2HearingRestrictionTypeCd.A,
                CvfcHearingRestriction2HearingRestrictionTypeCd.D,
            };

            var civilHearingRestrictions = _mapper.Map<ICollection<CivilHearingRestriction>>(
                hearingRestrictions.Where(r => includeAssigned
                    || r.HearingRestrictionTypeCd != CvfcHearingRestriction2HearingRestrictionTypeCd.G)
                .OrderBy(r => restrictionTypeOrder.IndexOf(r.HearingRestrictionTypeCd))
                .ThenBy(r => r.AdjFullNm));
                // Build a list of tasks for hearing restriction descriptions
                var hearingRestrictionDescTasks = civilHearingRestrictions
                    .Select(h => _lookupService.GetHearingRestrictionDescription(h.HearingRestrictionTypeCd.ToString()));

                var hearingRestrictionDescs = await Task.WhenAll(hearingRestrictionDescTasks);

                int idx = 0;
                foreach (var hearing in civilHearingRestrictions)
                {
                    hearing.HearingRestrictionTypeDsc = hearingRestrictionDescs[idx++];
                }
            return civilHearingRestrictions;
        }

        #endregion Civil Details

        #region Civil Appearance Details

        private async Task<ICollection<CivilAppearanceMethod>> PopulateAppearanceMethods(ICollection<JCCommon.Clients.FileServices.CivilAppearanceMethod> baseAppearanceMethods)
        {
            var appearanceMethods = _mapper.Map<ICollection<CivilAppearanceMethod>>(baseAppearanceMethods);
            var tasks = new List<Task>();
            foreach (var appearanceMethod in appearanceMethods)
            {
                tasks.Add(Task.Run(async () =>
                {
                    appearanceMethod.AppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(appearanceMethod.AppearanceMethodCd);
                    appearanceMethod.RoleTypeDesc = await _lookupService.GetCivilRoleTypeDescription(appearanceMethod.RoleTypeCd);
                }));
            }

            await Task.WhenAll(tasks);
            return appearanceMethods;
        }

        private async Task<CivilAdjudicator> PopulateDetailedAppearanceAdjudicator(CvfcPreviousAppearance previousAppearance, ICollection<JCCommon.Clients.FileServices.CivilAppearanceMethod> civilAppearanceMethods)
        {
            if (previousAppearance == null)
                return null;

            var appearanceMethodCd = civilAppearanceMethods.FirstOrDefault(am => am.RoleTypeCd == "ADJ")?.AppearanceMethodCd;
            return new CivilAdjudicator
            {
                FullName = previousAppearance.AdjudicatorName,
                AdjudicatorAppearanceMethod = previousAppearance.AdjudicatorAppearanceMethod,
                AdjudicatorAppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(previousAppearance.AdjudicatorAppearanceMethod),
                AppearanceMethodCd = appearanceMethodCd,
                AppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(appearanceMethodCd)
            };
        }

        /// <summary>
        /// This is mostly based off of getAppearanceCivilParty and expands by court list and FileContent.
        /// </summary>
        private async Task<ICollection<CivilAppearanceDetailParty>> PopulateDetailedAppearancePartiesAsync(ICollection<CivilAppearanceParty> parties,
            ICollection<ClParty> courtListParties)
        {
            // Optimize this
            var resultParties = new List<CivilAppearanceDetailParty>();
            var tasks = new List<Task>();
            foreach (var partyGroup in parties.GroupBy(a => a.PartyId))
            {
                //Map over our primary values from party group.
                var party = _mapper.Map<CivilAppearanceDetailParty>(partyGroup.First());

                //Get our roles from getAppearanceCivilParty. These should essentially be the same.
                tasks.Add(Task.Run(async () =>
                {
                    party.PartyRole = await partyGroup.Select(async pg => new ClPartyRole
                    {
                        RoleTypeCd = pg.PartyRoleTypeCd,
                        RoleTypeDsc = await _lookupService.GetCivilRoleTypeDescription(pg.PartyRoleTypeCd)
                    }).WhenAll();
                }));

                //Get information from appearanceMethods.
                // if (civilAppearanceMethods.Any(am => party.PartyRole.Any(pr => pr.RoleTypeCd == am.RoleTypeCd)))
                // {
                //     party.AppearanceMethodCd = civilAppearanceMethods.First().AppearanceMethodCd;
                //     party.AppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(civilAppearanceMethods.First().AppearanceMethodCd);
                // }

                //Get the additional information from court list.
                var courtListParty = courtListParties?.FirstOrDefault(clp => clp.PartyId == partyGroup.Key);
                if (courtListParty != null)
                {
                    //party.AttendanceMethodCd = courtListParty.AttendanceMethodCd;
                    //party.AttendanceMethodDesc = await _lookupService.GetCivilAssetsDescription(party.AttendanceMethodCd);
                    party.Counsel = _mapper.Map<ICollection<CivilCounsel>>(courtListParty.Counsel);
                    //party.Representative = _mapper.Map<ICollection<CivilRepresentative>>(courtListParty.Representative);
                    // foreach (var representative in party.Representative)
                    // {
                    //     representative.AttendanceMethodDesc = await _lookupService.GetCivilAssetsDescription(representative.AttendanceMethodCd);
                    // }
                    //party.LegalRepresentative = courtListParty.LegalRepresentative;
                }

                //Add additional information from previous appearance - this comes from FileContent.
                //if (previousAppearance?.CourtParticipant != null && previousAppearance.CourtParticipant.Any(cp => cp.PartId == party.PartyId))
                //{
                //Can be multiple rows here, but they repeat for the partyRole.
                //var targetParticipant = previousAppearance.CourtParticipant.First(cp => cp.PartId == party.PartyId);
                //party.PartyAppearanceMethod = targetParticipant.PartyAppearanceMethod;
                //party.PartyAppearanceMethodDesc = await _lookupService.GetCivilPartyAttendanceType(targetParticipant.PartyAppearanceMethod);

                //Update the counsel with their appearanceMethod.
                // foreach (var counsel in targetParticipant.Counsel)
                // {
                //     //We match on counselName. 
                //     //Not the best idea of matching data, but we aren't provided a counselId for Civil FileContent.  Although we are provided a counselId in FileDetails, CourtList. 
                //     //TEST Environment - Civil Case 2151 - Appearance 9042, demonstrates this.

                //     if (!counsel.AdditionalProperties.ContainsKey("counselName"))
                //         continue;

                //     var targetCounsel = party.Counsel?.FirstOrDefault(c => c.CounselFullName == counsel.CounselName);
                //     if (targetCounsel == null)
                //         continue;

                //     targetCounsel.CounselAppearanceMethod = counsel.CounselAppearanceMethod;
                //     targetCounsel.CounselAppearanceMethodDesc = await _lookupService.GetCivilCounselAttendanceType(targetCounsel.CounselAppearanceMethod);
                // }
                //}

                resultParties.Add(party);
            }
            await Task.WhenAll(tasks);
            return resultParties;
        }

        private async Task<IEnumerable<CivilAppearanceDocument>> PopulateDetailedAppearanceDocuments(List<CvfcDocument3> fileDetailDocuments, bool isVcUser)
        {
            //CivilAppearanceDocument, doesn't include appearances.
            var documents = _mapper.Map<IEnumerable<CivilAppearanceDocument>>(fileDetailDocuments);
            documents = documents.Where(doc => !isVcUser || !_filterOutDocumentTypes.Contains(doc.DocumentTypeCd));
            
            if (!documents.Any())
                return documents;
            
            // Batch all unique document type codes for parallel lookup
            var uniqueDocumentTypeCds = documents.Select(d => d.DocumentTypeCd).Where(cd => !string.IsNullOrEmpty(cd)).Distinct();
            
            // Execute all lookups in parallel
            var categoryTasks = uniqueDocumentTypeCds.Select(async cd => new { Code = cd, Category = await _lookupService.GetDocumentCategory(cd) });
            var descriptionTasks = uniqueDocumentTypeCds.Select(async cd => new { Code = cd, Description = await _lookupService.GetDocumentDescriptionAsync(cd) });
            
            var categoryResults = await Task.WhenAll(categoryTasks);
            var descriptionResults = await Task.WhenAll(descriptionTasks);
            
            // Create lookup dictionaries for fast access
            var categoryLookup = categoryResults.Where(r => !string.IsNullOrEmpty(r.Code)).ToDictionary(r => r.Code, r => r.Category ?? "");
            var descriptionLookup = descriptionResults.Where(r => !string.IsNullOrEmpty(r.Code)).ToDictionary(r => r.Code, r => r.Description ?? "");
            
            // Apply the results to each document
            foreach (var document in documents)
            {
                document.Category = !string.IsNullOrEmpty(document.DocumentTypeCd) 
                    ? categoryLookup.GetValueOrDefault(document.DocumentTypeCd, "") 
                    : "";
                document.DocumentTypeDescription = !string.IsNullOrEmpty(document.DocumentTypeCd) 
                    ? descriptionLookup.GetValueOrDefault(document.DocumentTypeCd, "") 
                    : "";
                document.ImageId = document.SealedYN != "N" ? null : document.ImageId;
                
                //foreach (var issue in document.Issue)
                //{
                    //issue.IssueTypeDesc = await _lookupService.GetCivilDocumentIssueType(issue.IssueTypeCd);
                    //issue.IssueResultCdDesc = await _lookupService.GetCivilDocumentIssueResult(issue.IssueResultCd);
                //}
            }
            
            return documents;
        }

        private async Task<IList<CivilDocument>> PopulateBinderDocuments(CivilFileDetailResponse detail, CvfcCivilFile fileContentCivilFile)
        {
            
            var labels = new Dictionary<string, string>
            {
                { LabelConstants.PHYSICAL_FILE_ID, detail.PhysicalFileId },
                { LabelConstants.COURT_CLASS_CD, detail.CourtClassCd.ToString() },
                { LabelConstants.JUDGE_ID, _currentUser.UserId().ToString() }
            };

            var binders = await _binderService.GetByLabels(labels);
            //return [];
            if (!binders.Succeeded || binders.Payload.Count == 0)
            {
                return [];
            }

            var binder = binders.Payload[0];
            var binderDocIds = binder.Documents.Select(d => d.DocumentId);

            var binderDocIdsOrdered = binder.Documents.Select((d, index) => new { d.DocumentId, Order = index }).ToDictionary(x => x.DocumentId, x => x.Order);

            var csrDocs = PopulateDetailCsrsDocuments([.. detail.Appearance.Where(a => binderDocIds.Contains(a.AppearanceId))]);
            var mappedDetail = _mapper.Map<RedactedCivilFileDetailResponse>(detail);
            var otherDocs = mappedDetail.Document.Where(d => binderDocIds.Contains(d.CivilDocumentId));

            var orderedDocs = csrDocs.Concat(otherDocs)
                .OrderBy(d => binderDocIdsOrdered.TryGetValue(d.CivilDocumentId, out var order) ? order : int.MaxValue);

            var binderDocuments = await PopulateDetailDocuments([.. orderedDocs], fileContentCivilFile, false, false);

            return binderDocuments;
        }

        #endregion Civil Appearance Details

        #endregion Helpers
    }
}