using AdminApi.Cache;
using AdminApi.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController(IOrganisationCache cache) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        IEnumerable<Organisation> orgs = cache.GetAll();
        return Ok(orgs);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Organisation? org = cache.GetById(id);
        return org is not null ? Ok(org) : NotFound();
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required");

        IEnumerable<OrganisationSearchResult> results = cache.Search(query, limit);
        return Ok(results);
    }
}