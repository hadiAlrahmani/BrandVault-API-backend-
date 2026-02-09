namespace BrandVault.Api.Features.Reviews.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for POST /api/reviews/:token/assets/:assetId/approvals.
/// External clients approve or request revisions with their name.
/// </summary>
public class PublicApprovalRequest
{
    [Required(ErrorMessage = "Author name is required")]
    [MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ActionType is required")]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Comment { get; set; }
}
