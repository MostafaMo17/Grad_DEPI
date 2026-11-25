using MediCare.Services.Dtos;
using MediCare.Services.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface ITokenService
    {
        // Returns the token text. expiresAtUtc tells when the token stops working
        string CreateToken(ApplicationUser user, out DateTime expiresAtUtc);

        // short lived token 5 minutes و accepted ONLY by the notification hub
        string CreateHubToken(int userId, out DateTime expiresAtUtc);
    }
}
