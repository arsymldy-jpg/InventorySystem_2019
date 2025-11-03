using Microsoft.AspNetCore.Mvc;
using System;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "API Inventory System is working!",
                Version = "1.0",
                Status = "Running"
            });
        }

        [HttpGet("database")]
        public IActionResult TestDatabase()
        {
            return Ok(new
            {
                Message = "Database connection test endpoint",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}