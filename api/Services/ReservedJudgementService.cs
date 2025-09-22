using System.Threading.Tasks;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Scv.Api.Infrastructure;
using Scv.Api.Models;
using Scv.Db.Models;
using Scv.Db.Repositories;

namespace Scv.Api.Services;

public class ReservedJudgementService(
    IAppCache cache,
    IMapper mapper,
    ILogger<ReservedJudgementService> logger,
    IRepositoryBase<ReservedJudgement> judgementRepo) : CrudServiceBase<IRepositoryBase<ReservedJudgement>, ReservedJudgement, ReservedJudgementDto>(
        cache,
        mapper,
        logger,
        judgementRepo)
{
    public override string CacheName => "GetReservedJudgementsAsync";

    public override async Task<OperationResult<ReservedJudgementDto>> ValidateAsync(ReservedJudgementDto dto, bool isEdit = false)
    {
        return OperationResult<ReservedJudgementDto>.Success(dto);
    }
}