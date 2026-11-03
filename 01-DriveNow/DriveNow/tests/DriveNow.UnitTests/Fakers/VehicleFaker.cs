using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Fakers;

public class VehicleFaker : Faker<Vehicle>
{
    public VehicleFaker()
    {
        UseSeed(123);
        RuleFor(vehicle => vehicle.DailyRate,
            faker => faker.Random.Decimal(100, 2000));

        RuleFor(vehicle => vehicle.Category,
            faker => faker.PickRandom<VehicleCategory>());

        RuleFor(vehicle => vehicle.IsAvailable, true);

        RuleFor(vehicle => vehicle.Mileage,
            faker => faker.Random.Int(0, 200000));
    }
}