namespace BrandVault.Api.Features.Workspaces;

using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Common;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Workspaces.DTOs;
using BrandVault.Api.Models;
using BrandVault.Api.Models.Enums;

/// <summary>
/// Workspace CRUD + team assignment management.
///
/// Express equivalent:
///   class WorkspaceService {
///     async getAll(clientId?) { return prisma.workspace.findMany({ where: clientId ? { clientId } : {}, include: ... }); }
///     async create(dto, userId) { ... }
///     async assignUser(workspaceId, userId) { ... }
///   }
/// </summary>
public class WorkspaceService : IWorkspaceService
{
    private readonly AppDbContext _context;

    public WorkspaceService(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================================
    // WORKSPACE CRUD
    // =========================================================================

    public async Task<List<WorkspaceResponse>> GetAllAsync(Guid? clientId)
    {
        var query = _context.Workspaces
            .Include(w => w.Client)
            .Include(w => w.CreatedBy)
            .Include(w => w.Assignments)
            .AsQueryable();

        if (clientId.HasValue)
        {
            query = query.Where(w => w.ClientId == clientId.Value);
        }

        var workspaces = await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return workspaces.Select(MapToResponse).ToList();
    }

    public async Task<WorkspaceResponse> GetByIdAsync(Guid id)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Client)
            .Include(w => w.CreatedBy)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace is null)
        {
            throw new ApiException("Workspace not found", 404);
        }

        return MapToResponse(workspace);
    }

    public async Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, Guid userId)
    {
        // Validate client exists
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists)
        {
            throw new ApiException("Client not found", 404);
        }

        var workspace = new Workspace
        {
            Name = request.Name,
            Description = request.Description,
            Deadline = request.Deadline,
            Status = WorkspaceStatus.Active,
            ClientId = request.ClientId,
            CreatedById = userId
        };

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();

        // Reload with navigations for the response
        await _context.Entry(workspace).Reference(w => w.Client).LoadAsync();
        await _context.Entry(workspace).Reference(w => w.CreatedBy).LoadAsync();
        await _context.Entry(workspace).Collection(w => w.Assignments).LoadAsync();

        return MapToResponse(workspace);
    }

    public async Task<WorkspaceResponse> UpdateAsync(Guid id, UpdateWorkspaceRequest request)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Client)
            .Include(w => w.CreatedBy)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace is null)
        {
            throw new ApiException("Workspace not found", 404);
        }

        // Validate status enum value
        if (!Enum.TryParse<WorkspaceStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ApiException(
                $"Invalid status '{request.Status}'. Valid values: Active, InReview, Completed, Archived", 400);
        }

        workspace.Name = request.Name;
        workspace.Description = request.Description;
        workspace.Deadline = request.Deadline;
        workspace.Status = status;

        await _context.SaveChangesAsync();

        return MapToResponse(workspace);
    }

    public async Task DeleteAsync(Guid id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);

        if (workspace is null)
        {
            throw new ApiException("Workspace not found", 404);
        }

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // ASSIGNMENT MANAGEMENT
    // =========================================================================

    public async Task<List<AssignmentResponse>> GetAssignmentsAsync(Guid workspaceId)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        var assignments = await _context.WorkspaceAssignments
            .Include(wa => wa.User)
            .Where(wa => wa.WorkspaceId == workspaceId)
            .OrderBy(wa => wa.CreatedAt)
            .ToListAsync();

        return assignments.Select(MapToAssignmentResponse).ToList();
    }

    public async Task<AssignmentResponse> AssignUserAsync(Guid workspaceId, AssignUserRequest request)
    {
        // Validate workspace exists
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        // Validate user exists
        var user = await _context.Users.FindAsync(request.UserId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        // Check for duplicate assignment
        var alreadyAssigned = await _context.WorkspaceAssignments
            .AnyAsync(wa => wa.WorkspaceId == workspaceId && wa.UserId == request.UserId);

        if (alreadyAssigned)
        {
            throw new ApiException("User is already assigned to this workspace", 409);
        }

        var assignment = new WorkspaceAssignment
        {
            WorkspaceId = workspaceId,
            UserId = request.UserId
        };

        _context.WorkspaceAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        // Reload user navigation for the response
        await _context.Entry(assignment).Reference(a => a.User).LoadAsync();

        return MapToAssignmentResponse(assignment);
    }

    public async Task UnassignUserAsync(Guid workspaceId, Guid userId)
    {
        var assignment = await _context.WorkspaceAssignments
            .FirstOrDefaultAsync(wa => wa.WorkspaceId == workspaceId && wa.UserId == userId);

        if (assignment is null)
        {
            throw new ApiException("Assignment not found", 404);
        }

        _context.WorkspaceAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private static WorkspaceResponse MapToResponse(Workspace workspace)
    {
        return new WorkspaceResponse
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Deadline = workspace.Deadline,
            Status = workspace.Status.ToString(),
            ClientId = workspace.ClientId,
            ClientName = workspace.Client.Name,
            CreatedById = workspace.CreatedById,
            CreatedByName = workspace.CreatedBy.Name,
            CreatedAt = workspace.CreatedAt,
            AssignmentCount = workspace.Assignments.Count
        };
    }

    private static AssignmentResponse MapToAssignmentResponse(WorkspaceAssignment assignment)
    {
        return new AssignmentResponse
        {
            Id = assignment.Id,
            UserId = assignment.UserId,
            UserName = assignment.User.Name,
            UserRole = assignment.User.Role.ToString(),
            AssignedAt = assignment.CreatedAt
        };
    }
}
