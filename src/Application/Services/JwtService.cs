using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Application.Services;

/// <summary>
/// Service for generating and managing JWT tokens
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggingService _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration, ILoggingService logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _secretKey =
                _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            _issuer =
                _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is not configured");
            _audience =
                _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is not configured");
            _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            _logger.LogInformation("JWT Service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize JWT Service configuration");
            throw;
        }
    }

    public string GenerateToken(UserEntity user)
    {
        if (user == null)
        {
            _logger.LogError("Attempt to generate token with null user");
            throw new ArgumentNullException(nameof(user));
        }

        try
        {
            _logger.LogDebug(
                "Generating JWT token for user: {UserId}, Email: {Email}",
                user.Id,
                user.Email
            );

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.AccessLevel.ToString()),
                new Claim("AccessLevel", user.AccessLevel.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Successfully generated JWT token for user: {UserId}", user.Id);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user: {UserId}", user.Id);
            throw;
        }
    }

    public int GetTokenExpirationSeconds()
    {
        return _expirationMinutes * 60;
    }
}
