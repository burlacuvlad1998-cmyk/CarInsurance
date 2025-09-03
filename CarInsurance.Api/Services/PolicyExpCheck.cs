using CarInsurance.Api.Dtos;
using CarInsurance.Api.Interfaces;

namespace CarInsurance.Api.Services;

public class PolicyExpirationCheck(ILogger<PolicyExpirationCheck> logger, IServiceProvider serviceProvider) : IHostedService
{
	private List<ExpiredPolicy> _expPolicies = new List<ExpiredPolicy>();
	private Timer? _timer = null;
	private List<long> loggedIds = new List<long>();

	public Task StartAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Policy Expiration Check Service is starting.");
		Console.WriteLine("Policy Expiration Check Service is starting.");
		_timer = new Timer(CheckExpirations, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
		return Task.CompletedTask;
	}

	private async void CheckExpirations(object? state)
	{
		using var scope = serviceProvider.CreateScope();
		var carService = scope.ServiceProvider.GetRequiredService<ICarService>();


		_expPolicies = await carService.GetExpiredPolicies(loggedIds);
		if (_expPolicies.Count == 0)
		{
			logger.LogInformation("No new expired policies found.");
		}
		else
		{
			foreach (var policy in _expPolicies)
			{
				logger.LogInformation($"Policy {policy.PolicyId} for car {policy.CarId} that started on {policy.StartDate} has expired on {policy.EndDate}.");
				loggedIds.Add(policy.PolicyId);
				Console.WriteLine($"Policy {policy.PolicyId} for car {policy.CarId} that started on {policy.StartDate} has expired on {policy.EndDate}.");
				loggedIds.Add(policy.PolicyId);
			}
			logger.LogInformation($"Policy expiration check completed. Found {_expPolicies.Count} expired policies.");
			Console.WriteLine($"Policy expiration check completed. Found {_expPolicies.Count} expired policies.");
		}
	}

	public Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Policy Expiration Check Service is stopping.");
		Console.WriteLine("Policy Expiration Check Service is stopping.");
		_timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}
}