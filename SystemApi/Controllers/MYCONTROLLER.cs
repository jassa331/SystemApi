using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MYCONTROLLER : ControllerBase
    {
        private readonly ILogger<MYCONTROLLER> _logger;
        private readonly ElasticsearchClient _client;

        public MYCONTROLLER(
            ILogger<MYCONTROLLER> logger,
            ElasticsearchClient client)
        {
            _logger = logger;
            _client = client;
        }

        // ------------------------
        // Reusable method to send logs to Elasticsearch
        // ------------------------
        private async Task LogToElastic(string message, string level)
        {
            try
            {
                await _client.IndexAsync(new
                {
                    message = message,
                    level = level,
                    user = User.Identity?.Name ?? "Anonymous",
                    endpoint = HttpContext.Request.Path,
                    method = HttpContext.Request.Method,
                    timestamp = DateTime.UtcNow
                }, idx => idx.Index("webapi-logs"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log to Elasticsearch");
            }
        }

        // ------------------------
        // Admin only
        // ------------------------
        [HttpGet("admin-data")]
        public async Task<IActionResult> AdminData()
        {
            _logger.LogInformation("AdminData API called");
            await LogToElastic("AdminData API called", "Information");

            return Ok("Admin access granted");
        }

        // ------------------------
        // Admin + User
        // ------------------------
        [HttpGet("user-data")]
        public async Task<IActionResult> UserData()
        {
            _logger.LogInformation("UserData API called");
            await LogToElastic("UserData API called", "Information");

            return Ok("Admin & User both allowed");
        }

        // ------------------------
        // Anyone authenticated
        // ------------------------
        [HttpGet("test")]
        public async Task<IActionResult> TestApi()
        {
            _logger.LogInformation("Test API called");
            await LogToElastic("Test API called", "Information");

            return Ok("JWT is working 🔥");
        }

        // ------------------------
        // Check Claims
        // ------------------------
        [HttpGet("claims")]
        public async Task<IActionResult> Claims()
        {
            var claims = User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            });

            await LogToElastic("Claims endpoint called", "Information");

            return Ok(claims);
        }

        // ------------------------
        // Manual test log endpoint
        // ------------------------
        [HttpGet("testlog")]
        public async Task<IActionResult> TestLog()
        {
            await LogToElastic("Manual log triggered", "Information");
            _logger.LogInformation("Manual log endpoint called");

            return Ok("Log sent to Elasticsearch");
        }
    }
}
