using FcTradeManager.Application.DTOs;
using FcTradeManager.Application.Interfaces;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FcTradeManager.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;

    public AlertService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AlertResponseDto>> GetUserAlertsAsync(Guid userId)
    {
        return await _context.Alerts
            .Where(a => a.UserId == userId)
            .Include(a => a.Card)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .Select(a => new AlertResponseDto
            {
                Id = a.Id,
                CardId = a.CardId,
                CardName = a.Card.Name,
                Kind = a.Kind,
                TriggeredPrice = a.TriggeredPrice,
                CreatedAt = a.CreatedAt,
                ReadAt = a.ReadAt
            })
            .ToListAsync();
    }

    public async Task MarkAlertAsReadAsync(Guid userId, Guid alertId)
    {
        var alert = await _context.Alerts
            .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

        if (alert == null)
            throw new KeyNotFoundException("Alert not found.");

        if (alert.ReadAt == null)
        {
            alert.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}