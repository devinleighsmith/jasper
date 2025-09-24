using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scv.Api.Models;
using Scv.Api.Services;

namespace Scv.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservedJudgementsController(ICrudService<ReservedJudgementDto> judgementService) : ControllerBase
{
    private readonly ICrudService<ReservedJudgementDto> _judgementService = judgementService;

    [HttpGet]
    public async Task<ActionResult> ReservedJudgement()
    {
        var reservedJudgmenets = await _judgementService.GetAllAsync();

        return Ok(reservedJudgmenets);
    }
}