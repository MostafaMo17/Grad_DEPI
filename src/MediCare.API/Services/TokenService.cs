using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MediCare.API.Services;

// Creates the JWTs
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;

        string signingKey = config["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }

    // The normal token: the user id only. The role, email and approval status are NOT in the token ,they are read from the database on every request.
    public string CreateToken(ApplicationUser user, out DateTime expiresAtUtc)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        int lifetimeDays = _config.GetValue<int>("Jwt:AccessTokenLifetimeDays");
        expiresAtUtc = DateTime.UtcNow.AddDays(lifetimeDays);

        return WriteToken(claims, expiresAtUtc);
    }

    // The hub token: lives 5 minutes and has scope "hub", so it works only for the notification hub
    public string CreateHubToken(int userId, out DateTime expiresAtUtc)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(AppClaimTypes.Scope, AppClaimTypes.HubScope)
        };

        expiresAtUtc = DateTime.UtcNow.AddMinutes(5);

        return WriteToken(claims, expiresAtUtc);
    }

    private string WriteToken(List<Claim> claims, DateTime expires)
    {
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        // Returns the token as text (header.payload.signature)
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}