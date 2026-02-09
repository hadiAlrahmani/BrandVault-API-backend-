namespace BrandVault.Api.Features.Reviews;

using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Common;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Assets.DTOs;
using BrandVault.Api.Features.Reviews.DTOs;
using BrandVault.Api.Hubs;
using BrandVault.Api.Models;
using BrandVault.Api.Models.Enums;

/// <summary>
/// Review link management + public review endpoints.
///
/// Express equivalent:
///   class ReviewService {
///     async createLink(workspaceId, userId) { token = crypto.randomBytes(32).toString('base64url'); ... }
///     async getPublicReview(token) { validate(token); return workspace + assets; }
///     async addClientComment(token, assetId, { authorName, content }) { ... }
///   }
/// </summary>
public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<ReviewHub> _hubContext;

    public ReviewService(AppDbContext context, IHubContext<ReviewHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // =========================================================================
    // REVIEW LINK CRUD (agency-side)
    // =========================================================================

    public async Task<List<ReviewLinkResponse>> GetReviewLinksAsync(Guid workspaceId)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        var links = await _context.ReviewLinks
            .Include(rl => rl.Workspace).ThenInclude(w => w.Client)
            .Include(rl => rl.CreatedBy)
            .Where(rl => rl.WorkspaceId == workspaceId)
            .OrderByDescending(rl => rl.CreatedAt)
            .ToListAsync();

        return links.Select(MapToResponse).ToList();
    }

    public async Task<ReviewLinkResponse> CreateReviewLinkAsync(CreateReviewLinkRequest request, Guid userId)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == request.WorkspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        // Generate a cryptographically secure URL-safe token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var reviewLink = new ReviewLink
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays),
            IsActive = true,
            WorkspaceId = request.WorkspaceId,
            CreatedById = userId
        };

        _context.ReviewLinks.Add(reviewLink);
        await _context.SaveChangesAsync();

        await _context.Entry(reviewLink).Reference(rl => rl.Workspace).LoadAsync();
        await _context.Entry(reviewLink.Workspace).Reference(w => w.Client).LoadAsync();
        await _context.Entry(reviewLink).Reference(rl => rl.CreatedBy).LoadAsync();

        return MapToResponse(reviewLink);
    }

    public async Task<ReviewLinkResponse> UpdateReviewLinkAsync(Guid id, UpdateReviewLinkRequest request)
    {
        var reviewLink = await _context.ReviewLinks
            .Include(rl => rl.Workspace).ThenInclude(w => w.Client)
            .Include(rl => rl.CreatedBy)
            .FirstOrDefaultAsync(rl => rl.Id == id);

        if (reviewLink is null)
        {
            throw new ApiException("Review link not found", 404);
        }

        reviewLink.IsActive = request.IsActive;

        if (request.ExpiresAt.HasValue)
        {
            reviewLink.ExpiresAt = request.ExpiresAt.Value;
        }

        await _context.SaveChangesAsync();

        return MapToResponse(reviewLink);
    }

    public async Task DeleteReviewLinkAsync(Guid id)
    {
        var reviewLink = await _context.ReviewLinks.FindAsync(id);

        if (reviewLink is null)
        {
            throw new ApiException("Review link not found", 404);
        }

        _context.ReviewLinks.Remove(reviewLink);
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // PUBLIC REVIEW (client-side, token-validated)
    // =========================================================================

    public async Task<PublicReviewResponse> GetPublicReviewAsync(string token)
    {
        var reviewLink = await ValidateTokenAsync(token);

        var workspace = await _context.Workspaces
            .Include(w => w.Client)
            .Include(w => w.Assets)
            .FirstAsync(w => w.Id == reviewLink.WorkspaceId);

        return new PublicReviewResponse
        {
            WorkspaceName = workspace.Name,
            ClientName = workspace.Client.Name,
            ExpiresAt = reviewLink.ExpiresAt,
            Assets = workspace.Assets
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new PublicAssetSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                    FileType = a.FileType,
                    CurrentVersion = a.CurrentVersion,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<AssetDetailResponse> GetPublicAssetAsync(string token, Guid assetId)
    {
        var reviewLink = await ValidateTokenAsync(token);

        var asset = await _context.Assets
            .Include(a => a.Workspace)
            .Include(a => a.UploadedBy)
            .Include(a => a.Versions).ThenInclude(v => v.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == assetId && a.WorkspaceId == reviewLink.WorkspaceId);

        if (asset is null)
        {
            throw new ApiException("Asset not found in this workspace", 404);
        }

        return new AssetDetailResponse
        {
            Id = asset.Id,
            Name = asset.Name,
            FileType = asset.FileType,
            CurrentVersion = asset.CurrentVersion,
            Status = asset.Status.ToString(),
            WorkspaceId = asset.WorkspaceId,
            WorkspaceName = asset.Workspace.Name,
            UploadedById = asset.UploadedById,
            UploadedByName = asset.UploadedBy.Name,
            CreatedAt = asset.CreatedAt,
            Versions = asset.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new AssetVersionResponse
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    FilePath = v.FilePath,
                    ThumbnailPath = v.ThumbnailPath,
                    FileSize = v.FileSize,
                    UploadedById = v.UploadedById,
                    UploadedByName = v.UploadedBy.Name,
                    UploadedAt = v.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<CommentResponse> CreateClientCommentAsync(string token, Guid assetId, PublicCommentRequest request)
    {
        var reviewLink = await ValidateTokenAsync(token);

        // Ensure the asset belongs to the review link's workspace
        var assetExists = await _context.Assets
            .AnyAsync(a => a.Id == assetId && a.WorkspaceId == reviewLink.WorkspaceId);

        if (!assetExists)
        {
            throw new ApiException("Asset not found in this workspace", 404);
        }

        var comment = new Comment
        {
            AuthorName = request.AuthorName,
            AuthorType = AuthorType.Client,
            Content = request.Content,
            AssetId = assetId,
            ReviewLinkId = reviewLink.Id
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var response = new CommentResponse
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            AuthorType = comment.AuthorType.ToString(),
            Content = comment.Content,
            AssetId = comment.AssetId,
            ReviewLinkId = comment.ReviewLinkId,
            CreatedAt = comment.CreatedAt
        };

        // Broadcast to connected agency users in this workspace
        // Like: io.to(`workspace_${id}`).emit('NewComment', response)
        await _hubContext.Clients
            .Group($"workspace_{reviewLink.WorkspaceId}")
            .SendAsync("NewComment", response);

        return response;
    }

    public async Task<ApprovalResponse> CreateClientApprovalAsync(string token, Guid assetId, PublicApprovalRequest request)
    {
        var reviewLink = await ValidateTokenAsync(token);

        if (!Enum.TryParse<ApprovalActionType>(request.ActionType, ignoreCase: true, out var actionType))
        {
            throw new ApiException(
                $"Invalid action type '{request.ActionType}'. Valid values: Approved, RevisionRequested", 400);
        }

        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.WorkspaceId == reviewLink.WorkspaceId);

        if (asset is null)
        {
            throw new ApiException("Asset not found in this workspace", 404);
        }

        var action = new ApprovalAction
        {
            ActionType = actionType,
            Comment = request.Comment,
            DoneByName = request.AuthorName,
            DoneByType = AuthorType.Client,
            AssetId = assetId,
            ReviewLinkId = reviewLink.Id
        };

        // Update asset status to match the approval action
        asset.Status = actionType == ApprovalActionType.Approved
            ? AssetStatus.Approved
            : AssetStatus.RevisionRequested;

        _context.ApprovalActions.Add(action);
        await _context.SaveChangesAsync();

        var response = new ApprovalResponse
        {
            Id = action.Id,
            ActionType = action.ActionType.ToString(),
            Comment = action.Comment,
            DoneByName = action.DoneByName,
            DoneByType = action.DoneByType.ToString(),
            AssetId = action.AssetId,
            ReviewLinkId = action.ReviewLinkId,
            CreatedAt = action.CreatedAt
        };

        // Broadcast to connected agency users in this workspace
        // Like: io.to(`workspace_${id}`).emit('NewApproval', response)
        await _hubContext.Clients
            .Group($"workspace_{reviewLink.WorkspaceId}")
            .SendAsync("NewApproval", response);

        return response;
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Validates a review link token: exists, active, not expired.
    /// Returns the ReviewLink entity if valid.
    /// </summary>
    private async Task<ReviewLink> ValidateTokenAsync(string token)
    {
        var reviewLink = await _context.ReviewLinks
            .FirstOrDefaultAsync(rl => rl.Token == token);

        if (reviewLink is null)
        {
            throw new ApiException("Review link not found", 404);
        }

        if (!reviewLink.IsActive)
        {
            throw new ApiException("Review link is no longer active", 401);
        }

        if (reviewLink.ExpiresAt < DateTime.UtcNow)
        {
            throw new ApiException("Review link has expired", 401);
        }

        return reviewLink;
    }

    private static ReviewLinkResponse MapToResponse(ReviewLink reviewLink)
    {
        return new ReviewLinkResponse
        {
            Id = reviewLink.Id,
            Token = reviewLink.Token,
            ExpiresAt = reviewLink.ExpiresAt,
            IsActive = reviewLink.IsActive,
            WorkspaceId = reviewLink.WorkspaceId,
            WorkspaceName = reviewLink.Workspace.Name,
            CreatedById = reviewLink.CreatedById,
            CreatedByName = reviewLink.CreatedBy.Name,
            CreatedAt = reviewLink.CreatedAt
        };
    }
}
