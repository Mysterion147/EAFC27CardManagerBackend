using EAFC27.Application.Interfaces;
using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Infrastructure.Services;

public class CardSeederService : ICardSeederService
{
    private readonly AppDbContext _context;

    public CardSeederService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedCardsAsync(string csvFilePath)
    {
        // import cards from CSV only if the database is empty
        if (await _context.Cards.AnyAsync())
            return;

        if (!File.Exists(csvFilePath))
            return;

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