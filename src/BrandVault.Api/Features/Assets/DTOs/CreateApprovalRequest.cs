namespace BrandVault.Api.Features.Assets.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// JSON body for POST /api/assets/:id/approvals.
/// ActionType must be "Approved" or "RevisionRequested".
/// Comment is optional (typically used for revision requests).
/// </summary>
public class CreateApprovalRequest
{
    [Required(ErrorMessage = "ActionType is required")]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Comment { get; set; }
}
