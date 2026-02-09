namespace BrandVault.Api.Features.Assets;

using BrandVault.Api.Features.Assets.DTOs;

public interface IAssetService
{
    // Asset CRUD
    Task<List<AssetResponse>> GetAllAsync(Guid workspaceId);
    Task<AssetDetailResponse> GetByIdAsync(Guid id);
    Task<AssetResponse> CreateAsync(CreateAssetRequest request, Guid userId);
    Task<AssetResponse> UpdateAsync(Guid id, UpdateAssetRequest request);
    Task DeleteAsync(Guid id);

    // Versions
    Task<List<AssetVersionResponse>> GetVersionsAsync(Guid assetId);
    Task<AssetVersionResponse> CreateVersionAsync(Guid assetId, CreateVersionRequest request, Guid userId);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadVersionAsync(Guid assetId, int versionNumber);

    // Comments
    Task<List<CommentResponse>> GetCommentsAsync(Guid assetId);
    Task<CommentResponse> CreateCommentAsync(Guid assetId, CreateCommentRequest request, Guid userId);

    // Approvals
    Task<List<ApprovalResponse>> GetApprovalsAsync(Guid assetId);
    Task<ApprovalResponse> CreateApprovalAsync(Guid assetId, CreateApprovalRequest request, Guid userId);
}
