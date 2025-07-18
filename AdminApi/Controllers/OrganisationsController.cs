using AdminApi.Entities;
using AdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController(IDatabaseRepository db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Organisation> orgs = await db.GetAllOrganisationsAsync();
        return Ok(orgs);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required");

        IEnumerable<OrganisationSearchResult> results = await db.SearchOrganisationsAsync(query, limit);
        return Ok(results);
    }
}