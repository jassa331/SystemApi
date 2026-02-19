//using BCrypt.Net;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using SystemApi.BLL.Interface;
//using SystemApi.DAL.IRepository;
//using SystemApi.DTO;
//using SystemApi.Entities;
//namespace SystemApi.BLL.Services
//{
//    public class JWTService: IAuthService
//    {
//        public readonly IAuthRepository _authRepository;
//        public readonly IConfiguration _configuration;
//        public JWTService(IAuthRepository authRepository, IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _authRepository = authRepository;
//        }

//        public async Task RegisterAsync(RegisterRequestDto dto)
//        {
//            var user = new User
//            {
//                Name = dto.Name,
//                Email = dto.Email,
//                PhoneNumber = dto.PhoneNumber,
//                Role = dto.Role,
//                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
//            };

//            await _authRepository.RegisterUserAsync(user);
//        }

//        public async Task<string> LoginAsync(loginDto dto)
//        {
//            var user = await _authRepository.GetUserByEmailAsync(dto.Email);
//            if (user == null)
//            {
//                throw new UnauthorizedAccessException("Invalid EMAIL");

//            }
//            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
//                throw new UnauthorizedAccessException("Invalid Password");
//            return GenerateJwt(user);
//        }
//        private string GenerateJwt(User user)
//        {
//            var claims = new List<Claim>
//    {
//        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//        new Claim(ClaimTypes.Email, user.Email),
//        new Claim(ClaimTypes.Role, user.Role) // 🔥 ROLE ADD HERE
//    };

//            var key = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
//            );

//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: _configuration["Jwt:Issuer"],
//                audience: null,
//                claims: claims,
//                expires: DateTime.UtcNow.AddHours(2),
//                signingCredentials: creds
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }



//    }
//}

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using SystemApi.BLL.Interface;
using SystemApi.DAL.IRepository;
using SystemApi.DTO;
using SystemApi.Entities;

namespace ServicePro.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository repository;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository repository, IConfiguration config)
        {
            this.repository = repository;
            this._config = config;
        }

        public async Task RegisterAsync(RegisterRequestDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await repository.RegisterUserAsync(user);
        }

        public async Task<string> LoginAsync(loginDto dto)
        {
            var user = await repository.GetUserByEmailAsync(dto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            return GenerateJwtToken(user); // ✅ ALWAYS JWT
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
{
    new Claim("id", user.Id.ToString()),
    new Claim("name", user.Name),
    new Claim("email", user.Email),
    new Claim("role", user.Role) // 🔥 SIMPLE ROLE CLAIM
};


            var key = new SymmetricSecurityKey(
       Encoding.UTF8.GetBytes(_config["Jwt:Key"])
   );


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
               expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}