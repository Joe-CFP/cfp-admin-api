using AdminApi.Cache;
using AdminApi.Entities;
using AdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController(IOrganisationCache cache, IDatabaseRepository db) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        IReadOnlyList<OrganisationPreview> orgs = cache.GetAll();
        return Ok(orgs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Organisation? org = await db.GetOrganisationByIdAsync(id);
        return org is not null ? Ok(org) : NotFound();
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required");

        List<OrganisationPreview> results = cache.Search(query, limit);
        return Ok(results);
    }

    [HttpGet("{id:int}/activity")]
    public async Task<IActionResult> GetActivity(int id)
    {
        MemberActivity activity = await db.GetOrganisationMemberActivityAsync(id);
        return Ok(activity);
    }
}