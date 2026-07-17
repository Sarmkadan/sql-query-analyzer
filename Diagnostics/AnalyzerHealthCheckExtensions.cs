using System;
using System.Threading.Tasks;

namespace SqlQueryAnalyzer.Diagnostics;

/// <summary>
/// Provides extension methods for <see cref="AnalyzerHealthCheck"/>.
/// </summary>
public static class AnalyzerHealthCheckExtensions
{
	/// <summary>
	/// Checks if the analyzer is healthy.
	/// </summary>
	/// <param name="analyzer">The health check instance.</param>
	/// <returns>True if the status is <see cref="HealthStatus.Healthy"/>; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="analyzer"/> is null.</exception>
	public static async Task<bool> IsHealthyAsync(this AnalyzerHealthCheck analyzer)
	{
		ArgumentNullException.ThrowIfNull(analyzer);
		return (await analyzer.CheckHealthAsync()).Status == HealthStatus.Healthy;
	}

	/// <summary>
	/// Performs a comprehensive health check and attempts self-healing if not healthy.
	/// </summary>
	/// <param name="analyzer">The health check instance.</param>
	/// <returns>The result of the self-healing attempt, if performed; otherwise, null if healthy.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="analyzer"/> is null.</exception>
	public static async Task<SelfHealResult?> PerformComprehensiveHealAsync(this AnalyzerHealthCheck analyzer)
	{
		ArgumentNullException.ThrowIfNull(analyzer);
		var result = await analyzer.CheckHealthAsync();

		return result.Status == HealthStatus.Healthy
			? null
			: await analyzer.AttemptSelfHealAsync(result);
	}

	/// <summary>
	/// Gets a string representation of the health status report.
	/// </summary>
	/// <param name="analyzer">The health check instance.</param>
	/// <returns>A string report containing the health status and component details.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="analyzer"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when <see cref="AnalyzerHealthCheck.CheckHealthAsync"/> returns null.</exception>
	public static async Task<string> GetStatusReportAsync(this AnalyzerHealthCheck analyzer)
	{
		ArgumentNullException.ThrowIfNull(analyzer);
		var healthCheckResult = await analyzer.CheckHealthAsync();

		ArgumentNullException.ThrowIfNull(healthCheckResult, nameof(healthCheckResult));
		return healthCheckResult.ToString();
	}
}