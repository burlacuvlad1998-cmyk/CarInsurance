using CarInsurance.Api.Dtos;

namespace CarInsurance.Api.Interfaces
{
	public interface ICarService
	{
		Task<List<CarDto>> ListCarsAsync();

		Task<bool> IsInsuranceValidAsync(long carId, DateOnly date);

		Task InsuranceClaim(long carId, DateOnly parsed, string description, int amount);

		Task<List<ClaimHist>> ClaimHistory(long carId);

		Task<List<ExpiredPolicy>> GetExpiredPolicies(List<long> loggedIds);
	}
}