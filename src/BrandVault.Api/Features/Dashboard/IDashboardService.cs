namespace BrandVault.Api.Features.Dashboard;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync();
}
