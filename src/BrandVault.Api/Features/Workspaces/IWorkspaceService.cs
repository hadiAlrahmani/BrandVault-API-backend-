namespace BrandVault.Api.Features.Workspaces;

using BrandVault.Api.Features.Workspaces.DTOs;

public interface IWorkspaceService
{
    Task<List<WorkspaceResponse>> GetAllAsync(Guid? clientId);
    Task<WorkspaceResponse> GetByIdAsync(Guid id);
    Task<WorkspaceResponse> CreateAsync(CreateWorkspaceRequest request, Guid userId);
    Task<WorkspaceResponse> UpdateAsync(Guid id, UpdateWorkspaceRequest request);
    Task DeleteAsync(Guid id);
    Task<List<AssignmentResponse>> GetAssignmentsAsync(Guid workspaceId);
    Task<AssignmentResponse> AssignUserAsync(Guid workspaceId, AssignUserRequest request);
    Task UnassignUserAsync(Guid workspaceId, Guid userId);
}
