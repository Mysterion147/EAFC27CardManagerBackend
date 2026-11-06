using FcTradeManager.Application.Interfaces;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FcTradeManager.Infrastructure.Services;

public class CardSeederService : ICardSeederService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CardSeederService> _logger;

    public CardSeederService(AppDbContext context, ILogger<CardSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedCardsAsync(string csvFilePath)
    {
        // import cards from CSV only if the database is empty
        if (await _context.Cards.AnyAsync())
            return;

        if (!File.Exists(csvFilePath))
        {
            _logger.LogWarning("Cards CSV not found at {Path}; the card catalog will stay empty.", csvFilePath);
            return;
        }

        var cards = new List<Card>();
        var lines = await File.ReadAllLinesAsync(csvFilePath);

        // skip the header line and parse each subsequent line
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split(',');
            if (values.Length < 6)
                continue;

            cards.Add(new Card
            {
                Id = Guid.NewGuid(),
                Name = values[0].Trim(),
                Overall = int.TryParse(values[1], out var ovr) ? ovr : 0,
                Position = values[2].Trim(),
                Club = values[3].Trim(),
                Rarity = values[4].Trim(),
                ExternalRef = values[5].Trim()
            });
        }

        if (cards.Any())
        {
            await _context.Cards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();
        }
    }
}