namespace BrandVault.Api.Features.Dashboard;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Single endpoint returning aggregated platform stats.
///
/// Express equivalent:
///   router.get('/', requireAuth, getDashboard);
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// GET /api/dashboard
    /// Returns aggregated stats: total clients, workspaces, assets by status, etc.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}
