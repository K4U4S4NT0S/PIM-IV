using Microsoft.AspNetCore.Mvc;

namespace FoodSmart.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "online", project = "FOODSMART", version = "2.0" });
}
