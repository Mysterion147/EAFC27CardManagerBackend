using FcTradeManager.Application.DTOs;
using FcTradeManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FcTradeManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    private Guid GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlertResponseDto>>> GetAlerts()
    {
        var userId = GetUserId();
        var alerts = await _alertService.GetUserAlertsAsync(userId);
        return Ok(alerts);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _alertService.MarkAlertAsReadAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}