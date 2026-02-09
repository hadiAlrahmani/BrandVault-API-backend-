namespace BrandVault.Api.Features.Assets;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Assets.DTOs;

/// <summary>
/// Asset CRUD + version management + comments + approvals.
///
/// Express equivalent:
///   router.get('/', getAll);                                    // ?workspaceId required
///   router.get('/:id', getById);
///   router.post('/', upload.single('file'), create);            // multipart form
///   router.put('/:id', requireRole('Admin','Manager'), update);
///   router.delete('/:id', requireRole('Admin'), delete);
///   router.get('/:id/versions', getVersions);
///   router.post('/:id/versions', upload.single('file'), createVersion);
///   router.get('/:id/versions/:vn/download', downloadVersion);
///   router.get('/:id/comments', getComments);
///   router.post('/:id/comments', createComment);
///   router.get('/:id/approvals', getApprovals);
///   router.post('/:id/approvals', requireRole('Admin','Manager'), createApproval);
/// </summary>
[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    // =========================================================================
    // ASSET CRUD
    // =========================================================================

    /// <summary>
    /// GET /api/assets?workspaceId=xxx
    /// Lists all assets in a workspace. workspaceId is required.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid workspaceId)
    {
        var assets = await _assetService.GetAllAsync(workspaceId);
        return Ok(assets);
    }

    /// <summary>
    /// GET /api/assets/:id
    /// Returns asset detail with full version history.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        return Ok(asset);
    }

    /// <summary>
    /// POST /api/assets
    /// Upload a new asset (multipart form: file + name + workspaceId).
    /// Admin, Manager, or Designer can upload.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Designer")]
    [RequestSizeLimit(52_428_800)] // 50MB
    public async Task<IActionResult> Create([FromForm] CreateAssetRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var asset = await _assetService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = asset.Id }, asset);
    }

    /// <summary>
    /// PUT /api/assets/:id
    /// Update asset metadata (name, status). Admin + Manager only.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssetRequest request)
    {
        var asset = await _assetService.UpdateAsync(id, request);
        return Ok(asset);
    }

    /// <summary>
    /// DELETE /api/assets/:id
    /// Delete asset and all version files from disk. Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _assetService.DeleteAsync(id);
        return NoContent();
    }

    // =========================================================================
    // VERSION MANAGEMENT
    // =========================================================================

    /// <summary>
    /// GET /api/assets/:id/versions
    /// Lists all versions of an asset (newest first).
    /// </summary>
    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var versions = await _assetService.GetVersionsAsync(id);
        return Ok(versions);
    }

    /// <summary>
    /// POST /api/assets/:id/versions
    /// Upload a new version (multipart form: file).
    /// Auto-increments the version number.
    /// </summary>
    [HttpPost("{id:guid}/versions")]
    [Authorize(Roles = "Admin,Manager,Designer")]
    [RequestSizeLimit(52_428_800)] // 50MB
    public async Task<IActionResult> CreateVersion(Guid id, [FromForm] CreateVersionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var version = await _assetService.CreateVersionAsync(id, request, userId);
        return Created("", version);
    }

    /// <summary>
    /// GET /api/assets/:id/versions/:versionNumber/download
    /// Download a specific version's file.
    /// </summary>
    [HttpGet("{id:guid}/versions/{versionNumber:int}/download")]
    public async Task<IActionResult> DownloadVersion(Guid id, int versionNumber)
    {
        var (stream, contentType, fileName) = await _assetService.DownloadVersionAsync(id, versionNumber);
        return File(stream, contentType, fileName);
    }

    // =========================================================================
    // COMMENTS (agency-side)
    // =========================================================================

    /// <summary>
    /// GET /api/assets/:id/comments
    /// Lists all comments on an asset (oldest first).
    /// </summary>
    [HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid id)
    {
        var comments = await _assetService.GetCommentsAsync(id);
        return Ok(comments);
    }

    /// <summary>
    /// POST /api/assets/:id/comments
    /// Add a comment as an agency user. AuthorName is auto-set from JWT.
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comment = await _assetService.CreateCommentAsync(id, request, userId);
        return Created("", comment);
    }

    // =========================================================================
    // APPROVALS (agency-side)
    // =========================================================================

    /// <summary>
    /// GET /api/assets/:id/approvals
    /// Lists all approval actions on an asset (newest first).
    /// </summary>
    [HttpGet("{id:guid}/approvals")]
    public async Task<IActionResult> GetApprovals(Guid id)
    {
        var approvals = await _assetService.GetApprovalsAsync(id);
        return Ok(approvals);
    }

    /// <summary>
    /// POST /api/assets/:id/approvals
    /// Approve an asset or request a revision. Admin + Manager only.
    /// Also updates the asset's status automatically.
    /// </summary>
    [HttpPost("{id:guid}/approvals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateApproval(Guid id, [FromBody] CreateApprovalRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var approval = await _assetService.CreateApprovalAsync(id, request, userId);
        return Created("", approval);
    }
}
