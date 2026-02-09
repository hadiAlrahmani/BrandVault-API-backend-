namespace BrandVault.Api.Features.Workspaces;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Workspaces.DTOs;

/// <summary>
/// CRUD endpoints for workspaces + team assignment management.
///
/// Express equivalent:
///   router.get('/', getAll);                           // ?clientId optional filter
///   router.get('/:id', getById);
///   router.post('/', requireRole('Admin','Manager'), create);
///   router.put('/:id', requireRole('Admin','Manager'), update);
///   router.delete('/:id', requireRole('Admin'), delete);
///   router.get('/:id/assignments', getAssignments);
///   router.post('/:id/assignments', requireRole('Admin','Manager'), assignUser);
///   router.delete('/:id/assignments/:userId', requireRole('Admin','Manager'), unassignUser);
/// </summary>
[ApiController]
[Route("api/workspaces")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    // =========================================================================
    // WORKSPACE CRUD
    // =========================================================================

    /// <summary>
    /// GET /api/workspaces?clientId=xxx
    /// Lists all workspaces. Optionally filter by clientId.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? clientId)
    {
        var workspaces = await _workspaceService.GetAllAsync(clientId);
        return Ok(workspaces);
    }

    /// <summary>
    /// GET /api/workspaces/:id
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workspace = await _workspaceService.GetByIdAsync(id);
        return Ok(workspace);
    }

    /// <summary>
    /// POST /api/workspaces
    /// Creates a new workspace under a client. Admin + Manager only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var workspace = await _workspaceService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace);
    }

    /// <summary>
    /// PUT /api/workspaces/:id
    /// Updates workspace name, description, deadline, status. Admin + Manager only.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkspaceRequest request)
    {
        var workspace = await _workspaceService.UpdateAsync(id, request);
        return Ok(workspace);
    }

    /// <summary>
    /// DELETE /api/workspaces/:id
    /// Deletes workspace and cascades to assets, assignments, review links. Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _workspaceService.DeleteAsync(id);
        return NoContent();
    }

    // =========================================================================
    // ASSIGNMENT MANAGEMENT
    // =========================================================================

    /// <summary>
    /// GET /api/workspaces/:id/assignments
    /// Lists all users assigned to a workspace.
    /// </summary>
    [HttpGet("{id:guid}/assignments")]
    public async Task<IActionResult> GetAssignments(Guid id)
    {
        var assignments = await _workspaceService.GetAssignmentsAsync(id);
        return Ok(assignments);
    }

    /// <summary>
    /// POST /api/workspaces/:id/assignments
    /// Assigns a user to the workspace. Admin + Manager only.
    /// </summary>
    [HttpPost("{id:guid}/assignments")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AssignUser(Guid id, [FromBody] AssignUserRequest request)
    {
        var assignment = await _workspaceService.AssignUserAsync(id, request);
        return Created("", assignment);
    }

    /// <summary>
    /// DELETE /api/workspaces/:id/assignments/:userId
    /// Removes a user from the workspace. Admin + Manager only.
    /// </summary>
    [HttpDelete("{id:guid}/assignments/{userId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UnassignUser(Guid id, Guid userId)
    {
        await _workspaceService.UnassignUserAsync(id, userId);
        return NoContent();
    }
}
