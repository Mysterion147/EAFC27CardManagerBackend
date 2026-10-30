using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Infrastructure.Services;

public class PriceService : IPriceService
{
    private readonly AppDbContext _context;

    public PriceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PriceSnapshotResponseDto> AddPriceSnapshotAsync(Guid cardId, CreatePriceSnapshotDto dto, string source = "manual")
    {
        var card = await _context.Cards.FindAsync(cardId);
        if (card == null)
            throw new KeyNotFoundException("Card not found in the catalog.");

        var snapshot = new PriceSnapshot
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            Price = dto.Price,
            Source = source,
            RecordedAt = DateTime.UtcNow
        };

        await _context.PriceSnapshots.AddAsync(snapshot);
        await _context.SaveChangesAsync();

        return new PriceSnapshotResponseDto
        {
            Id = snapshot.Id,
            CardId = snapshot.CardId,
            Price = snapshot.Price,
            Source = snapshot.Source,
            RecordedAt = snapshot.RecordedAt
        };
    }

    public async Task<IEnumerable<PriceSnapshotResponseDto>> GetPriceHistoryAsync(Guid cardId, string period = "24h")
    {
        var cardExists = await _context.Cards.AnyAsync(c => c.Id == cardId);
        if (!cardExists)
            throw new KeyNotFoundException("Card not found in the catalog.");

        var query = _context.PriceSnapshots
            .Where(p => p.CardId == cardId)
            .AsNoTracking();

        // Filtro de períodos para o gráfico (RF13): 24h, 7d, 30d
        var cutoffDate = period.ToLower() switch
        {
            "24h" => DateTime.UtcNow.AddHours(-24),
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.UtcNow.AddHours(-24)
        };

        query = query.Where(p => p.RecordedAt >= cutoffDate);

        return await query
            .OrderBy(p => p.RecordedAt)
            .Select(p => new PriceSnapshotResponseDto
            {
                Id = p.Id,
                CardId = p.CardId,
                Price = p.Price,
                Source = p.Source,
                RecordedAt = p.RecordedAt
            })
            .ToListAsync();
    }
}