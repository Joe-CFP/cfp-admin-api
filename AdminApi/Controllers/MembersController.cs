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

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        MemberPreview? member = await db.GetMemberPreviewByEmailAsync(email);
        return member is not null ? Ok(member) : NotFound();
    }

    [HttpGet("{id:int}/journey")]
    public async Task<IActionResult> GetJourney(int id)
    {
        UserJourney? journey = await db.GetMemberJourneyByIdAsync(id);
        return journey is not null ? Ok(journey) : NotFound();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<IActionResult> GetActivity(int id)
    {
        MemberActivity? activity = await db.GetMemberActivityByIdAsync(id);
        return activity is not null ? Ok(activity) : NotFound();
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required");
        IEnumerable<MemberPreview> results = await db.SearchMembersAsync(query, limit);
        return Ok(results);
    }
    
    [HttpGet("{id:int}/saved-searches")]
    public async Task<IActionResult> GetSavedSearches(int id)
    {
        IEnumerable<SavedSearch> searches = await db.GetSavedSearchesByMemberIdAsync(id);
        return Ok(searches);
    }
}