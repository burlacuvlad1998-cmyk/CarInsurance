namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
// Task B, step 1: Register an insurance claim for a car
public record InsuranceClaimResponse(string status);
// Task B step 2: Get all insurance claims for a car
public record ClaimHistoryResponse(long CarId, List<DateOnly> claimDate, List<string> Period);
public record ClaimHist(long Id, DateOnly StartDate, DateOnly EndDate, string period);
// Task D, step 1: Run a background service to check for expired policies
public record ExpiredPolicy(long PolicyId, long CarId, DateOnly StartDate, DateOnly EndDate);
