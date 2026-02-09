namespace BrandVault.Api.Features.Assets.DTOs;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public Guid? ReviewLinkId { get; set; }
    public DateTime CreatedAt { get; set; }
}
