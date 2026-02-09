namespace BrandVault.Api.Features.Reviews;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// CRUD endpoints for review link management (agency-side).
///
/// Express equivalent:
///   router.get('/', getAll);                     // ?workspaceId required
///   router.post('/', requireRole('Admin','Manager'), create);
///   router.put('/:id', requireRole('Admin','Manager'), update);
///   router.delete('/:id', requireRole('Admin'), delete);
/// </summary>
[ApiController]
[Route("api/review-links")]
[Authorize]
public class ReviewLinksController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewLinksController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// GET /api/review-links?workspaceId=xxx
    /// Lists all review links for a workspace.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid workspaceId)
    {
        var links = await _reviewService.GetReviewLinksAsync(workspaceId);
        return Ok(links);
    }

    /// <summary>
    /// POST /api/review-links
    /// Creates a new tokenized review link. Admin + Manager only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateReviewLinkRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var link = await _reviewService.CreateReviewLinkAsync(request, userId);
        return Created("", link);
    }

    /// <summary>
    /// PUT /api/review-links/:id
    /// Update review link (toggle active, change expiry). Admin + Manager only.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewLinkRequest request)
    {
        var link = await _reviewService.UpdateReviewLinkAsync(id, request);
        return Ok(link);
    }

    /// <summary>
    /// DELETE /api/review-links/:id
    /// Delete a review link. Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _reviewService.DeleteReviewLinkAsync(id);
        return NoContent();
    }
}
