using Hangfire.Dashboard;

namespace Tgs.GameRecord.YeeBet;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
