using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Interfaces;
using CarInsurance.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db) : ICarService
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    // Task B, step 1: Register an insurance claim for a car
    public async Task InsuranceClaim(long carId, DateOnly parsed, string description, int amount)
    {
        // Task C step 1: Ensure the car exists
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        await _db.Policies.AddAsync(
            new InsurancePolicy
            {
                CarId = carId,
                Provider = description,
                StartDate = parsed,
                EndDate = parsed.AddDays(amount)
            }
        );
        await _db.SaveChangesAsync();
    }

    //Task B step 2: Get all insurance claims for a car
    public async Task<List<ClaimHist>> ClaimHistory(long carId)
    {
        // Task C step 1: Ensure the car exists
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");
        var policeDB = await _db.Policies.ToListAsync();
        try
        {
            return await _db.Policies.Where(c => c.CarId == carId)
                .Select(c => new ClaimHist(c.CarId, c.StartDate, c.EndDate, $"{c.EndDate.DayNumber - c.StartDate.DayNumber} days"))
                .ToListAsync();
        }
        catch
        {
            throw new InvalidOperationException("DB is probably empty or one of end date is Null");
        }
    }

	// Task D, step 1: Run a background service to check for expired policies
	public async Task<List<ExpiredPolicy>> GetExpiredPolicies(List<long> loggedIds)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            var candidates = await _db.Policies.Where(p => p.EndDate < today).ToListAsync();

			return candidates.Where(c => !loggedIds.Contains(c.Id) && ((now - c.EndDate.ToDateTime(TimeOnly.MinValue)).TotalHours >= 1))
                .Select(c => new ExpiredPolicy(c.Id, c.CarId, c.StartDate, c.EndDate))
                .ToList();
        }
        catch
        {
            throw new InvalidOperationException("DB is probably empty or one of end date is Null");
        }
    }
}

