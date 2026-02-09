namespace BrandVault.Api.Features.Assets;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Common;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Assets.DTOs;
using BrandVault.Api.Models;
using BrandVault.Api.Models.Enums;
using BrandVault.Api.Services.FileStorage;

/// <summary>
/// Asset CRUD + version management + agency comments/approvals.
///
/// Express equivalent:
///   class AssetService {
///     async create(file, dto, userId) { ... }
///     async uploadVersion(assetId, file, userId) { ... }
///     async approve(assetId, actionType, userId) { ... }
///   }
/// </summary>
public class AssetService : IAssetService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public AssetService(AppDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    // =========================================================================
    // ASSET CRUD
    // =========================================================================

    public async Task<List<AssetResponse>> GetAllAsync(Guid workspaceId)
    {
        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        var assets = await _context.Assets
            .Include(a => a.Workspace)
            .Include(a => a.UploadedBy)
            .Where(a => a.WorkspaceId == workspaceId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assets.Select(MapToResponse).ToList();
    }

    public async Task<AssetDetailResponse> GetByIdAsync(Guid id)
    {
        var asset = await _context.Assets
            .Include(a => a.Workspace)
            .Include(a => a.UploadedBy)
            .Include(a => a.Versions).ThenInclude(v => v.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null)
        {
            throw new ApiException("Asset not found", 404);
        }

        return MapToDetailResponse(asset);
    }

    public async Task<AssetResponse> CreateAsync(CreateAssetRequest request, Guid userId)
    {
        _fileStorage.ValidateFile(request.File);

        var workspaceExists = await _context.Workspaces.AnyAsync(w => w.Id == request.WorkspaceId);
        if (!workspaceExists)
        {
            throw new ApiException("Workspace not found", 404);
        }

        var (filePath, fileSize) = await _fileStorage.SaveFileAsync(request.File);
        var fileType = Path.GetExtension(request.File.FileName).ToLowerInvariant();

        var asset = new Asset
        {
            Name = request.Name,
            FileType = fileType,
            CurrentVersion = 1,
            Status = AssetStatus.Draft,
            WorkspaceId = request.WorkspaceId,
            UploadedById = userId
        };

        // Create the first version alongside the asset
        var version = new AssetVersion
        {
            VersionNumber = 1,
            FilePath = filePath,
            ThumbnailPath = null,
            FileSize = fileSize,
            UploadedById = userId
        };

        asset.Versions.Add(version);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Reload navigations for the response
        await _context.Entry(asset).Reference(a => a.Workspace).LoadAsync();
        await _context.Entry(asset).Reference(a => a.UploadedBy).LoadAsync();

        return MapToResponse(asset);
    }

    public async Task<AssetResponse> UpdateAsync(Guid id, UpdateAssetRequest request)
    {
        var asset = await _context.Assets
            .Include(a => a.Workspace)
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null)
        {
            throw new ApiException("Asset not found", 404);
        }

        if (!Enum.TryParse<AssetStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ApiException(
                $"Invalid status '{request.Status}'. Valid values: Draft, InReview, Approved, RevisionRequested", 400);
        }

        asset.Name = request.Name;
        asset.Status = status;

        await _context.SaveChangesAsync();

        return MapToResponse(asset);
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _context.Assets
            .Include(a => a.Versions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null)
        {
            throw new ApiException("Asset not found", 404);
        }

        // Delete all version files from disk
        foreach (var version in asset.Versions)
        {
            await _fileStorage.DeleteFileAsync(version.FilePath);
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // VERSION MANAGEMENT
    // =========================================================================

    public async Task<List<AssetVersionResponse>> GetVersionsAsync(Guid assetId)
    {
        var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
        if (!assetExists)
        {
            throw new ApiException("Asset not found", 404);
        }

        var versions = await _context.AssetVersions
            .Include(v => v.UploadedBy)
            .Where(v => v.AssetId == assetId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(MapToVersionResponse).ToList();
    }

    public async Task<AssetVersionResponse> CreateVersionAsync(Guid assetId, CreateVersionRequest request, Guid userId)
    {
        _fileStorage.ValidateFile(request.File);

        var asset = await _context.Assets.FindAsync(assetId);
        if (asset is null)
        {
            throw new ApiException("Asset not found", 404);
        }

        var (filePath, fileSize) = await _fileStorage.SaveFileAsync(request.File);

        asset.CurrentVersion += 1;

        var version = new AssetVersion
        {
            VersionNumber = asset.CurrentVersion,
            FilePath = filePath,
            ThumbnailPath = null,
            FileSize = fileSize,
            AssetId = assetId,
            UploadedById = userId
        };

        _context.AssetVersions.Add(version);
        await _context.SaveChangesAsync();

        await _context.Entry(version).Reference(v => v.UploadedBy).LoadAsync();

        return MapToVersionResponse(version);
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadVersionAsync(Guid assetId, int versionNumber)
    {
        var version = await _context.AssetVersions
            .Include(v => v.Asset)
            .FirstOrDefaultAsync(v => v.AssetId == assetId && v.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new ApiException("Version not found", 404);
        }

        var absolutePath = _fileStorage.GetFullPath(version.FilePath);
        if (!File.Exists(absolutePath))
        {
            throw new ApiException("File not found on disk", 404);
        }

        // Determine content type from extension
        var fileName = Path.GetFileName(version.FilePath);
        if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
        // Build a clean download name: "assetname_v1.png"
        var downloadName = $"{version.Asset.Name}_v{version.VersionNumber}{version.Asset.FileType}";

        return (stream, contentType, downloadName);
    }

    // =========================================================================
    // COMMENTS (agency-side)
    // =========================================================================

    public async Task<List<CommentResponse>> GetCommentsAsync(Guid assetId)
    {
        var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
        if (!assetExists)
        {
            throw new ApiException("Asset not found", 404);
        }

        var comments = await _context.Comments
            .Where(c => c.AssetId == assetId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToCommentResponse).ToList();
    }

    public async Task<CommentResponse> CreateCommentAsync(Guid assetId, CreateCommentRequest request, Guid userId)
    {
        var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
        if (!assetExists)
        {
            throw new ApiException("Asset not found", 404);
        }

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        var comment = new Comment
        {
            AuthorName = user.Name,
            AuthorType = AuthorType.Agency,
            Content = request.Content,
            AssetId = assetId,
            ReviewLinkId = null
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return MapToCommentResponse(comment);
    }

    // =========================================================================
    // APPROVALS (agency-side)
    // =========================================================================

    public async Task<List<ApprovalResponse>> GetApprovalsAsync(Guid assetId)
    {
        var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
        if (!assetExists)
        {
            throw new ApiException("Asset not found", 404);
        }

        var actions = await _context.ApprovalActions
            .Where(aa => aa.AssetId == assetId)
            .OrderByDescending(aa => aa.CreatedAt)
            .ToListAsync();

        return actions.Select(MapToApprovalResponse).ToList();
    }

    public async Task<ApprovalResponse> CreateApprovalAsync(Guid assetId, CreateApprovalRequest request, Guid userId)
    {
        if (!Enum.TryParse<ApprovalActionType>(request.ActionType, ignoreCase: true, out var actionType))
        {
            throw new ApiException(
                $"Invalid action type '{request.ActionType}'. Valid values: Approved, RevisionRequested", 400);
        }

        var asset = await _context.Assets.FindAsync(assetId);
        if (asset is null)
        {
            throw new ApiException("Asset not found", 404);
        }

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        var action = new ApprovalAction
        {
            ActionType = actionType,
            Comment = request.Comment,
            DoneByName = user.Name,
            DoneByType = AuthorType.Agency,
            AssetId = assetId,
            ReviewLinkId = null
        };

        // Update asset status to match the approval action
        asset.Status = actionType == ApprovalActionType.Approved
            ? AssetStatus.Approved
            : AssetStatus.RevisionRequested;

        _context.ApprovalActions.Add(action);
        await _context.SaveChangesAsync();

        return MapToApprovalResponse(action);
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    private static AssetResponse MapToResponse(Asset asset)
    {
        return new AssetResponse
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
            CreatedAt = asset.CreatedAt
        };
    }

    private static AssetDetailResponse MapToDetailResponse(Asset asset)
    {
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
                .Select(MapToVersionResponse)
                .ToList()
        };
    }

    private static AssetVersionResponse MapToVersionResponse(AssetVersion version)
    {
        return new AssetVersionResponse
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            FilePath = version.FilePath,
            ThumbnailPath = version.ThumbnailPath,
            FileSize = version.FileSize,
            UploadedById = version.UploadedById,
            UploadedByName = version.UploadedBy.Name,
            UploadedAt = version.CreatedAt
        };
    }

    private static CommentResponse MapToCommentResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            AuthorType = comment.AuthorType.ToString(),
            Content = comment.Content,
            AssetId = comment.AssetId,
            ReviewLinkId = comment.ReviewLinkId,
            CreatedAt = comment.CreatedAt
        };
    }

    private static ApprovalResponse MapToApprovalResponse(ApprovalAction action)
    {
        return new ApprovalResponse
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
    }
}
