using AdminApi.Entities;
using AdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController(IDatabaseRepository db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Member? member = await db.GetMemberByIdAsync(id);
        return member is not null ? Ok(member) : NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required");

        IEnumerable<MemberSearchResult> results = await db.SearchMembersAsync(query, limit);
        return Ok(results);
    }
}