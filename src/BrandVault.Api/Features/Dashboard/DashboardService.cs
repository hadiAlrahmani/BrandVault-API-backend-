namespace BrandVault.Api.Features.Dashboard;

using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Data;

/// <summary>
/// Aggregates stats across all entities for the dashboard.
///
/// Express equivalent:
///   async getDashboard() {
///     const [clients, workspaces, assets, ...] = await Promise.all([
///       prisma.client.count(),
///       prisma.workspace.count(),
///       prisma.asset.groupBy({ by: ['status'], _count: true }),
///       ...
///     ]);
///     return { totalClients: clients, ... };
///   }
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var totalClients = await _context.Clients.CountAsync();
        var totalWorkspaces = await _context.Workspaces.CountAsync();
        var totalAssets = await _context.Assets.CountAsync();

        // Group assets by status — e.g. { "Draft": 5, "Approved": 3 }
        var assetsByStatus = await _context.Assets
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var activeReviewLinks = await _context.ReviewLinks
            .CountAsync(rl => rl.IsActive && rl.ExpiresAt > DateTime.UtcNow);

        var totalComments = await _context.Comments.CountAsync();
        var totalApprovals = await _context.ApprovalActions.CountAsync();

        return new DashboardResponse
        {
            TotalClients = totalClients,
            TotalWorkspaces = totalWorkspaces,
            TotalAssets = totalAssets,
            AssetsByStatus = assetsByStatus,
            ActiveReviewLinks = activeReviewLinks,
            TotalComments = totalComments,
            TotalApprovals = totalApprovals
        };
    }
}
