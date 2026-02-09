namespace BrandVault.Api.Features.Reviews;

using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Reviews.DTOs;

/// <summary>
/// Public review endpoints — NO authentication required.
/// External clients access these via the tokenized review link URL.
/// Token validation happens in the service layer.
///
/// Express equivalent:
///   router.get('/:token', getReview);                          // no auth middleware
///   router.get('/:token/assets/:assetId', getAsset);
///   router.post('/:token/assets/:assetId/comments', addComment);
///   router.post('/:token/assets/:assetId/approvals', addApproval);
/// </summary>
[ApiController]
[Route("api/reviews")]
public class PublicReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public PublicReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// GET /api/reviews/:token
    /// Get workspace info + assets list via review link.
    /// </summary>
    [HttpGet("{token}")]
    public async Task<IActionResult> GetReview(string token)
    {
        var review = await _reviewService.GetPublicReviewAsync(token);
        return Ok(review);
    }

    /// <summary>
    /// GET /api/reviews/:token/assets/:assetId
    /// Get a single asset's detail + version history.
    /// </summary>
    [HttpGet("{token}/assets/{assetId:guid}")]
    public async Task<IActionResult> GetAsset(string token, Guid assetId)
    {
        var asset = await _reviewService.GetPublicAssetAsync(token, assetId);
        return Ok(asset);
    }

    /// <summary>
    /// POST /api/reviews/:token/assets/:assetId/comments
    /// Add a comment as an external client.
    /// </summary>
    [HttpPost("{token}/assets/{assetId:guid}/comments")]
    public async Task<IActionResult> AddComment(string token, Guid assetId, [FromBody] PublicCommentRequest request)
    {
        var comment = await _reviewService.CreateClientCommentAsync(token, assetId, request);
        return Created("", comment);
    }

    /// <summary>
    /// POST /api/reviews/:token/assets/:assetId/approvals
    /// Approve or request revision as an external client.
    /// </summary>
    [HttpPost("{token}/assets/{assetId:guid}/approvals")]
    public async Task<IActionResult> AddApproval(string token, Guid assetId, [FromBody] PublicApprovalRequest request)
    {
        var approval = await _reviewService.CreateClientApprovalAsync(token, assetId, request);
        return Created("", approval);
    }

    /// <summary>
    /// GET /api/reviews/:token/assets/:assetId/versions/:versionNumber/download
    /// Download/view a specific version's file via public review link.
    /// </summary>
    [HttpGet("{token}/assets/{assetId:guid}/versions/{versionNumber:int}/download")]
    public async Task<IActionResult> DownloadVersion(string token, Guid assetId, int versionNumber)
    {
        var (stream, contentType, fileName) = await _reviewService.DownloadPublicVersionAsync(token, assetId, versionNumber);
        return File(stream, contentType, fileName);
    }
}
