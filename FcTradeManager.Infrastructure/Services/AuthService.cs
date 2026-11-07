using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using FcTradeManager.Application.DTOs;
using FcTradeManager.Application.Interfaces;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace FcTradeManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenMinutes"] ?? "15"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (existingUser)
            throw new InvalidOperationException("This e-mail is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        var response = BuildAuthResponse(user, out _);
        await _context.SaveChangesAsync();
        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid e-mail or password.");

        var response = BuildAuthResponse(user, out _);
        await _context.SaveChangesAsync();
        return response;
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var stored = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (stored == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.RevokedAt != null)
        {
            // a revoked token being reused: assume theft and revoke every active token of this user
            var active = await _context.RefreshTokens
                .Where(r => r.UserId == stored.UserId && r.RevokedAt == null)
                .ToListAsync();
            foreach (var t in active) t.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        stored.RevokedAt = DateTime.UtcNow;
        var response = BuildAuthResponse(stored.User, out var newHash);
        stored.ReplacedByTokenHash = newHash;

        await _context.SaveChangesAsync();
        return response;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private AuthResponseDto BuildAuthResponse(User user, out string refreshTokenHash)
    {
        var (accessToken, expiresAt) = GenerateJwtToken(user);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        refreshTokenHash = HashToken(rawRefresh);

        var days = double.Parse(_configuration["JwtSettings:RefreshTokenDays"] ?? "7");
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        });

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = rawRefresh
        };
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}