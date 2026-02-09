namespace BrandVault.Api.Features.Assets.DTOs;

public class ApprovalResponse
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string DoneByName { get; set; } = string.Empty;
    public string DoneByType { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public Guid? ReviewLinkId { get; set; }
    public DateTime CreatedAt { get; set; }
}
