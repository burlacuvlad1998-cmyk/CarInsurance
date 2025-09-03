using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace CarInsuranceIntegrationTests
{
	public class CarInsuranceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly HttpClient _client;

		public CarInsuranceIntegrationTests(WebApplicationFactory<Program> factory)
		{
			_client = factory.CreateClient();
		}

		[Fact]
		public async Task InsuranceClaim_ReturnBadRequest()
		{
			// Arrange
			long carId = 1;
			string invalidDate = "2023-13-01"; // Invalid month
			string description = "Accident";
			int amount = 5;

			// Act
			var response = await _client.PostAsync($"/api/cars/{carId}/claims?claimDate={invalidDate}&description={description}&amount={amount}", null);
			
			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var errorMessage = await response.Content.ReadAsStringAsync();
			Assert.Equal("Invalid date format. Use YYYY-MM-DD.", errorMessage.Trim('"'));
		}

		[Fact]
		public async Task InsuranceClaim_ReturnNotFound()
		{
			// Arrange
			long carId = 999; // Non-existent car ID
			string claimDate = "2023-10-01";
			string description = "Accident";
			int amount = 5;
			
			// Act
			var response = await _client.PostAsync($"/api/cars/{carId}/claims?claimDate={claimDate}&description={description}&amount={amount}", null);
			
			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task InsuranceClaim_ReturnOk()
		{
			// Arrange
			long carId = 1;
			string claimDate = "2023-10-01";
			string description = "Accident";
			int amount = 5;
			
			// Act
			var response = await _client.PostAsync($"/api/cars/{carId}/claims?claimDate={claimDate}&description={description}&amount={amount}", null);
			
			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task ClaimHistory_ReturnOk()
		{
			// Arrange
			long carId = 1;
			
			// Act
			var response = await _client.GetAsync($"/api/cars/{carId}/claims-history");
			
			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task ClaimHistory_ReturnNotFound()
		{
			// Arrange
			long carId = 999; // Non-existent car ID
			
			// Act
			var response = await _client.GetAsync($"/api/cars/{carId}/claims-history");
			
			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}
	}
}