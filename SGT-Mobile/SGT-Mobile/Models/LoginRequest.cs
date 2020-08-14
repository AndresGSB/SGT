using System;
using System.Collections.Generic;
using System.Text;

namespace SGTMobile.Models
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Token
    {
        public string token { get; set; }
    }
}
