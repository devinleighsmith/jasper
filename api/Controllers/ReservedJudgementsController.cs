using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scv.Api.Documents.Parsers;
using Scv.Api.Documents.Parsers.Models;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Models;
using Scv.Api.Services;
using Scv.Db.Models;

namespace Scv.Api.Controllers;

[Authorize(AuthenticationSchemes = "SiteMinder, OpenIdConnect", Policy = nameof(ProviderAuthorizationHandler))]
[Route("api/[controller]")]
[ApiController]
public class ReservedJudgementsController(ICrudService<ReservedJudgementDto> judgementService) : ControllerBase
{
    private readonly ICrudService<ReservedJudgementDto> _judgementService = judgementService;

    [HttpPost]
    public async Task<ActionResult> CreateReservedJudgement(ICsvParser parser, IMapper mapper)
    {
        var fileBytes = await System.IO.File.ReadAllBytesAsync("files/test.csv");
        using var memoryStream = new MemoryStream(fileBytes);
        var records = parser.Parse<CsvReservedJudgement>(memoryStream);
        var reservedJudgements = mapper.Map<List<ReservedJudgement>>(records);
        var dtoList = mapper.Map<List<ReservedJudgementDto>>(reservedJudgements);
        await _judgementService.AddRangeAsync(dtoList);

        return Ok(records);
    }

    [HttpGet]
    public async Task<ActionResult> GetReservedJudgements()
    {
        var reservedJudgments = await _judgementService.GetAllAsync();

        return Ok(reservedJudgments);
    }
}