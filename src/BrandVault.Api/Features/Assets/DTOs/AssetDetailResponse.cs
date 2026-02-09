namespace BrandVault.Api.Features.Assets.DTOs;

/// <summary>
/// Extended response for GET /api/assets/:id.
/// Includes the full version history alongside the base asset info.
/// </summary>
public class AssetDetailResponse : AssetResponse
{
    public List<AssetVersionResponse> Versions { get; set; } = new();
}
