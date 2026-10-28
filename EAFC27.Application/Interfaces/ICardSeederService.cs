namespace EAFC27.Application.Interfaces;

public interface ICardSeederService
{
    Task SeedCardsAsync(string csvFilePath);
}