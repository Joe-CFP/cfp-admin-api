using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        // later: inject service/repo and return real data
        return Ok(new[] { new { Id = 1, Name = "Test Org" } });
    }
}
