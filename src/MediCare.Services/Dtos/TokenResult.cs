using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    // What the token service returns: the token text and when it expires
    public class TokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
