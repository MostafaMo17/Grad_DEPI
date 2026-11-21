using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    //  response of a successful login
    public class LoginResponseDto
    {
        // The client sends it later in the header: Authorization: Bearer <token>
        public string Token { get; set; } = string.Empty;

        // When the token stops working (UTC). MediCare.Web will use it for its cookie lifetime
        public DateTime ExpiresAtUtc { get; set; }

        public UserDto User { get; set; } = new UserDto();
    }
}
