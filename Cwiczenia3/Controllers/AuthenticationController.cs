using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cwiczenia7.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Cwiczenia5.Services;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Cwiczenia7.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IStudentDbService _service;
        public IConfiguration Configuration { get; set; }
        public AuthenticationController(IConfiguration configuration, IStudentDbService service)
        {
            Configuration = configuration;
            _service = service;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var login = request.Login;
            var passwordValue = request.Password;
            var passwordHashDb = _service.GetPassword(login);

            if(passwordHashDb == "NO_SUCH_USER")
            {
                return Unauthorized(passwordHashDb);
            }

            if(passwordHashDb == null)
            {
                var salt = GenerateSalt();
                passwordHashDb = CreatePasswordHash(passwordValue, salt);
                _service.CreatePassword(passwordHashDb, salt, login);
            }

            var saltDb = _service.GetSalt(login);

            if (!ValidatePasswordHash(passwordValue, saltDb, passwordHashDb))
            {
                return Unauthorized("Wrong password");
            }

            var refreshToken = Guid.NewGuid();
            _service.UpdateRefreshToken(login, refreshToken);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(GenerateToken(request.Login)),
                refreshToken
            });
        }
        public static string CreatePasswordHash(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                password: value,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }

        public static bool ValidatePasswordHash(string value, string salt, string hash)
            => CreatePasswordHash(value, salt) == hash;

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(RefreshTokenRequest request)
        {
            var refreshTokenDb = _service.GetRefreshToken(request.IndexNumber);

            if (refreshTokenDb == "NO_SUCH_USER")
            {
                return Unauthorized(refreshTokenDb);
            }else if(refreshTokenDb != request.RefreshToken)
            {
                return Unauthorized("WRONG_REFRESH_TOKEN");
            }

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(GenerateToken(request.IndexNumber))
            });
        }
        public string GenerateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
        public JwtSecurityToken GenerateToken(string indexNumber)
        {
            ICollection<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, indexNumber));

            var roles = _service.GetUserRoles(indexNumber);

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "APBD",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds
            );
            return token;
        }
    }
}