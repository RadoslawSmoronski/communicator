using Application.Interfaces;
using Application.Settings;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Shared.Result;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly JWTTokenSettings _tokenSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(UserManager<UserAccount> userManager, IOptions<JWTTokenSettings> options, ILogger<TokenService> logger)
        {
            _userManager = userManager;
            _tokenSettings = options.Value;
            _logger = logger;
        }

        public async Task<Result<string>> CreateAccessTokenAsync(Guid userId)
        {
            _logger.LogInformation("[TokenService - CreateAccessTokenAsync] Attempting to create access token for user id: {Id}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                _logger.LogWarning("[TokenService - CreateAccessTokenAsync] User not found for id: {Id}", userId);
                return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
            }

            _logger.LogInformation("[TokenService - CreateAccessTokenAsync] User found. Creating JWT token for user: {UserName}, id: {Id}", user.UserName, user.Id);

            var accessToken = CreateJwtToken(user);

            _logger.LogInformation("[TokenService - CreateAccessTokenAsync] JWT token created successfully for user id: {Id}", userId);

            return accessToken;
        }

        private string CreateJwtToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(_tokenSettings.AccessTokenLifeInSeconds)),
                SigningCredentials = creds,
                Issuer = _tokenSettings.Issuer,
                Audience = _tokenSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writtenToken = tokenHandler.WriteToken(token);

            return writtenToken;
        }
    }
}
