using Microsoft.AspNetCore.Mvc;

namespace ProjectManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API hoạt động bình thường");
        }
    }
} 