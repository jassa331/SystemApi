using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemApi.DAL.Data;
using SystemApi.DAL.IRepository;

namespace SystemApi.Controllers
{
    [ApiController]
    [Route("api/simple")]
    public class SimpleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SimpleController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles ="Admin")]

        [HttpGet("MY")]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Users
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("claSims")]
        public IActionResult ClaimsCheck()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }
        [HttpGet]
        [Authorize]
        public IActionResult GetW()
        {
            return Ok("JWT WORKING 🔥");
        }

        [HttpGet("claims")]
        public IActionResult Claims()
        {
            return Ok(User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }));
        }
    }

}
