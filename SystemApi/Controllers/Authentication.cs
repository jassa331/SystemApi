using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemApi.BLL.Interface;
using SystemApi.DTO;

namespace SystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        public readonly IAuthService _authe;
        public Authentication(IAuthService authService)
        {
            _authe = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("please enter valid Candenstials ");
                }
                await _authe.RegisterAsync(dto);
                return Ok("User registered successfully");
            }

            catch (Exception ex)
            {
                return BadRequest("something went wrong ");
            }
        }
        [HttpPost]
        [HttpPost("login")]
        public async Task<IActionResult> Login(loginDto dto)
        {
            if (dto == null)
                return BadRequest("Please enter valid credentials");

            try
            {
                var token = await _authe.LoginAsync(dto);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
    
