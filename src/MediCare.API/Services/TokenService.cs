using MediCare.Services.Dtos;
using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace MediCare.API.Services
{

    // Creates the JWT after a successful login or registration
    // Registered in Program as AddScoped<ITokenService, TokenService>()
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        // The key we sign the token with. It is the same key that AddJwtBearer uses in Program to verify it
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;

            string signingKey = config["Jwt:SigningKey"]
                ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        public string CreateToken(ApplicationUser user, out DateTime expiresAtUtc)
        {
            // the information inside the token. We keep it to the user id only
            // The role, email and approval status are NOT in the token , they are read from the database on every request
            List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            int lifetimeDays = _config.GetValue<int>("Jwt:AccessTokenLifetimeDays");
            expiresAtUtc = DateTime.UtcNow.AddDays(lifetimeDays);


            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAtUtc,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            };

            // Returns the token as text (header.payload.signature)
            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }
    }
}
