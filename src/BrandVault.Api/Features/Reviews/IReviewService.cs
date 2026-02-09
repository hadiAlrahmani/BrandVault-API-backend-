namespace BrandVault.Api.Features.Reviews;

using BrandVault.Api.Features.Assets.DTOs;
using BrandVault.Api.Features.Reviews.DTOs;

public interface IReviewService
{
    // Review Link CRUD (agency-side)
    Task<List<ReviewLinkResponse>> GetReviewLinksAsync(Guid workspaceId);
    Task<ReviewLinkResponse> CreateReviewLinkAsync(CreateReviewLinkRequest request, Guid userId);
    Task<ReviewLinkResponse> UpdateReviewLinkAsync(Guid id, UpdateReviewLinkRequest request);
    Task DeleteReviewLinkAsync(Guid id);

    // Public Review (client-side, validated by token)
    Task<PublicReviewResponse> GetPublicReviewAsync(string token);
    Task<AssetDetailResponse> GetPublicAssetAsync(string token, Guid assetId);
    Task<CommentResponse> CreateClientCommentAsync(string token, Guid assetId, PublicCommentRequest request);
    Task<ApprovalResponse> CreateClientApprovalAsync(string token, Guid assetId, PublicApprovalRequest request);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadPublicVersionAsync(string token, Guid assetId, int versionNumber);
}
