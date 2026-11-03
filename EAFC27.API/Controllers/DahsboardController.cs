using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EAFC27.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ITradeService _tradeService;

    public DashboardController(ITradeService tradeService)
    {
        _tradeService = tradeService;
    }

    private Guid GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
    {
        var userId = GetUserId();
        var dashboard = await _tradeService.GetDashboardAsync(userId);
        return Ok(dashboard);
    }
}