namespace BrandVault.Api.Features.Dashboard;

/// <summary>
/// Aggregated stats for the agency dashboard.
///
/// Express equivalent:
///   interface DashboardResponse {
///     totalClients: number;
///     totalWorkspaces: number;
///     totalAssets: number;
///     assetsByStatus: Record&lt;string, number&gt;;
///     activeReviewLinks: number;
///     totalComments: number;
///     totalApprovals: number;
///   }
/// </summary>
public class DashboardResponse
{
    public int TotalClients { get; set; }
    public int TotalWorkspaces { get; set; }
    public int TotalAssets { get; set; }
    public Dictionary<string, int> AssetsByStatus { get; set; } = new();
    public int ActiveReviewLinks { get; set; }
    public int TotalComments { get; set; }
    public int TotalApprovals { get; set; }
}
