using CarInsurance.Api.Dtos;
using CarInsurance.Api.Interfaces;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(ICarService service) : ControllerBase
{
    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

	// Task B, step 1: Register an insurance claim for a car
	[HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult<InsuranceClaimResponse>> InsuranceClaim(long carId, [FromQuery] string claimDate, string description, int amount)
    {
		// Task C step 2: Validate the input date format
		if (!DateOnly.TryParseExact(claimDate, "yyyy-MM-dd", out var parsed))
			return BadRequest("Invalid date format. Use YYYY-MM-DD.");
		try
		{
			await service.InsuranceClaim(carId, parsed, description, amount);
			return Ok(new InsuranceClaimResponse("Added succesfully"));
		}
		catch (KeyNotFoundException)
		{
			return NotFound();
		}
	}

	//Task B step 2: Get all insurance claims for a car
	[HttpGet("cars/{carId:long}/claims-history")]
	public async Task<ActionResult<ClaimHistoryResponse>> ClaimHistory(long carId)
	{
		try
		{
			var valid = await service.ClaimHistory(carId);
			valid = valid.OrderBy(c => c.StartDate).ToList();

			return Ok(new ClaimHistoryResponse(carId, valid.Select(c => c.StartDate).ToList(), valid.Select(c=>c.period).ToList()));
		}
		catch (KeyNotFoundException)
		{
			return NotFound();
		}
		catch (InvalidOperationException)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, "DB is probably empty or one of end date is empty"); // Internal Server Error
		}
	}
}
