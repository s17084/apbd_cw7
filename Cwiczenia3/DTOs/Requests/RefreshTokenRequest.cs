using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia7.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        public string IndexNumber { get; set; }
        public string RefreshToken { get; set; }
    }
}
