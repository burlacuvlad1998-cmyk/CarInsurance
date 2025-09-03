using CarInsurance.Api.Controllers;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;

namespace CarInsUniTest
{
	public class CarsControllerTests
	{
		private readonly AutoMocker Mocker = new AutoMocker();
		private readonly CarsController _controller;

		public CarsControllerTests()
		{
			_controller = Mocker.CreateInstance<CarsController>();
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
			var result = await _controller.InsuranceClaim(carId, invalidDate, description, amount);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
			Assert.Equal("Invalid date format. Use YYYY-MM-DD.", badRequestResult.Value);
		}

		[Fact]
		public async Task InsuranceClaim_ReturnNotFound()
		{
			// Arrange
			long carId = 999; // Non-existent car ID
			string claimDate = "2023-10-01";
			string description = "Accident";
			int amount = 5;
			Mocker.GetMock<ICarService>().Setup(s => s.InsuranceClaim(carId, It.IsAny<DateOnly>(), description, amount))
				.ThrowsAsync(new KeyNotFoundException());

			// Act
			var result = await _controller.InsuranceClaim(carId, claimDate, description, amount);

			// Assert
			Assert.IsType<NotFoundResult>(result.Result);
		}

		[Fact]
		public async Task InsuranceClaim_ReturnOk()
		{
			// Arrange
			long carId = 1;
			string claimDate = "2023-10-01";
			string description = "Accident";
			int amount = 5;
			Mocker.GetMock<ICarService>().Setup(s => s.InsuranceClaim(carId, It.IsAny<DateOnly>(), description, amount))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.InsuranceClaim(carId, claimDate, description, amount);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result.Result);
			var response = Assert.IsType<InsuranceClaimResponse>(okResult.Value);
			Assert.Equal("Added succesfully", response.status);
		}

		[Fact]
		public async Task ClaimHistory_ReturnNotFound()
		{
			// Arrange
			long carId = 999; // Non-existent car ID
			Mocker.GetMock<ICarService>().Setup(s => s.ClaimHistory(carId))
				.ThrowsAsync(new KeyNotFoundException());
			
			// Act
			var result = await _controller.ClaimHistory(carId);
			
			// Assert
			Assert.IsType<NotFoundResult>(result.Result);
		}

		[Fact]
		public async Task ClaimHistory_ReturnOk()
		{
			// Arrange
			long carId = 1;
			var claimHistories = new List<ClaimHist>();
			Mocker.GetMock<ICarService>().Setup(s => s.ClaimHistory(carId))
				.ReturnsAsync(claimHistories);
			
			// Act
			var result = await _controller.ClaimHistory(carId);
			
			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result.Result);
			var response = Assert.IsType<ClaimHistoryResponse>(okResult.Value);
			Assert.Equal(carId, response.CarId);
		}

	}
}